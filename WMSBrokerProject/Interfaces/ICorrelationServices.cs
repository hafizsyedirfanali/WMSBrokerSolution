using WMSBrokerProject.Repositories;

namespace WMSBrokerProject.Interfaces
{
    public interface ICorrelationServices
    {
        public List<CorrelationItem> CorrelationItems { get; set; }
        void SaveCorrelationItem(CorrelationItem correlationItem);
        void RemoveCorrelationItem(string taskId);
        List<CorrelationItem> GetCorrelationItems();
        CorrelationItem? GetCorrelationItemByTaskId(string taskId);
        CorrelationItem? GetCorrelationItemByCorrelationId(string correlationId);
    }
}
