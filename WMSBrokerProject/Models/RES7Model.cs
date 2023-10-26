namespace WMSBrokerProject.Models
{
    public class RES7Model
    {
        public List<RES7ProIDClass> ProIdList { get; set; }
    }
    public class RES7ProIDClass
    {
        public string ProId { get; set; }
        public string Opened { get; set; }
        public string Closed { get; set; }
    }
}
