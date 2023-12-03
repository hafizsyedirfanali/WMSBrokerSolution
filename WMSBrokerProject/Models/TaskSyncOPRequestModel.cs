using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.Serialization;

namespace WMSBrokerProject.Models
{
    public class TaskSyncOPRequestModel
    {
        [JsonProperty("taskId")]
        public string taskId { get; set; }
        [JsonProperty("header")]
        public Header header { get; set; }
        [JsonProperty("status")]
        public Status status { get; set; }

        public class From
        {
            public string orgId { get; set; }
            public string systemId { get; set; }
        }

        public class Header
        {
            public From from { get; set; }
            public int updateCount { get; set; }
            public DateTime created { get; set; }
        }

        public class Status
        {
            public string mainStatus { get; set; }
            public string subStatus { get; set; }
            public string reason { get; set; }
            public string clarification { get; set; }
        }


    }
}
