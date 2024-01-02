using Newtonsoft.Json;
using WMSBrokerProject.Utilities;

namespace WMSBrokerProject.Models
{
	public class TaskSyncRequestModel
	{
		public string taskId { get; set; }
		public Header header { get; set; }
		public Status status { get; set; }

		public class From
		{
			public string orgId { get; set; }
			public string systemId { get; set; }
		}

		public class Header
		{
			public From from { get; set; }
			public int updateCount { get; set; }
			public string created { get; set; }
		}

		

		public class Status
		{
            [JsonConverter(typeof(NullValueRemoverConverter))]
            public object? mainStatus { get; set; }

            [JsonConverter(typeof(NullValueRemoverConverter))]
			public object? subStatus { get; set; }
            
			[JsonConverter(typeof(NullValueRemoverConverter))]
			public object? reason { get; set; }
            
			[JsonConverter(typeof(NullValueRemoverConverter))]
			public object? clarification { get; set; }
		}


	}
}
