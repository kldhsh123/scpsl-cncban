using System.Collections.Generic;
using Newtonsoft.Json;

namespace SCPSLCNCBan_LabAPI.Models
{
    /// <summary>
    /// API响应数据模型
    /// </summary>
    public class BanResponse
    {
        [JsonProperty("s")]
        public int s { get; set; }

        [JsonProperty("rs")]
        public List<string> rs { get; set; }

        [JsonProperty("b")]
        public List<BanInfo> b { get; set; }
    }

    /// <summary>
    /// 封禁信息数据模型
    /// </summary>
    public class BanInfo
    {
        [JsonProperty("v")]
        public string v { get; set; }

        [JsonProperty("r")]
        public int r { get; set; }

        [JsonProperty("m")]
        public int m { get; set; }

        [JsonProperty("g")]
        public string g { get; set; }
    }

    /// <summary>
    /// CNBan封禁信息数据模型
    /// </summary>
    public class CNBanInfo
    {
        public int type;
        public string value;
        public string reason;  // 封禁原因/备注
    }

    /// <summary>
    /// 用于日志记录的旧格式封禁信息
    /// </summary>
    public class LegacyBanInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public int Time { get; set; }
        public string Reason { get; set; }
        public string StartTime { get; set; }
        public string Range { get; set; }
    }
}
