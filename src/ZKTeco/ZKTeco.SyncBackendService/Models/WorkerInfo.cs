using Newtonsoft.Json;

namespace ZKTeco.SyncBackendService.Models
{
    [JsonObject]
    public class WorkerInfo
    {
        public string EnrollNumber { get; set; }
        [JsonProperty(PropertyName = "_id")]
        public string UserId { get; set; }

        public string ProjectId { get; set; }
    }
}
