namespace SCPSLCNCBan_LabAPI.Constants
{
    /// <summary>
    /// API相关常量
    /// </summary>
    public static class ApiConstants
    {
        /// <summary>
        /// API根URL
        /// </summary>
        public const string API_ROOT_URL = "http://api.kldhsh.top/SCPSLCNCBan";

        /// <summary>
        /// API密钥
        /// </summary>
        public const string API_KEY = "sjsJS023";

        /// <summary>
        /// 是否在顶部显示额外消息
        /// </summary>
        public const bool SHOW_ADDITIONAL_MESSAGE_AT_TOP = true;

        /// <summary>
        /// CNBan关键词API URL
        /// </summary>
        public const string CNBAN_KEYWORDS_URL = "https://api.manghui.net/t/keywords.html";

        /// <summary>
        /// CNBan封禁列表API URL模板
        /// https://api.manghui.net/t/getbanlist?time={0}&version=1
        /// </summary>
        public const string CNBAN_BANLIST_URL_TEMPLATE = "https://api.manghui.net/t/getbanlist?time={0}&version=1";

        /// <summary>
        /// 刷新间隔（毫秒）
        /// </summary>
        public const int REFRESH_INTERVAL_MS = 600000; // 10分钟

        /// <summary>
        /// 关键词刷新间隔（分钟）
        /// </summary>
        public const int KEYWORDS_REFRESH_INTERVAL_MINUTES = 1440; // 24小时
    }
}
