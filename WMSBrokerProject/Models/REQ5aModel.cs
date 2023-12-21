namespace WMSBrokerProject.Models
{
    public class REQ5aModel
    {
        public string RequestId { get; set; }
        public string InId { get; set; }
        public string PRO_ID_3 { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string HouseNo { get; set; }
        public string HouseNoSuffix { get; set; }
        public string Street { get; set; }
        public string Address_FIN_ID { get; set; }
        public RES4aTemplate Template { get; set; }
        //public RES2Model RES2Response { get; set; }
        public Dictionary<string, string> ExtractedAddressValues { get; set; }
    }
}
