using System.Xml.Serialization;

namespace WMSBrokerProject.Models
{
    public class RES4aXMLResponseModel
    {
        [XmlRoot("Response")]
        public class Response
        {
            public Header Header { get; set; }
            public Body Body { get; set; }
        }

        public class Header
        {
            public string RequestId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class Body
        {
            public Result Result { get; set; }
        }

        public class Result
        {
            public Rows Rows { get; set; }
        }

        public class Rows
        {
            [XmlAttribute("Count")]
            public int Count { get; set; }

            [XmlElement("Row")]
            public List<Row> RowList { get; set; }
        }

        public class Row
        {
            [XmlElement("Value")]
            public List<Value> Values { get; set; }
        }

        public class Value
        {
            [XmlAttribute("FieldName")]
            public string FieldName { get; set; }

            [XmlText]
            public string FieldValue { get; set; }
        }





        //[XmlRoot(ElementName = "Header")]
        //public class Header
        //{

        //	[XmlElement(ElementName = "RequestId")]
        //	public int RequestId { get; set; }

        //	[XmlElement(ElementName = "Timestamp")]
        //	public DateTime Timestamp { get; set; }
        //}

        //[XmlRoot(ElementName = "Value")]
        //public class Value
        //{

        //	[XmlAttribute(AttributeName = "FieldName")]
        //	public string FieldName { get; set; }

        //	[XmlText]
        //	public int Text { get; set; }
        //}

        //[XmlRoot(ElementName = "Row")]
        //public class Row
        //{

        //	[XmlElement(ElementName = "Value")]
        //	public List<Value> Value { get; set; }
        //}

        //[XmlRoot(ElementName = "Rows")]
        //public class Rows
        //{

        //	[XmlElement(ElementName = "Row")]
        //	public List<Row> Row { get; set; }

        //	[XmlAttribute(AttributeName = "Count")]
        //	public int Count { get; set; }

        //	[XmlText]
        //	public string Text { get; set; }
        //}

        //[XmlRoot(ElementName = "Result")]
        //public class Result
        //{

        //	[XmlElement(ElementName = "Rows")]
        //	public Rows Rows { get; set; }
        //}

        //[XmlRoot(ElementName = "Body")]
        //public class Body
        //{

        //	[XmlElement(ElementName = "Result")]
        //	public Result Result { get; set; }
        //}

        //[XmlRoot(ElementName = "Response")]
        //public class Response
        //{

        //	[XmlElement(ElementName = "Header")]
        //	public Header Header { get; set; }

        //	[XmlElement(ElementName = "Body")]
        //	public Body Body { get; set; }
        //}
    }


    public class RES4aModel
    {

        public string? FIN_ID { get; set; }
        public List<RES4aAddress> Addresses { get; set; }
        public RES4aTemplate Template { get; set; }
        public List<FinNameFC> FinNameFCList { get; set; }

    }

    public class RES4aAddress
    {
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string HouseNo { get; set; }
        public string HouseNoSuffix { get; set; }
        public string Street { get; set; }
        public string FIN_Id { get; set; }
        public string FIN_Name { get; set; }
    }

    public class FinNameFC
    {
        public string FinName { get; set; }
        public Dictionary<string, string> SelectListItems { get; set; }
    }
    public class RES4aTemplateFields
    {
        public string FIN_ID { get; set; }
        public string UDF_TYPE { get; set; }
        public string FIN_NAME { get; set; }
        public string FIN_RECORD_ID { get; set; }
        public string FIN_PATH { get; set; }
        public string FIN_DATE { get; set; }
        public string FIN_NUMBER { get; set; }
        public string FIN_MEMO { get; set; }
        public string FIN_FILE_EXT { get; set; }
        public string UDF_TYPEINFO { get; set; }
        public string UDF_LABEL { get; set; }
        public string PRO_ID { get; set; }
    }
    public class GoEfficientTemplateAttributesClass
    {
        public string FinId { get; set; }
        public string FinName { get; set; }
        public string ProId { get; set; }
        public string UdfType { get; set; }
        public string UdfTypeInfo { get; set; }
        public string GoEfficientAttributeName { get; set; }
        public string MappingName { get; set; }
        public string MappingValue { get; set; }
    }
    public class RES4aTemplate
    {
        public List<GoEfficientTemplateAttributesClass> GoEfficientTemplateAttributeList { get; set; }
        public Dictionary<string, object?> GoEfficientTemplateValues { get; set; }

        public Dictionary<string, string> GoEfficientAddressTemplateAttributes { get; set; }
        public Dictionary<string, object?> GoEfficientAddressTemplateValues { get; set; }
       
    }
}
