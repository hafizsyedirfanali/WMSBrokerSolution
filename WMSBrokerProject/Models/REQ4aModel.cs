namespace WMSBrokerProject.Models
{
    public class REQ4aModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? AanvraagId { get; set; }
        public string? ProId { get; set; }
        public RES2Model RES2Response { get; set; }
        public Dictionary<string, string> GoEfficientAttributes { get; set; }
    }
}
