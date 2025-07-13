using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SCPSLCNCBan_LabAPI.Constants;
using SCPSLCNCBan_LabAPI.Models;
using SCPSLCNCBan_LabAPI.Utils;

namespace SCPSLCNCBan_LabAPI.Services
{
    /// <summary>
    /// 封禁API服务
    /// </summary>
    public class BanApiService
    {
        /// <summary>
        /// 获取封禁列表和理由列表
        /// </summary>
        /// <returns>封禁数据，如果失败返回null</returns>
        public async Task<(List<BanInfo> banList, List<string> banReasons)> GetBanListAsync()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = await client.DownloadStringTaskAsync($"{ApiConstants.API_ROOT_URL}/listr.php?key={ApiConstants.API_KEY}");
                    
                    var response = JsonConvert.DeserializeObject<BanResponse>(json);
                    
                    // 检查API返回状态
                    if (response.s == 1)
                    {
                        PluginLogger.Debug($"成功刷新封禁列表，共{response.b.Count}条记录，{response.rs.Count}个封禁理由");
                        return (response.b, response.rs);
                    }
                    else
                    {
                        PluginLogger.Debug($"API返回状态异常: {response.s}，不更新封禁列表");
                        return (null, null);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.Error($"刷新封禁列表失败: {e}");
                return (null, null);
            }
        }
    }
}
