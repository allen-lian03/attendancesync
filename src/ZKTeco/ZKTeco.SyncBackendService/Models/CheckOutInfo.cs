using Newtonsoft.Json;
using System;

namespace ZKTeco.SyncBackendService.Models
{
    [JsonObject]
    public class CheckOutInfo
    {
        public CheckOutInfo(string projectId, string workerId, DateTime logDate)
        {
            ProjectId = projectId;
            WorkerId = workerId;
            CheckOutDate = logDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        [JsonProperty(PropertyName = "pid")]
        public string ProjectId { get; private set; }

        [JsonProperty(PropertyName = "wid")]
        public string WorkerId { get; private set; }

        [JsonProperty(PropertyName = "out")]
        public string CheckOutDate { get; private set; }
    }
}
