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
        [Obsolete("Don't use it",error:true)]
        public Dictionary<string, string> GoEfficientTemplateAttributes { get; set; }

        public List<GoEfficientTemplateAttributesClass> GoEfficientTemplateAttributeList { get; set; }
        public Dictionary<string, object?> GoEfficientTemplateValues { get; set; }

        public Dictionary<string, string> GoEfficientAddressTemplateAttributes { get; set; }
        public Dictionary<string, object?> GoEfficientAddressTemplateValues { get; set; }
        //public string? ObjectId { get; set; }
        //public string? AanvraagId { get; set; }
        //public string? TelefoonnummerMobiel { get; set; }
        //public string? Voorletters { get; set; }
        //public string? Tussenvoegsels { get; set; }
        //public string? Achternaam { get; set; }
        //public string? Emailadres { get; set; }
        //public string? AanvraagDatum { get; set; }
        //public string? WensJaar { get; set; }
        //public string? WensWeek { get; set; }
        //public string? ObjectType { get; set; }
        //public string? Plaats { get; set; }
        //public string? Postcode { get; set; }
        //public string? Huisnummer { get; set; }
        //public string? HuisnummerToevoeging { get; set; }
        //public string? Straat { get; set; }
    }
}
