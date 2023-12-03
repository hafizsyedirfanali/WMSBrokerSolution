using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;


namespace WMSBrokerProject.Models;

public class TaskIndicationRequestModel
{
    [JsonProperty("header")]
    public Header header { get; set; }
    [JsonProperty("inId")]
    public string inId { get; set; }


    public class Header
    {
        public From from { get; set; }
        public int updateCount { get; set; }
        public DateTime created { get; set; }
        public string priority { get; set; }
    }

    public class From
    {
        public string orgId { get; set; }
        public string systemId { get; set; }
    }

}