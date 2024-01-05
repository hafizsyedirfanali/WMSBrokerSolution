using System.Threading.Tasks;
using WMSBrokerProject.Interfaces;

namespace WMSBrokerProject.Repositories
{
    public class CorrelationImplementation : ICorrelationServices
    {
        public CorrelationImplementation()
        {
            this.CorrelationItems = new List<CorrelationItem>();
        }
        public List<CorrelationItem> CorrelationItems { get; set; }

        public CorrelationItem? GetCorrelationItemByCorrelationId(string correlationId)
        {
            return CorrelationItems.FirstOrDefault(s => s.CorrelationID == correlationId);
        }

        public CorrelationItem? GetCorrelationItemByTaskId(string taskId)
        {
            return CorrelationItems.FirstOrDefault(s => s.TaskId == taskId);
        }

        public List<CorrelationItem> GetCorrelationItems()
        {
            return CorrelationItems;
        }

        public void RemoveCorrelationItem(string taskId)
        {
            var correlationItem = CorrelationItems.FirstOrDefault(s=>s.TaskId == taskId);
            if(correlationItem is not null)
            {
                this.CorrelationItems.Remove(correlationItem);
            }
        }

        public void SaveCorrelationItem(CorrelationItem correlationItem)
        {
            if(this.CorrelationItems.Any(s=>s.TaskId == correlationItem.TaskId))
            {
                CorrelationItem selectedItem = this.CorrelationItems.FirstOrDefault(s=>s.TaskId == correlationItem.TaskId)!;
                if(!string.IsNullOrEmpty(correlationItem.CorrelationID)) selectedItem.CorrelationID = correlationItem.CorrelationID;
                if (!string.IsNullOrEmpty(correlationItem.Pro_Id)) selectedItem.Pro_Id = correlationItem.Pro_Id;
                if (!string.IsNullOrEmpty(correlationItem.Action)) selectedItem.Action = correlationItem.Action;
            }
            else
            {
                this.CorrelationItems.Add(correlationItem);
            }
        }
    }
    public class CorrelationItem
    {
        public string TaskId { get; set; }
        public string? CorrelationID { get; set; }
        public string? Pro_Id { get; set; }
        public string? Action { get; set; }
    }
}
