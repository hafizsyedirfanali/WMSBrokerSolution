using static WMSBrokerProject.Models.TaskFetchResponseModel;

namespace WMSBrokerProject.Models
{
    public class TaskFetchResponseModel
    {
        public string inId { get; set; }
        public TaskInfo taskInfo { get; set; }
        public string originator { get; set; }
        public Header header { get; set; }
        public Status status { get; set; }
        public string action { get; set; }
        public string urgency { get; set; }
        public string taskId { get; set; }
        public Area area { get; set; }
        public List<Comment> comments { get; set; }
        public string contractant { get; set; }
        public bool readOnly { get; set; }

        public class TaskInfo
        {
            public HasInfo hasInfo { get; set; }
            public PopInfo popInfo { get; set; }
        }
		public class PopInfo
		{
			public string DHid { get; set; }
			public ContactPerson contactPerson { get; set; }
			public ConnectionAddress connectionAddress { get; set; }
			public string fiberNumber { get; set; }
			public string outletStatus { get; set; }
			public Payment payment { get; set; }
            public ActiveEquipmentEndpoint activeEquipmentEndpoint { get; set; }
        }
		public class HasInfo
        {
            public string DHid { get; set; }
            public ContactPerson contactPerson { get; set; }
            public ConnectionAddress connectionAddress { get; set; }
            public string fiberNumber { get; set; }
            public string outletStatus { get; set; }
            public Payment payment { get; set; }
        }

        public class ContactPerson
        {
            public string lastName { get; set; }
            public string phoneNumber { get; set; }
            public string email { get; set; }
        }

        public class ConnectionAddress
        {
            public string postalCode { get; set; }
            public int houseNumber { get; set; }
            public string city { get; set; }
            public string streetName { get; set; }
            public string country { get; set; }
        }

        public class Payment
        {
            public double amount { get; set; }
            public string referenceContractor { get; set; }
        }
        public class ActiveEquipmentEndpoint
        {
            public string pop { get; set; }
            public string frame { get; set; }
            public string block { get; set; }
            public string module { get; set; }
            public string port { get; set; }
            public string connector { get; set; }
            public string row { get; set; }
            public string odfTray { get; set; }
        }


		public class Header
        {
            public From from { get; set; }
            public int updateCount { get; set; }
            public DateTime created { get; set; }
        }

        public class From
        {
            public string orgId { get; set; }
            public string systemId { get; set; }
        }

        public class Status
        {
            public string mainStatus { get; set; }
        }

        public class Area
        {
            public string networkType { get; set; }
        }

        public class Comment
        {
            public Party party { get; set; }
            public Author author { get; set; }
            public DateTime created { get; set; }
            public string note { get; set; }
            public string id { get; set; }
        }

        public class Party
        {
            public string orgId { get; set; }
            public string systemId { get; set; }
        }

        public class Author
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
        }

    }
}
