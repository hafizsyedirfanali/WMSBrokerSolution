namespace WMSBrokerProject.Models
{
    /// <summary>
    /// <CityName> <StreetName> <HouseNumber>[<HouseNumberSuffix>] <ZipCode> <AanvraagID>
    /// </summary>
    public class REQ4Model
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string InId { get; set; }
        public string CityName { get; set; }
        public string StreetName { get; set; }  
        public string HouseNumber { get; set; } 
        public string? HouseNumberExtension { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PRO_ID { get; set; }
        public string Indicator2 { get; set; }
        
    }

}
