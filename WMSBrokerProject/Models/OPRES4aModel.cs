namespace WMSBrokerProject.Models
{
    public class OPRES4aModel
    {
        public string InID { get; set; }
        public Res4aRowFields? Res4ARowFields { get; set; }
        public Dictionary<string, object?> SelectListItems { get; set; }
        public string? ActionName { get; set; }
    }
    public class Res4aRowFields
    {
        public string Pro_Description { get; set; }
        public string Pro_Template_Id { get; set; }
    }
    public class Res4aActionFields
    {
        public string FIN_NAME { get; set; }
        public string FIN_PATH { get; set; }
        public string UDF_TYPEINFO { get; set; }
    }
    public class Res4aGetTemplateModel
    {
        public List<RES4aTemplateFields> Templates { get; set; }
        public List<Fin_AddressClass> Addresses { get; set; }
    }

}
