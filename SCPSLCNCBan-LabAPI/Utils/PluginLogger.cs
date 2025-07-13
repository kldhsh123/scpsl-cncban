using LabApi.Features.Console;
using SCPSLCNCBan_LabAPI.Models;

namespace SCPSLCNCBan_LabAPI.Utils
{
    /// <summary>
    /// 插件专用日志工具类
    /// </summary>
    public static class PluginLogger
    {
        private static Config _config;

        /// <summary>
        /// 初始化日志工具
        /// </summary>
        /// <param name="config">插件配置</param>
        public static void Initialize(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// 输出信息日志
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void Info(string message)
        {
            Logger.Info($"[SCPSLCNCBan] {message}");
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void Error(string message)
        {
            Logger.Error($"[SCPSLCNCBan] {message}");
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void Warn(string message)
        {
            Logger.Warn($"[SCPSLCNCBan] {message}");
        }

        /// <summary>
        /// 输出调试日志（受配置控制）
        /// </summary>
        /// <param name="message">消息内容</param>
        public static void Debug(string message)
        {
            if (_config?.Debug == true)
            {
                Logger.Info($"[SCPSLCNCBan-DEBUG] {message}");
            }
        }

        /// <summary>
        /// 输出详细调试日志（受配置控制）
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="category">调试分类</param>
        public static void DebugVerbose(string message, string category = "GENERAL")
        {
            if (_config?.Debug == true)
            {
                Logger.Info($"[SCPSLCNCBan-DEBUG-{category}] {message}");
            }
        }
    }
}
