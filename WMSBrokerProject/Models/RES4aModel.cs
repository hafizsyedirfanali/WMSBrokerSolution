namespace WMSBrokerProject.Models
{
    public class RES4aModel
    {

        public string? FIN_ID { get; set; }
        public List<RES4aAddress> Addresses { get; set; }
        public RES4aTemplate Template { get; set; }

    }
    public class RES4aAddress
    {
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string HouseNo { get; set; }
        public string HouseNoSuffix { get; set; }
        public string Street { get; set; }
        public string FIN_Id { get; set; }
        public string FIN_Name { get; set; }
    }
    public class GoEfficientTemplateAttributesClass
    {
        public string FinId { get; set; }
        public string FinName { get; set; }
        public string ProId { get; set; }
        public string UdfType { get; set; }
        public string GoEfficientAttributeName { get; set; }
        public string MappingName { get; set; }
        public string MappingValue { get; set; }
    }
    public class RES4aTemplate
    {
        public List<GoEfficientTemplateAttributesClass> GoEfficientTemplateAttributeList { get; set; }
        public Dictionary<string, object?> GoEfficientTemplateValues { get; set; }

        public Dictionary<string, string> GoEfficientAddressTemplateAttributes { get; set; }
        public Dictionary<string, object?> GoEfficientAddressTemplateValues { get; set; }
       
    }
}
