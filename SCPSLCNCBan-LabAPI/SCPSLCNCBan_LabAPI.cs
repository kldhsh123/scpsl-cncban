using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LabApi.Loader.Features.Plugins;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using SCPSLCNCBan_LabAPI.Models;
using SCPSLCNCBan_LabAPI.Services;
using SCPSLCNCBan_LabAPI.Utils;
using SCPSLCNCBan_LabAPI.Constants;

namespace SCPSLCNCBan_LabAPI
{
    /// <summary>
    /// SCPSL中国社区联盟封禁系统主插件类
    /// </summary>
    public class SCPSLCNCBan : Plugin<Config>
    {
        public override string Name => "SCPSLCNCBan-LabAPI";
        public override string Author => "kldhsh123";
        public override Version Version => new Version(1, 3, 0);
        public override string Description => "SCPSL中国社区联盟封禁系统";
        public override Version RequiredApiVersion => new Version(1, 1, 0);

        // 服务实例
        private BanApiService _banApiService;
        private CNBanService _cnBanService;
        private BanChecker _banChecker;
        private LogManager _logManager;
        
        // 定时器
        private Timer _refreshTimer;
        
        // 数据缓存
        private List<BanInfo> _banList = new List<BanInfo>();
        private List<string> _banReasons = new List<string>();

        public override void Enable()
        {
            try
            {
                // 初始化日志工具
                PluginLogger.Initialize(Config);

                // 初始化服务
                InitializeServices();

                // 注册事件
                RegisterEvents();

                // 启动服务
                StartServices();

                PluginLogger.Info($"{Name} v{Version} 已加载");
                PluginLogger.Debug($"调试模式已启用，将显示详细日志信息");
            }
            catch (Exception ex)
            {
                PluginLogger.Error($"插件启用失败: {ex}");
            }
        }

        public override void Disable()
        {
            try
            {
                // 注销事件
                UnregisterEvents();

                // 停止服务
                StopServices();

                PluginLogger.Info($"{Name} 已卸载");
            }
            catch (Exception ex)
            {
                PluginLogger.Error($"插件禁用失败: {ex}");
            }
        }

        /// <summary>
        /// 初始化所有服务
        /// </summary>
        private void InitializeServices()
        {
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(Config.BanLogPath));
            Directory.CreateDirectory(Path.GetDirectoryName(Config.KeywordsPath));

            // 初始化服务
            _banApiService = new BanApiService();
            _cnBanService = new CNBanService(Config, Config.KeywordsPath);
            _banChecker = new BanChecker(Config);
            _logManager = new LogManager(Config, Config.BanLogPath);

            // 初始化敏感词
            _cnBanService.InitializeKeywords();
            
            // 确保日志目录存在
            _logManager.EnsureLogDirectoryExists();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PlayerEvents.PreAuthenticating += OnPlayerPreAuthenticating;
            PlayerEvents.Joined += OnPlayerJoined;
            ServerEvents.RoundEnded += OnRoundEnded;
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        private void UnregisterEvents()
        {
            PlayerEvents.PreAuthenticating -= OnPlayerPreAuthenticating;
            PlayerEvents.Joined -= OnPlayerJoined;
            ServerEvents.RoundEnded -= OnRoundEnded;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        private void StartServices()
        {
            // 启动CNBan下载线程
            _cnBanService.StartCNBanDownloader();

            // 初始刷新并设置定时刷新
            RefreshBanList();
            _refreshTimer = new Timer(state => RefreshBanList(), null, 
                ApiConstants.REFRESH_INTERVAL_MS, ApiConstants.REFRESH_INTERVAL_MS);
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private void StopServices()
        {
            // 停止CNBan下载线程
            _cnBanService?.StopCNBanDownloader();
            
            // 释放定时器
            _refreshTimer?.Dispose();
        }

        /// <summary>
        /// 刷新封禁列表
        /// </summary>
        private async void RefreshBanList()
        {
            var result = await _banApiService.GetBanListAsync();
            if (result.banList != null && result.banReasons != null)
            {
                _banList = result.banList;
                _banReasons = result.banReasons;
            }
        }

        /// <summary>
        /// 玩家预认证事件处理
        /// </summary>
        private void OnPlayerPreAuthenticating(PlayerPreAuthenticatingEventArgs ev)
        {
            try
            {
                string steamId = ev.UserId;
                string ip = ev.IpAddress;

                PluginLogger.DebugVerbose($"检查玩家预认证: ID={steamId}, IP={ip}", "AUTH");

                // 在预认证阶段，我们只能检查ID和IP相关的封禁
                // 敏感词检查需要在玩家加入后进行（因为需要昵称信息）

                PluginLogger.DebugVerbose($"玩家 {steamId} 通过预认证检查", "AUTH");
            }
            catch (Exception e)
            {
                PluginLogger.Error($"检查玩家预认证时出错: {e}");
            }
        }

        /// <summary>
        /// 玩家加入事件处理
        /// </summary>
        private void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            try
            {
                Player player = ev.Player;
                string playerName = player.Nickname;
                string steamId = player.UserId;
                string ip = player.IpAddress;

                PluginLogger.DebugVerbose($"检查玩家加入: 昵称={playerName}, ID={steamId}, IP={ip}", "JOIN");

                // 检查敏感词（现在在这里检查，因为有昵称信息）
                string keyword = _banChecker.CheckKeywords(playerName, _cnBanService.Keywords);
                if (keyword != null)
                {
                    PluginLogger.Debug($"玩家 {playerName} 昵称包含敏感词: {keyword}，踢出玩家");
                    HandleKeywordKick(player, playerName, steamId, ip, keyword);
                    return;
                }

                // 检查CNBan封禁
                if (Config.SyncCNBanPlayers)
                {
                    string cleanSteamId = PlatformUtils.GetCleanUserId(steamId);
                    var cnBanResult = _banChecker.CheckCNBan(cleanSteamId, ip, _cnBanService.CNBanList);
                    if (cnBanResult.isIdBanned || cnBanResult.isIpBanned)
                    {
                        PluginLogger.Debug($"玩家 {playerName} 被CNBan封禁，踢出玩家");
                        HandleCNBanKick(player, playerName, steamId, ip, cnBanResult);
                        return;
                    }
                }

                // 检查CNCBAN封禁
                var cncBanResult = _banChecker.CheckCNCBan(steamId, ip, _banList);
                if (cncBanResult.isIdBanned || (cncBanResult.isIpBanned && Config.EnableIpBan))
                {
                    PluginLogger.Debug($"玩家 {playerName} 被CNCBAN封禁，踢出玩家");
                    HandleCNCBanKick(player, playerName, steamId, ip, cncBanResult);
                    return;
                }

                PluginLogger.DebugVerbose($"玩家 {playerName} 通过所有检查", "JOIN");
            }
            catch (Exception e)
            {
                PluginLogger.Error($"检查已加入玩家时出错: {e}");
            }
        }

        /// <summary>
        /// 回合结束事件处理
        /// </summary>
        private void OnRoundEnded(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            _logManager.ClearRoundLogCooldown();
        }

        /// <summary>
        /// 处理敏感词踢出
        /// </summary>
        private void HandleKeywordKick(Player player, string playerName, string steamId, string ip, string keyword)
        {
            var legacyBan = new LegacyBanInfo
            {
                Type = "keyword",
                Value = steamId,
                Reason = $"包含违禁词: {keyword}",
                StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Time = 0
            };

            _logManager.LogBan(playerName, steamId, ip, false, false, legacyBan);

            string platform = PlatformUtils.GetUserPlatform(steamId);
            string message = Messages.KEYWORD_BANNED_MESSAGE.Replace("{platform}", platform);
            player.Kick(message);

            PluginLogger.Info($"玩家 {playerName} 加入游戏被拦截，触发敏感词：{keyword}");
        }

        /// <summary>
        /// 处理CNBan踢出
        /// </summary>
        private void HandleCNBanKick(Player player, string playerName, string steamId, string ip,
            (bool isIdBanned, bool isIpBanned, CNBanInfo idBan, CNBanInfo ipBan) result)
        {
            string banType = result.isIdBanned && result.isIpBanned ? "both" :
                           (result.isIdBanned ? "steamid" : "ip");
            string banValue = result.isIdBanned && result.isIpBanned ? $"{steamId}|{ip}" :
                            (result.isIdBanned ? steamId : ip);

            // 获取封禁备注信息
            string banReason = "CNBan封禁";
            string banCode = "";

            if (result.isIdBanned && !string.IsNullOrEmpty(result.idBan?.reason))
            {
                banCode = result.idBan.reason;
                banReason = $"CNBan封禁 (代码: {banCode})";
            }
            else if (result.isIpBanned && !string.IsNullOrEmpty(result.ipBan?.reason))
            {
                banCode = result.ipBan.reason;
                banReason = $"CNBan封禁 (代码: {banCode})";
            }

            var legacyBan = new LegacyBanInfo
            {
                Type = banType,
                Value = banValue,
                Reason = banReason,
                StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Time = 0
            };

            string kickMessage = _banChecker.CreateCNBanKickMessage(result.isIdBanned, result.isIpBanned, banCode);
            player.Kick(kickMessage);

            _logManager.LogBan(playerName, PlatformUtils.GetCleanUserId(steamId), ip,
                result.isIdBanned, result.isIpBanned, legacyBan);

            string logMessage = $"玩家 {playerName} 因CNBan封禁被踢出游戏";
            if (!string.IsNullOrEmpty(banCode))
            {
                logMessage += $" (代码: {banCode})";
            }
            PluginLogger.Info(logMessage);
        }

        /// <summary>
        /// 处理CNCBAN踢出
        /// </summary>
        private void HandleCNCBanKick(Player player, string playerName, string steamId, string ip,
            (bool isIdBanned, bool isIpBanned, BanInfo activeBan) result)
        {
            // 获取封禁理由
            string banReason = "未知原因";
            if (result.activeBan.r >= 0 && result.activeBan.r < _banReasons.Count)
            {
                banReason = _banReasons[result.activeBan.r];
            }

            string userPlatform = PlatformUtils.GetUserPlatform(steamId);
            string kickMessage = _banChecker.CreateKickMessage(banReason, userPlatform, 
                result.isIdBanned, result.isIpBanned);

            player.Kick(kickMessage);

            // 创建日志记录
            string banType = result.isIdBanned ? 
                (PlatformUtils.IsDiscordUser(steamId) ? "discordid" : "steamid") : "ip";

            var legacyBan = new LegacyBanInfo
            {
                Type = banType,
                Value = result.isIdBanned ? result.activeBan.v : (result.activeBan.g ?? ip),
                Reason = banReason,
                StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Time = 0,
                Range = result.activeBan.g
            };

            _logManager.LogBan(playerName, PlatformUtils.GetCleanUserId(steamId), ip,
                result.isIdBanned, result.isIpBanned, legacyBan);

            PluginLogger.Info($"玩家 {playerName} 因CNCBAN封禁被踢出游戏");
        }
    }
}
