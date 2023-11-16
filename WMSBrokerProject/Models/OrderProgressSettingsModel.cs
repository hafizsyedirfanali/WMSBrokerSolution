namespace WMSBrokerProject.Models
{
    public class OrderProgressSettingsModel
    {
        public Dictionary<string, OrderProgressTemplate> OrderProgressTemplates { get; set; }
    }
    public class OrderProgressTemplate
    {
        public string ActionType { get; set; }
        public string TemplateID { get; set; }
        public string WMSStatus { get; set; }
        public string GoEfficientStatus { get; set; }
    }
}
