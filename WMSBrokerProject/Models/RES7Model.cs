namespace WMSBrokerProject.Models
{
    public class RES7Model
    {
        public List<RES7ProIDClass> TaskIdList { get; set; }
    }
    public class RES7ProIDClass
    {
        public string TaskId { get; set; }
        public string ProIdDESC { get; set; }
        public string Opened { get; set; }
        public string Closed { get; set; }
    }
}
