namespace WMSBrokerProject.Models
{
    public class REQ5Model
    {
        public string InId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DamangeType { get; set; }
        public string IsHoleOpen { get; set; }
        public string KLICReportingNo { get; set; }
        public string DateTimeOfDamage { get; set; }
        public string CompanyName { get; set; }
        public string ReportingDate { get; set; }
        public string ReporterName { get; set; }
        public string ReporterPhoneNumber { get; set; }
        public string PRO_ID_3 { get; set; }
        public RES4aTemplate RES4aTemplate { get; set; }
        public Dictionary<string,object?> GoEfficientTemplateValues { get; set; }

    }
    
}
