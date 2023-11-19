namespace WMSBrokerProject.Models
{
    public class TTRES4Model
    {
        public int ResultCode { get; set; }
    }
    public class OrderProcessingTemplateResponse
    {
        public List<TemplateClass> Templates { get; set; }
    }
    public class TemplateClass
    {
        public string TemplateKey { get; set; }
        public string ActionType { get; set; }
        public string TemplateID { get; set; }
        public string WMSStatus { get; set; }
        public string GoEfficientStatus { get; set; }
    }
}
