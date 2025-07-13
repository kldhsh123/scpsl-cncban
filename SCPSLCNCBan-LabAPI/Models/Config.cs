using System;
using System.ComponentModel;
using System.IO;

namespace SCPSLCNCBan_LabAPI.Models
{
    public class Config
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;

        [Description("是否开启调试模式")]
        public bool Debug { get; set; } = false;

        [Description("检查间隔")]
        public float CheckInterval { get; set; } = 10f;

        [Description("是否启用IP封禁检查")]
        public bool EnableIpBan { get; set; } = true;
        
        [Description("是否启用CNBan IP段封禁检查")]
        public bool EnableCNBanIpBan { get; set; } = true;
        
        [Description("是否启用CNCBAN IP段封禁检查")]
        public bool EnableIpRangeBan { get; set; } = true;
        
        [Description("封禁日志路径")]
        public string BanLogPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory", "CNCBAN-BansLog.txt");
        
        [Description("敏感词列表路径")]
        public string KeywordsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCP Secret Laboratory", "cncban_keywords.txt");
        
        [Description("是否启用云端敏感词词阻止加入(芒辉API提供)")]
        public bool EnableCloudKeywords { get; set; } = true;
        
        [Description("是否同步封禁CNBan玩家")]
        public bool SyncCNBanPlayers { get; set; } = true;

        [Description("是否启用日志冷却(防止同一玩家在当前回合重复记录封禁日志)")]
        public bool EnableLogCooldown { get; set; } = true;
    }
}
