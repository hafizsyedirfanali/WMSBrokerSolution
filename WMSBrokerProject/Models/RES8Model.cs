namespace WMSBrokerProject.Models
{
    public class RES8Model
    {
        
        public List<RES48AddressFields> AddressFields { get; set; }
        
    }

    public class RES48AddressFields
    {
        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string Country { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public string HouseNumberExtension { get; set; }
    }

    public class AddressFieldsList
    {
        public string Fin_Name { get; set; }
        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string Country { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public string HouseNumberExtension { get; set; }

    }
}
