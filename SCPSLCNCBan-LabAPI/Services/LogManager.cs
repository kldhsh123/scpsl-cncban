using System;
using System.Collections.Generic;
using System.IO;
using SCPSLCNCBan_LabAPI.Models;
using SCPSLCNCBan_LabAPI.Utils;

namespace SCPSLCNCBan_LabAPI.Services
{
    /// <summary>
    /// 日志管理服务
    /// </summary>
    public class LogManager
    {
        private readonly Config _config;
        private readonly string _banLogPath;
        private readonly HashSet<string> _loggedPlayersThisRound = new HashSet<string>();

        public LogManager(Config config, string banLogPath)
        {
            _config = config;
            _banLogPath = banLogPath;
        }

        /// <summary>
        /// 记录封禁日志
        /// </summary>
        /// <param name="playerName">玩家昵称</param>
        /// <param name="steamId">Steam ID</param>
        /// <param name="ip">IP地址</param>
        /// <param name="isIdBanned">是否ID被封禁</param>
        /// <param name="isIpBanned">是否IP被封禁</param>
        /// <param name="ban">封禁信息</param>
        /// <param name="port">端口号</param>
        public void LogBan(string playerName, string steamId, string ip, bool isIdBanned, bool isIpBanned, LegacyBanInfo ban, ushort port = 0)
        {
            try
            {
                // 日志冷却检查（如果启用）
                if (_config.EnableLogCooldown)
                {
                    // 创建玩家唯一标识符用于日志冷却
                    string playerIdentifier = $"{steamId}_{ip}";

                    // 检查是否已在当前回合记录过此玩家的封禁日志
                    if (_loggedPlayersThisRound.Contains(playerIdentifier))
                    {
                        PluginLogger.DebugVerbose($"玩家 {playerName} ({playerIdentifier}) 在当前回合已记录过封禁日志，跳过重复记录", "LOG");
                        return;
                    }

                    // 添加到已记录列表
                    _loggedPlayersThisRound.Add(playerIdentifier);
                }

                string banType = isIdBanned && isIpBanned ? "ID和IP" : (isIdBanned ? "ID" : (isIpBanned ? "IP" : "敏感词"));
                string source = ban.Type == "keyword" ? "敏感词检测" : (ban.Reason == "CNBan封禁" ? "CNBan" : "CNCBAN");
                string portInfo = port > 0 ? $", 端口: {port}" : "";
                string rangeInfo = !string.IsNullOrEmpty(ban.Range) ? $", IP段: {ban.Range}" : "";
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] 阻止玩家 {playerName} (SteamID: {steamId}, IP: {ip}{portInfo}{rangeInfo}) 连接服务器，{banType}已被封禁，原因: {ban.Reason}";

                File.AppendAllText(_banLogPath, logEntry + Environment.NewLine);
                PluginLogger.Info(logEntry);

                if (_config.EnableLogCooldown)
                {
                    string playerIdentifier = $"{steamId}_{ip}";
                    PluginLogger.DebugVerbose($"已记录玩家 {playerName} ({playerIdentifier}) 的封禁日志，当前回合已记录 {_loggedPlayersThisRound.Count} 个玩家", "LOG");
                }
            }
            catch (Exception e)
            {
                PluginLogger.Error($"写入封禁日志失败: {e}");
            }
        }

        /// <summary>
        /// 清理回合日志冷却
        /// </summary>
        public void ClearRoundLogCooldown()
        {
            _loggedPlayersThisRound.Clear();
            PluginLogger.Debug("回合结束，已清理封禁日志冷却列表");
        }

        /// <summary>
        /// 确保日志目录存在
        /// </summary>
        public void EnsureLogDirectoryExists()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_banLogPath));
            }
            catch (Exception e)
            {
                PluginLogger.Error($"创建日志目录失败: {e}");
            }
        }
    }
}
