using System;
using System.Text.RegularExpressions;

namespace SCPSLCNCBan_LabAPI.Utils
{
    /// <summary>
    /// 网络和IP相关工具类
    /// </summary>
    public static class NetworkUtils
    {
        /// <summary>
        /// 检查IP地址是否匹配指定的模式（支持通配符）
        /// </summary>
        /// <param name="pattern">IP模式，支持通配符*</param>
        /// <param name="ipAddress">要检查的IP地址</param>
        /// <returns>是否匹配</returns>
        public static bool IpMatch(string pattern, string ipAddress)
        {
            try
            {
                // 处理CNBan的IP段格式，如"14.204.182.*_1"或"101.18.215.114_1"
                if (pattern.Contains("_"))
                {
                    pattern = pattern.Split('_')[0];
                }

                // 将通配符转换为正则表达式
                string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                return Regex.IsMatch(ipAddress, regexPattern);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
