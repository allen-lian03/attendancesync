using Newtonsoft.Json;
using System;

namespace ZKTeco.SyncBackendService.Models
{
    [JsonObject]
    public class CheckInInfo
    {
        public CheckInInfo(string projectId, string workerId, string location, DateTime logDate)
        {
            ProjectId = projectId;
            WorkerId = workerId;
            Location = location;
            CheckInDate = logDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        [JsonProperty(PropertyName = "pid")]
        public string ProjectId { get; private set; }

        [JsonProperty(PropertyName = "wid")]
        public string WorkerId { get; private set; }

        [JsonProperty(PropertyName = "loc")]
        public string Location { get; private set; }

        [JsonProperty(PropertyName = "in")]
        public string CheckInDate { get; private set; }
    }
}
