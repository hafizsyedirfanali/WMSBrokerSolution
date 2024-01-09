using System.ComponentModel.DataAnnotations;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.TaskSyncModels
{
    public class TaskSyncOPRequestModel
    {
        public string taskId { get; set; }
        public Header header { get; set; }
        public Status status { get; set; }

    }

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
        public string? subStatus { get; set; }
        public string? reason { get; set; }
        public string? clarification { get; set; }
    }
}
