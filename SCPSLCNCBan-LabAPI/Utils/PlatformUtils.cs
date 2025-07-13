namespace SCPSLCNCBan_LabAPI.Utils
{
    /// <summary>
    /// 平台识别工具类
    /// </summary>
    public static class PlatformUtils
    {
        /// <summary>
        /// 获取去除平台后缀的纯用户ID
        /// </summary>
        /// <param name="userId">包含平台后缀的用户ID</param>
        /// <returns>纯用户ID</returns>
        public static string GetCleanUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return userId;

            return userId.Replace("@steam", "").Replace("@discord", "");
        }

        /// <summary>
        /// 获取用户平台类型
        /// </summary>
        /// <param name="userId">包含平台后缀的用户ID</param>
        /// <returns>平台名称</returns>
        public static string GetUserPlatform(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return "Unknown";

            if (userId.Contains("@steam"))
                return "Steam";
            else if (userId.Contains("@discord"))
                return "Discord";
            else
                return "Unknown";
        }

        /// <summary>
        /// 判断用户是否为Discord用户
        /// </summary>
        /// <param name="userId">包含平台后缀的用户ID</param>
        /// <returns>是否为Discord用户</returns>
        public static bool IsDiscordUser(string userId)
        {
            return !string.IsNullOrEmpty(userId) && userId.Contains("@discord");
        }
    }
}
