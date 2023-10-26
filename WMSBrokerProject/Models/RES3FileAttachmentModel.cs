namespace WMSBrokerProject.Models
{
    public class RES3FileAttachmentModel
    {
        public List<BijlageClass> BijlageList { get; set; }
    }
    public class BijlageClass
    {
        public string VraagCode { get; set; }
        public string BijlageID { get; set; }
        public string Bestandsnaam { get; set; }
        public string FileContent { get; set; }
    }
}
