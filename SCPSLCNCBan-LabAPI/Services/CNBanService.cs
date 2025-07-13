using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using SCPSLCNCBan_LabAPI.Constants;
using SCPSLCNCBan_LabAPI.Models;
using SCPSLCNCBan_LabAPI.Utils;

namespace SCPSLCNCBan_LabAPI.Services
{
    /// <summary>
    /// CNBan数据服务
    /// </summary>
    public class CNBanService
    {
        private readonly Config _config;
        private readonly string _keywordsPath;
        private List<CNBanInfo> _cnBanList = new List<CNBanInfo>();
        private List<string> _keywords = new List<string>();
        private Thread _cnBanDownloadThread;
        private long _cnBanTimestamp = -1;

        public CNBanService(Config config, string keywordsPath)
        {
            _config = config;
            _keywordsPath = keywordsPath;
        }

        /// <summary>
        /// 获取CNBan封禁列表
        /// </summary>
        public List<CNBanInfo> CNBanList => _cnBanList;

        /// <summary>
        /// 获取敏感词列表
        /// </summary>
        public List<string> Keywords => _keywords;

        /// <summary>
        /// 初始化敏感词列表
        /// </summary>
        public void InitializeKeywords()
        {
            try
            {
                if (!File.Exists(_keywordsPath))
                {
                    File.WriteAllText(_keywordsPath, DefaultKeywords.Keywords);
                    PluginLogger.Info($"已创建默认敏感词配置文件: {_keywordsPath}");
                }

                _keywords = ConvertKeywords(File.ReadAllText(_keywordsPath));
                PluginLogger.Info($"自定义阻止加入敏感词列表已加载，共{_keywords.Count}个敏感词");

                if (_keywords.Count > 0)
                {
                    PluginLogger.Debug($"敏感词列表内容: {string.Join(", ", _keywords)}");
                }
            }
            catch (Exception e)
            {
                PluginLogger.Error($"加载敏感词列表失败: {e}");
                _keywords = new List<string>();
            }
        }

        /// <summary>
        /// 启动CNBan下载线程
        /// </summary>
        public void StartCNBanDownloader()
        {
            _cnBanDownloadThread = new Thread(new ThreadStart(CNBanListDownloader))
            {
                Priority = ThreadPriority.Lowest,
                IsBackground = true,
                Name = "SCPSL: CNBanList Downloader"
            };
            _cnBanDownloadThread.Start();
        }

        /// <summary>
        /// 停止CNBan下载线程
        /// </summary>
        public void StopCNBanDownloader()
        {
            if (_cnBanDownloadThread != null && _cnBanDownloadThread.IsAlive)
            {
                _cnBanDownloadThread.Abort();
            }
        }

        /// <summary>
        /// 转换敏感词字符串为列表
        /// </summary>
        private List<string> ConvertKeywords(string str)
        {
            return str.ToLower()
                .Replace("\r", "")
                .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !p.StartsWith("#"))
                .ToList();
        }

        /// <summary>
        /// CNBan列表下载器
        /// </summary>
        private void CNBanListDownloader()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                _cnBanList = new List<CNBanInfo>();
                int counter = (_config.EnableCloudKeywords ? ApiConstants.KEYWORDS_REFRESH_INTERVAL_MINUTES : 0);
                
                while (true)
                {
                    // 下载云端敏感词
                    if (counter >= ApiConstants.KEYWORDS_REFRESH_INTERVAL_MINUTES)
                    { 
                        DownloadCloudKeywords(httpClient);
                        counter = 1;
                    }

                    // 下载CNBan封禁列表
                    DownloadCNBanList(httpClient);

                    Thread.Sleep(ApiConstants.REFRESH_INTERVAL_MS);

                    if (_config.EnableCloudKeywords)
                        counter++;
                }
            }
            catch (ThreadAbortException) { }
        }

        /// <summary>
        /// 下载云端敏感词
        /// </summary>
        private void DownloadCloudKeywords(HttpClient httpClient)
        {
            try
            {
                HttpResponseMessage response = httpClient.GetAsync(ApiConstants.CNBAN_KEYWORDS_URL).Result;
                HttpStatusCode statusCode = response.StatusCode;
                if (statusCode == HttpStatusCode.OK)
                {
                    var raw = response.Content.ReadAsStringAsync().Result;
                    var kwList = ConvertKeywords(File.ReadAllText(_keywordsPath));
                    var cloudKeywords = ConvertKeywords(Encoding.UTF8.GetString(Convert.FromBase64String(raw)));
                    kwList.AddRange(cloudKeywords);
                    
                    // 更新全局敏感词列表
                    _keywords = kwList;

                    PluginLogger.Info($"云端阻止加入敏感词列表已更新，云端敏感词数量: {cloudKeywords.Count}，总数: {_keywords.Count}");
                    PluginLogger.Debug($"敏感词列表内容:内容涉嫌违规，仅在开发者模式展示");
                }
            }
            catch (Exception ex)
            {
                PluginLogger.Error($"下载敏感词失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 下载CNBan封禁列表
        /// </summary>
        private void DownloadCNBanList(HttpClient httpClient)
        {
            try
            {
                string url = string.Format(ApiConstants.CNBAN_BANLIST_URL_TEMPLATE, _cnBanTimestamp);
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                HttpStatusCode statusCode = response.StatusCode;
                if (statusCode == HttpStatusCode.OK)
                {
                    var rawContent = response.Content.ReadAsStringAsync().Result;
                    var onlineList = rawContent.Split('\n').ToList();

                    if (onlineList[0] == "full")
                        _cnBanList.Clear();

                    var content = onlineList.Select(str => str.Split('_')).Where(element => element.Length > 1);

                    foreach (var single in content)
                    {
                        try
                        {
                            int type = int.Parse(single[1]);
                            string value = single[0];
                            string reason = single.Length > 2 ? single[2] : ""; // 第三个部分是备注/原因

                            bool alreadyExists = _cnBanList.Any(ban => ban.type == type && ban.value == value);

                            if (!alreadyExists)
                            {
                                _cnBanList.Add(new CNBanInfo {
                                    type = type,
                                    value = value,
                                    reason = reason
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            PluginLogger.DebugVerbose($"解析CNBan条目失败: {string.Join("_", single)}, 错误: {ex.Message}", "CNBAN");
                        }
                    }

                    if (long.TryParse(onlineList.Last() ?? "-1", out long servertime) && servertime != -1)
                        _cnBanTimestamp = servertime;

                    PluginLogger.Debug($"CNBan封禁列表已更新，共{_cnBanList.Count}条记录");

                    // 显示包含备注的记录数量
                    int recordsWithReason = _cnBanList.Count(ban => !string.IsNullOrEmpty(ban.reason));
                    if (recordsWithReason > 0)
                    {
                        PluginLogger.DebugVerbose($"其中 {recordsWithReason} 条记录包含备注信息", "CNBAN");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                PluginLogger.Error($"下载CNBan封禁列表失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                PluginLogger.Error($"处理CNBan封禁列表失败: {ex.Message}");
            }
        }
    }
}
