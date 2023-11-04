using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WMSBrokerProject.Repositories
{
	public class GoEfficientServiceImplementation : IGoEfficientService
    {
        private readonly string? templateFolder;
        private readonly string symbolForConcatenation;
        private readonly string symbolForPriority;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly GoEfficientCredentials goEfficientCredentials;
        public GoEfficientServiceImplementation(IConfiguration configuration, IWebHostEnvironment hostEnvironment,
            IOptions<GoEfficientCredentials> goEfficientCredentials)
        {
            _configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.goEfficientCredentials = goEfficientCredentials.Value;
            this.symbolForConcatenation = configuration.GetSection("MijnAansluiting:SymbolForConcatenation").Value ?? "+";
            this.symbolForPriority = configuration.GetSection("MijnAansluiting:SymbolForPriority").Value ?? "*";
            this.templateFolder = configuration.GetSection("TemplatesFolder").Value;
        }
        public string? GetHighestPriorityKey(RES2Model model, string sourceKey, string destinationKey)
        {
            string? priorityKey = null;
            var sourceKeyArray = sourceKey.Split(symbolForPriority);
            foreach (var sKey in sourceKeyArray.OrderByDescending(s => s))
            {
                var value = GetValueOfKey(model, sKey);
                if (value is not null)
                {
                    priorityKey = sKey;//check
                }
            }
            return priorityKey;
        }
        public object? GetValueOfKey(RES2Model model, string sourceKey)
        {
            object? value = null;
            model.WMSBeheerderAttributes.TryGetValue(sourceKey, out value);
            return value;
        }
        
        public (string DestinationKey, object? Value) GetOneToOneValue(RES2Model model, string sourceKey, string destinationKey)
        {
            object? value;
            //Following is 1:1 map - following line of code is fetching the path from response for the mapped key
            if (sourceKey.Contains(symbolForPriority))
            {
                sourceKey = GetHighestPriorityKey(model, sourceKey, destinationKey) ?? string.Empty;
            }
            model.WMSBeheerderAttributes.TryGetValue(sourceKey, out value);
            return (destinationKey, value);
        }
        public (string DestinationKey, object Value) GetOneToManyValue(RES2Model model, string sourceKey, string destinationKey)
        {
            List<object> sourceValueList = new();
            var sourceKeyArray = sourceKey.Split(symbolForConcatenation);
            foreach (var sKey in sourceKeyArray)
            {
                if (sKey.Contains(symbolForPriority))
                {
                    var priorityKey = GetHighestPriorityKey(model, sourceKey, destinationKey);
                    if (priorityKey != null)
                    {
                        var val = GetValueOfKey(model, priorityKey);
                        if (val is not null)
                        {
                            sourceValueList.Add(val);
                        }
                    }
                }
                //checking if array has any element that contains "[".. This is for optional parameter
                if (sKey.Contains("["))
                {
                    //Removing [ ] 
                    var sKeyWithoutBracket = sKey.Substring(1, sKey.Length - 2);
                    if (sKeyWithoutBracket != null)
                    {
                        var val = GetValueOfKey(model, sKeyWithoutBracket);
                        if (val is not null)
                        {
                            sourceValueList.Add(val);
                        }
                    }
                }
                else
                {
                    var val = GetValueOfKey(model, sKey);
                    if (val is not null)
                    {
                        sourceValueList.Add(val);
                    }
                }
            }
            object sourceValueConcatenated;
            if (sourceKeyArray.Contains("WensJaar") && sourceValueList.Count==2)
            {
                List<int> list = sourceValueList.Select(o => Convert.ToInt32(o)).OrderBy(s=>s).ToList();
                
                sourceValueConcatenated = GetFridayFromDate(list[0], list[1]);
            }
            else
            {
                sourceValueConcatenated = string.Join(' ', sourceValueList);
            }

            return (destinationKey, sourceValueConcatenated);
        }
        public DateTime GetFridayFromDate(int weekNumber, int year)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            DateTime firstMonday = jan1.AddDays((int)DayOfWeek.Monday - (int)jan1.DayOfWeek + (jan1.DayOfWeek <= DayOfWeek.Monday ? 0 : 7));
            DateTime desiredMonday = firstMonday.AddDays((weekNumber - 1) * 7);
            return desiredMonday.AddDays(4);
        }

        public async Task<ResponseModel<string>> GetKeyForRES4Mapping()
        {
            var responseModel = new ResponseModel<string>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("MijnAansluitingRES4Mapping").AsEnumerable();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];
                        responseModel.Result = destinationKey;
                    }
                }
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;

        }
        public async Task<ResponseModel<string>> GetKeyForValueInRES3aMapping(string value)
        {
            var responseModel = new ResponseModel<string>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("MijnAansluitingRES3aMapping").AsEnumerable();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];
                        if (value.Contains(sourceKey))
                        {
                            responseModel.Result = destinationKey;
                        }
                    }
                }
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }
        public async Task<ResponseModel<RES4aTemplate>> FillDataIn4aTemplate(RES4aTemplate template, TaskFetchResponse2Model model)
        {
            var responseModel = new ResponseModel<RES4aTemplate>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("WMSBeheerderRES2Mapping").AsEnumerable();
                
                Dictionary<string, object?> mappedValues = new();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];

                    }
                }


                template.GoEfficientTemplateValues = mappedValues;
                responseModel.Result = template;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }
        public async Task<ResponseModel<RES4aTemplate>> FillDataIn4aAddressTemplate(RES4aTemplate template, TaskFetchResponse2Model model)
        {
            var responseModel = new ResponseModel<RES4aTemplate>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("MijnAansluitingRES2AddressMapping").AsEnumerable();
                
                Dictionary<string, object?> mappedValues = new();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];

                        //For 1:Many
                        //if (sourceKey.Contains(symbolForConcatenation))
                        //{
                        //    var valueTuple = GetOneToManyValue(model, sourceKey, destinationKey);
                        //    mappedValues.Add(valueTuple.DestinationKey, valueTuple.Value);
                        //}
                        //else if (sourceKey.Contains(symbolForPriority))
                        //{
                        //    var valueTuple = GetOneToOneValue(model, sourceKey, destinationKey);
                        //    mappedValues.Add(valueTuple.DestinationKey, valueTuple.Value);
                        //}
                        //else
                        //{
                        //    var valueTuple = GetOneToOneValue(model, sourceKey, destinationKey);
                        //    mappedValues.Add(valueTuple.DestinationKey, valueTuple.Value);
                        //}
                    }
                }
                template.GoEfficientAddressTemplateValues = mappedValues;
                responseModel.Result = template;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }
        public async Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientFileAttributes()
        {
            var responseModel = new ResponseModel<Dictionary<string, string>>();
            try
            {
                var goEfficientAttributes = _configuration.GetSection("GoEfficientFileAttributes").AsEnumerable();
                Dictionary<string, string> goEfficientDict = new();
                foreach (var attribute in goEfficientAttributes)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var value = attribute.Value;
                        var leftKey = keyArray[keyArray.Length - 1];
                        goEfficientDict.Add(leftKey, value);
                    }
                }
                responseModel.Result = goEfficientDict;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10016;
            }
            return responseModel;
        }
        public async Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientAttributes()
        {
            var responseModel = new ResponseModel<Dictionary<string, string>>();
            try
            {
                var goEfficientAttributes = _configuration.GetSection("GoEfficientAttributes").AsEnumerable();
                Dictionary<string, string> goEfficientDict = new();
                foreach (var attribute in goEfficientAttributes)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var value = attribute.Value;
                        var leftKey = keyArray[keyArray.Length - 1];
                        goEfficientDict.Add(leftKey, value);
                    }
                }
                responseModel.Result = goEfficientDict;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10016;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES4Model>> REQ4_GetProIDAsync(REQ4Model model)
        {
            var responseModel = new ResponseModel<RES4Model>();

            try
            {
                using HttpClient client = new HttpClient();
                var date = DateTime.Now;
                var year = date.ToString("yyyy");
                var year_week = GetYearWeekISO(DateTime.Now);
                var yearweek = year + "-" + year_week;
                
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest4 = string.Empty;
                var houseNumberSuffix = string.IsNullOrEmpty(model.HouseNumberSuffix) ? "" : model.HouseNumberSuffix + " ";


                xmlRequest4 = $@"<Request>
                                     {GetXMLHeader(model.InId)}
                                     <Body>
                                         <CreateOperation>
                                             <OperationName>PRO_CREATE_TREE_FROM_TEMPL</OperationName>
                                             <Values>
                                                 <Value FieldName=""PRO.PRO_ID"">6744412</Value>
                                                 <Value FieldName=""Indicator"">{year};{year_week};{model.CityName} {model.StreetName} {model.HouseNumber} {houseNumberSuffix}{model.ZipCode} {model.InId}</Value>
                                                 <Value FieldName=""Indicator2"">6999459;6999459;9244608</Value>
                                                 <Value FieldName=""Indicator3"">P</Value>
                                             </Values>
                                         </CreateOperation>
                                     </Body>
                                 </Request>";

				var content = new StringContent(xmlRequest4, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstanceTemplateResponse_RES04.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }
                
                XDocument xdoc = XDocument.Parse(xmlResponse);

                var proId3Value = xdoc.Descendants("Value")
                                      .FirstOrDefault(e => (string)e.Attribute("FieldName") == "PRO.PRO_ID_3")?.Value;

                if (proId3Value != null)
                {
                    responseModel.Result = new RES4Model { ProId3 = proId3Value };
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.ErrorMessage = "PRO.PRO_ID_3 not found.";
                    responseModel.ErrorCode = 10003;
                }
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10004;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10005;
            }
            return responseModel;
        }

        //in REQ4aModel Pass the mapped properties of GoEfficient
        public async Task<ResponseModel<RES4aModel>> REQ4a_GetTemplateFromGoEfficient(REQ4aModel model)//This service is to get the form template
        {
            var responseModel = new ResponseModel<RES4aModel>();

            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest4a = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.InId)}
	                                        <Body>
		                                        <ReadOperation>
			                                        <Fields>
				                                        <Field>FIN.FIN_ID</Field>
				                                        <Field>UDF.UDF_TYPE</Field>
				                                        <Field>FIN.FIN_NAME</Field>
				                                        <Field>FIN.FIN_RECORD_ID</Field>
				                                        <Field>FIN.FIN_PATH</Field>
				                                        <Field>FIN.FIN_DATE</Field>
				                                        <Field>FIN.FIN_NUMBER</Field>
				                                        <Field>FIN.FIN_MEMO</Field>
				                                        <Field>FIN.FIN_FILE_EXT</Field>
				                                        <Field>UDF.UDF_TYPEINFO</Field>
				                                        <Field>UDF.UDF_LABEL</Field>
				                                        <Field>PRO.PRO_ID</Field>
			                                        </Fields>
			                                        <Conditions>
				                                        <Condition RightVariableType=""LiteralValue"" RightValue=""{model.ProId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""PRO.PRO_ID""/>
			                                        </Conditions>
			                                        <OperationName>FIN_PRO_READ</OperationName>
		                                        </ReadOperation>
	                                        </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest4a, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstantiatedAttacmentsResponse_RES04a.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                RES4aTemplate template = new RES4aTemplate();
                List<GoEfficientTemplateAttributesClass> templateAttributeList = new();
                XDocument xdoc = XDocument.Parse(xmlResponse);

                foreach (var property in model.GoEfficientAttributes)
                {
                    //if we want to get all attributes without address we can do it here
                    XElement? rowElement = xdoc.Descendants("Row")
                                .FirstOrDefault(row =>
                                    row.Elements("Value")
                                    .Any(e => (string)e.Attribute("FieldName")! == "FIN.FIN_NAME" &&
                                    e.Value.ToLower() == property.Value.ToLower()));
                    if (rowElement is not null)
                    {
                        string finId = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "FIN.FIN_ID")?.Value!;
                        string finName = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value!;
                        string proId = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "PRO.PRO_ID")?.Value!;
                        string udfType = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "UDF.UDF_TYPE")?.Value!;

                        templateAttributeList.Add(new GoEfficientTemplateAttributesClass
                        {
                            GoEfficientAttributeName = property.Key,
                            MappingName = property.Value,
                            FinId = finId,
                            FinName = finName,
                            ProId = proId,
                            UdfType = udfType
                        });
                    }
                }
                template.GoEfficientTemplateAttributeList = templateAttributeList;


                //2. fill the values from res2 into above


                List<RES4aAddress> addresses = (from row in xdoc.Descendants("Row")
                                                let udfTypeValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPE")?.Value
                                                where udfTypeValue == "A"
                                                let udfLabelValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_LABEL")?.Value
                                                let finNameValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value
                                                let udfFIN_IDValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_ID")?.Value
                                                select new RES4aAddress
                                                {
                                                    HouseNo = udfLabelValue,
                                                    FIN_Id = udfFIN_IDValue,
                                                    FIN_Name = finNameValue
                                                }).ToList();
                //1. create TemplateAttributes for all addresses "AddressTemplateAttribute"
                Dictionary<string, string> addressTemplateAttribute = new Dictionary<string, string>();
                foreach (var property in model.GoEfficientAttributes)
                {
                    var isAvailableInRes4a = addresses.Where(s => s.FIN_Name == property.Value).Any();
                    if (isAvailableInRes4a)
                    {
                        addressTemplateAttribute.Add(property.Key, property.Value);
                    }
                }
                template.GoEfficientAddressTemplateAttributes = addressTemplateAttribute;



                responseModel.Result = new RES4aModel
                {
                    Template = template,
                    Addresses = addresses,
                    FIN_ID = addresses.Select(s => s.FIN_Id).FirstOrDefault()
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10006;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10007;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES5Model>> REQ5_SaveRecordToGoEfficient(REQ5Model model)
        {
            var responseModel = new ResponseModel<RES5Model>();

            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string valueText = string.Empty;
                foreach (var templateField in model.GoEfficientTemplateValues)
                {
                    valueText += @$"<Value FieldName=""{templateField.Key}"">{templateField.Value}</Value>";
                }
                valueText += @"<Value FieldName=""Aanvraag ontvangen via?"">Aansluitingen.nl</Value>";
                string xmlRequest5 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.InId)}
	                                        <Body>
		                                        <UpdateOperation>
			                                        <OperationName>PRO_FIN_UPDATE</OperationName>
			                                        <Values>
                                                       {valueText}
			                                        </Values>
			                                        <Conditions>
				                                        <Condition FieldName=""PRO.PRO_ID"">{model.PRO_ID_3}</Condition>
				                                        <Condition FieldName=""Indicator"">R</Condition>
			                                        </Conditions>
		                                        </UpdateOperation>
	                                        </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest5, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_UpdateInstantiatedAttachmentsResponse_RES05.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                responseModel.Result = new RES5Model { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10009;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES5aModel>> REQ5a_SaveAddressToGoEfficient(REQ5aModel model)
        {
            var responseModel = new ResponseModel<RES5aModel>();

            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string valueText = string.Empty;
                foreach (var templateField in model.Template.GoEfficientAddressTemplateValues)
                {
                    valueText += @$"<Value FieldName=""{templateField.Key}"">{templateField.Value}</Value>";
                }
                var dict = model.Template.GoEfficientAddressTemplateValues;
                dict.TryGetValue("ADRESS.ADRESS_STREET", out object? straat);
                dict.TryGetValue("ADRESS.ADRESS_HOUSNR", out object? huisnummer);
                dict.TryGetValue("ADRESS.ADRESS_HOUSNR_SFX", out object? huisnummerToevoeging);
                dict.TryGetValue("ADRESS.ADRESS_ZIPCODE", out object? postcode);
                dict.TryGetValue("ADRESS.ADRESS_TOWN", out object? plaats);
                valueText += @"<Value FieldName=""ADRESS.ADRESS_CNTR_ISO3166A3"">NLD</Value>";
                valueText += @$"<Value FieldName=""FIN.FIN_PATH"">{straat} {huisnummer} {huisnummerToevoeging} {postcode} {plaats}</Value>";

                string xmlRequest5a = @$"<?xml version=""1.0"" encoding=""utf-8""?>
                            <Request>
	                            {GetXMLHeader(model.InId)}
	                            <Body>
		                            <UpdateOperation>
			                            <OperationName>FIN_UPDATE_V1</OperationName>
			                            <Values>
				                           {valueText}
                                            <Value FieldName=""ADRESS.ADRESS_STATE""/>
				                            <Value FieldName=""ADRESS.ADRESS_POSTAL_BOX""/>
			                            </Values>
			                            <Conditions>
				                            <Condition FieldName=""FIN.FIN_ID"">{model.Address_FIN_ID}</Condition>
			                            </Conditions>
		                            </UpdateOperation>
	                            </Body>
                            </Request>";

                var content = new StringContent(xmlRequest5a, Encoding.UTF8, "application/xml");

                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_UpdateInstantiatedAddressAttachmentsResponse_RES05a.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                responseModel.Result = new RES5aModel { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10010;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10011;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES5bModel>> REQ5b_AddFilesToGoEfficient(REQ5bModel model)
        {
            var responseModel = new ResponseModel<RES5bModel>();

            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest5b = $@"<Request>
                                      {GetXMLHeader(model.AanvraagId)}                               
                                      <Body>
                                       <UpdateOperation>
                                           <OperationName>FIN_UPDATE_V1</OperationName>
                                           <Values> 
                                               <Value FieldName=""FIN.FIN_PATH"">{model.Fin_Path}</Value>
                                               <Value FieldName=""FIN.FIN_CONTENTS"">{model.Fin_Content}</Value>
                                           </Values>
                                           <Conditions>
                                                <Condition FieldName=""FIN.FIN_ID"">{model.Fin_Id}</Condition>
                                           </Conditions>
                                       </UpdateOperation>
                                       </Body>
                                  </Request>";

                var content = new StringContent(xmlRequest5b, Encoding.UTF8, "application/xml");
                
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_Request05bUpdateInstantiatedFileContentsAttachments_RES05b.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                responseModel.Result = new RES5bModel { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10009;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES6Model>> REQ6_IsRecordExist(REQ6Model model)
        //It is used to check if the record already exist in GoEfficient
        {
            var responseModel = new ResponseModel<RES6Model>();

            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest6 = @$"<Request>
                                        {GetXMLHeader(model.InId)}
                                        <Body>
                                            <ReadOperation>
                                                <Fields>
                                                    <Field>PRO.PRO_ID</Field>
                                                </Fields>
                                                <Conditions>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""{model.InId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""FIN.FIN_PATH""/>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""{model.HuurderId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""FIN.FIN_UDF_ID""/>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""'cifwms-huurderid'"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""FIN.FIN_NAME_L""/>
                                                </Conditions>
                                                <OperationName>PRO_READ_M_V1</OperationName>
                                            </ReadOperation>
                                        </Body>
                                    </Request>";

                var content = new StringContent(xmlRequest6, Encoding.UTF8, "application/xml");

                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstanceTemplateRequest_RES06.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }
                
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlResponse);
                string countValue = xmlDoc.SelectSingleNode("//Rows").Attributes["Count"].Value;
                int.TryParse(countValue, out int count);

                responseModel.Result = new RES6Model { IsRecordExist = count > 0 };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10014;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10015;
            }
            return responseModel;
        }
        private string GetXMLHeader(string ind)
        {
            return $@"<Header>
                        <User>{goEfficientCredentials.Username}</User>
                        <PassWord>{goEfficientCredentials.Password}</PassWord>
                        <Culture>{goEfficientCredentials.Culture}</Culture>
		                <DataserverName>{goEfficientCredentials.DataserverName}</DataserverName>
                        <RequestId>{ind}</RequestId>
                        <ContinueOnError>true</ContinueOnError>
                    </Header>";
        }
        private string GetYearWeekISO(DateTime date)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dfi.Calendar;

            int week = calendar.GetWeekOfYear(date, dfi.CalendarWeekRule, DayOfWeek.Monday);

            // Handle the edge case where the day is in the first week of the year, but the year is the previous one.
            if (date.Month == 12 && week == 1)
            {
                return $"{date.Year + 1}-{week:00}";
            }
            return $"{date.Year}-{week:00}";
        }

		public async Task<ResponseModel<Dictionary<string, object>>> FillDataInBeheerderAttributesDictionary(TaskFetchResponseModel model)
		{
			var responseModel = new ResponseModel<Dictionary<string, object>>();
			try
			{
				Dictionary<string, object?> beheerderAttributes = new Dictionary<string, object>();
				if (_configuration.GetSection("WMSBeheerderAttributes").GetChildren().Any(x => x.Key == model.action))
				{
					beheerderAttributes = _configuration.GetSection($"WMSBeheerderAttributes:{model.action}")
						.GetChildren()
						.ToDictionary(x => x.Key, x => (object?) x.Value);
				}

				responseModel.Result = beheerderAttributes;
				responseModel.IsSuccess = true;
			}			
			catch (Exception ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 10018;
			}
			return responseModel;
		}
	}

   
}
