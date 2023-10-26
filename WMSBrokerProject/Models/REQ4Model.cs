namespace WMSBrokerProject.Models
{
    /// <summary>
    /// <CityName> <StreetName> <HouseNumber>[<HouseNumberSuffix>] <ZipCode> <AanvraagID>
    /// </summary>
    public class REQ4Model
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string AanvraagId { get; set; }
        public string CityName { get; set; }
        public string StreetName { get; set; }  
        public string HouseNumber { get; set; } 
        public string? HouseNumberSuffix { get; set; }
        public string ZipCode { get; set; }
        
    }

}
