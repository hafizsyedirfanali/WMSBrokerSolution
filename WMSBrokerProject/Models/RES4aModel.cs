namespace WMSBrokerProject.Models
{
    public class RES4aModel
    {

        public string? FIN_ID { get; set; }
        //public List<RES4aAddress> Addresses { get; set; }
        public RES4aTemplate Template { get; set; }
        public List<FinNameFC> FinNameFCList { get; set; }

    }
    public class FinNameFC
    {
        public string FinName { get; set; }
        public Dictionary<string, string> SelectListItems { get; set; }
    }
    public class RES4aTemplateFields
    {
        public string FIN_ID { get; set; }
        public string UDF_TYPE { get; set; }
        public string FIN_NAME { get; set; }
        public string FIN_RECORD_ID { get; set; }
        public string FIN_PATH { get; set; }
        public string FIN_DATE { get; set; }
        public string FIN_NUMBER { get; set; }
        public string FIN_MEMO { get; set; }
        public string FIN_FILE_EXT { get; set; }
        public string UDF_TYPEINFO { get; set; }
        public string UDF_LABEL { get; set; }
        public string PRO_ID { get; set; }
    }
    public class GoEfficientTemplateAttributesClass
    {
        public string FinId { get; set; }
        public string FinName { get; set; }
        public string ProId { get; set; }
        public string UdfType { get; set; }
        public string UdfTypeInfo { get; set; }
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
