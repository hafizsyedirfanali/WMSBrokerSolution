using WMSBrokerProject.Interfaces;

namespace WMSBrokerProject.Repositories
{
    public class CorrelationImplementation : ICorrelationServices
    {
        public string inId { get; set; }
        public string CorrelationID { get; set; }
    }
}
