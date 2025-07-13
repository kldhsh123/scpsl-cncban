using System;
using System.Collections.Generic;
using System.Linq;
using SCPSLCNCBan_LabAPI.Constants;
using SCPSLCNCBan_LabAPI.Models;
using SCPSLCNCBan_LabAPI.Utils;

namespace SCPSLCNCBan_LabAPI.Services
{
    /// <summary>
    /// 封禁检查服务
    /// </summary>
    public class BanChecker
    {
        private readonly Config _config;

        public BanChecker(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// 检查敏感词
        /// </summary>
        /// <param name="playerName">玩家昵称</param>
        /// <param name="keywords">敏感词列表</param>
        /// <returns>匹配的敏感词，如果没有匹配返回null</returns>
        public string CheckKeywords(string playerName, List<string> keywords)
        {
            var nickName = playerName?.ToLower();
            if (string.IsNullOrEmpty(nickName))
                return null;

            PluginLogger.DebugVerbose($"正在检查玩家 {playerName} 的昵称是否包含敏感词，当前敏感词列表包含 {keywords.Count} 个词", "KEYWORD");

            string skey = keywords.FirstOrDefault(key => nickName.Contains(key));
            if (skey != null)
            {
                PluginLogger.DebugVerbose($"玩家 {playerName} 的昵称中包含敏感词: {skey}", "KEYWORD");
            }

            return skey;
        }

        /// <summary>
        /// 检查CNBan封禁
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="ip">IP地址</param>
        /// <param name="cnBanList">CNBan封禁列表</param>
        /// <returns>封禁信息</returns>
        public (bool isIdBanned, bool isIpBanned, CNBanInfo idBan, CNBanInfo ipBan) CheckCNBan(string userId, string ip, List<CNBanInfo> cnBanList)
        {
            bool isCNBanIdBanned = false;
            bool isCNBanIpBanned = false;
            CNBanInfo idBan = null;
            CNBanInfo ipBan = null;

            PluginLogger.DebugVerbose($"检查CNBan: userId='{userId}', ip='{ip}', 列表中有{cnBanList.Count}条记录", "CNBAN");

            // 检查SteamID是否被封禁
            // 尝试精确匹配
            idBan = cnBanList.FirstOrDefault(p => p.type == 0 && p.value == userId);

            // 如果精确匹配失败，尝试匹配带@steam后缀的格式
            if (idBan == null)
            {
                idBan = cnBanList.FirstOrDefault(p => p.type == 0 && p.value == $"{userId}@steam");
            }

            // 如果还是没匹配到，尝试从列表中的完整格式提取纯ID进行匹配
            if (idBan == null)
            {
                idBan = cnBanList.FirstOrDefault(p => p.type == 0 && PlatformUtils.GetCleanUserId(p.value) == userId);
            }

            if (idBan != null)
            {
                isCNBanIdBanned = true;
                PluginLogger.DebugVerbose($"CNBan ID匹配: {userId} 匹配到 {idBan.value}", "CNBAN");
            }
            else
            {
                PluginLogger.DebugVerbose($"CNBan ID未匹配: {userId}", "CNBAN");
                // 显示前几个ID记录用于调试
                var idRecords = cnBanList.Where(p => p.type == 0).Take(3).Select(p => p.value);
                PluginLogger.DebugVerbose($"列表中前3个ID记录: {string.Join(", ", idRecords)}", "CNBAN");
            }

            // 检查IP是否被封禁（如果启用了IP检查）
            if (_config.EnableCNBanIpBan)
            {
                ipBan = cnBanList.FirstOrDefault(p => p.type == 1 && NetworkUtils.IpMatch(p.value, ip));
                if (ipBan != null)
                {
                    isCNBanIpBanned = true;
                    PluginLogger.DebugVerbose($"CNBan IP匹配: {ip} 匹配到 {ipBan.value}", "CNBAN");
                }
            }

            return (isCNBanIdBanned, isCNBanIpBanned, idBan, ipBan);
        }

        /// <summary>
        /// 检查CNCBAN封禁
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="ip">IP地址</param>
        /// <param name="banList">封禁列表</param>
        /// <returns>封禁信息</returns>
        public (bool isIdBanned, bool isIpBanned, BanInfo activeBan) CheckCNCBan(string userId, string ip, List<BanInfo> banList)
        {
            bool isIpBanned = false;
            bool isIdBanned = false;
            BanInfo activeBan = null;

            foreach (BanInfo ban in banList)
            {
                string cleanPlayerUserId = PlatformUtils.GetCleanUserId(userId);

                // 检查Steam ID是否被封禁 (m=0表示Steam)
                if (ban.m == 0 && !PlatformUtils.IsDiscordUser(userId) && cleanPlayerUserId == ban.v)
                {
                    isIdBanned = true;
                    activeBan = ban;
                }
                // 检查Discord ID是否被封禁 (m=1表示Discord)
                else if (ban.m == 1 && PlatformUtils.IsDiscordUser(userId) && cleanPlayerUserId == ban.v)
                {
                    isIdBanned = true;
                    activeBan = ban;
                }
                // 检查IP段是否被封禁（如果启用了IP检查且有IP段信息）
                else if (_config.EnableIpBan && !string.IsNullOrEmpty(ban.g) && NetworkUtils.IpMatch(ban.g, ip))
                {
                    isIpBanned = true;
                    if (!isIdBanned) // 如果ID没有被封禁，使用IP的封禁信息
                        activeBan = ban;
                }
            }

            return (isIdBanned, isIpBanned, activeBan);
        }

        /// <summary>
        /// 创建踢出消息
        /// </summary>
        /// <param name="banReason">封禁理由</param>
        /// <param name="userPlatform">用户平台</param>
        /// <param name="isIdBanned">是否ID被封禁</param>
        /// <param name="isIpBanned">是否IP被封禁</param>
        /// <param name="messageType">消息类型（CNCBAN或CNBan）</param>
        /// <returns>踢出消息</returns>
        public string CreateKickMessage(string banReason, string userPlatform, bool isIdBanned, bool isIpBanned, string messageType = "CNCBAN")
        {
            DateTime currentTime = DateTime.Now;

            // 创建一个替换参数的函数
            string ReplaceParameters(string text)
            {
                return text
                    .Replace("{reason}", banReason)
                    .Replace("{starttime}", currentTime.ToString("yyyy年M月d日H时m分s秒"))
                    .Replace("{bantype}", isIdBanned ? userPlatform : "IP")
                    .Replace("{platform}", userPlatform);
            }

            string baseMessage = ReplaceParameters(Messages.KICK_MESSAGE);
            string additionalMessage = "";

            // 获取额外的提示信息
            if (isIpBanned && isIdBanned)
                additionalMessage = ReplaceParameters(Messages.BOTH_BANNED_MESSAGE);
            else if (isIpBanned)
                additionalMessage = ReplaceParameters(Messages.IP_BANNED_MESSAGE);
            else if (isIdBanned)
                additionalMessage = ReplaceParameters(Messages.ID_BANNED_MESSAGE);

            // 根据配置决定额外信息的位置
            string finalMessage = ApiConstants.SHOW_ADDITIONAL_MESSAGE_AT_TOP
                ? $"\n[{messageType}]社区联盟封禁\n{additionalMessage}\n{baseMessage}"
                : $"[{messageType}]社区联盟封禁\n{baseMessage}";

            return finalMessage;
        }

        /// <summary>
        /// 创建CNBan踢出消息
        /// </summary>
        /// <param name="isIdBanned">是否ID被封禁</param>
        /// <param name="isIpBanned">是否IP被封禁</param>
        /// <param name="banCode">封禁代码/备注</param>
        /// <returns>踢出消息</returns>
        public string CreateCNBanKickMessage(bool isIdBanned, bool isIpBanned, string banCode = "")
        {
            // 构建封禁原因消息，包含代码信息
            string reasonMessage = "封禁原因: 您已被CNBan封禁";
            if (!string.IsNullOrEmpty(banCode))
            {
                reasonMessage += $" (代码: {banCode})";
            }

            string baseMessage = $"{reasonMessage}\n您可能在游戏内外使用了包括但不限于外挂，恶意BUG等非法手段，遭到举报并核实后封禁，永久无法加入任何国服联盟封禁的服务器";

            // 获取额外的提示信息
            string additionalMessage;

            if (isIdBanned && isIpBanned)
                additionalMessage = "您的steam账号和IP地址均已被永久封禁\n如有疑问或申诉请访问: ac.cnscpsl.cn";
            else if (isIpBanned)
                additionalMessage = "您的ip已被永久封禁\n如有疑问或申诉请访问: ac.cnscpsl.cn\n如果您正在使用加速器，请尝试关闭加速器或更换节点";
            else
                additionalMessage = "您的steam账号已被永久封禁\n如有疑问或申诉请访问: ac.cnscpsl.cn";

            // 根据配置决定额外信息的位置
            string finalMessage = ApiConstants.SHOW_ADDITIONAL_MESSAGE_AT_TOP
                ? $"\n[CNBan]社区封禁\n{additionalMessage}\n{baseMessage}"
                : $"[CNBan]社区封禁\n{baseMessage}";

            return finalMessage;
        }
    }
}
