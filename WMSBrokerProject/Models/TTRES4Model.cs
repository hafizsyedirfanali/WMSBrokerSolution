namespace WMSBrokerProject.Models
{
    public class TTRES4Model
    {
        public int ResultCode { get; set; }
    }
    public class TrackTraceTemplateResponse
    {
        public List<TemplateClass> Templates { get; set; }
    }
    public class TemplateClass
    {
        public string TemplateId { get; set; }
        public string Status { get; set; }
    }
}
