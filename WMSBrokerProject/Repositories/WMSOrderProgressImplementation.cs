﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;
using WMSBrokerProject.TaskIndicationModels;

namespace WMSBrokerProject.Repositories
{
    public class WMSOrderProgressImplementation : IOrderProgressService
    {
        private readonly string orgId;
        private readonly string token;
        private readonly string baseAddress;
        private readonly string? templateFolder;
        private readonly string symbolForConcatenation;
        private readonly string symbolForPriority;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly GoEfficientCredentials goEfficientCredentials;
        private readonly OrderProgressConfigurationModel orderProgressSettings;
        private readonly ICorrelationServices correlationServices;
        //private readonly OrderProgressMappingOptions _orderProgressMappingOptions;



        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string endpointUrl;
        public WMSOrderProgressImplementation(IConfiguration configuration, IWebHostEnvironment hostEnvironment,
            IOptions<GoEfficientCredentials> goEfficientCredentials, ICorrelationServices correlationServices)
        {
            orderProgressSettings = new OrderProgressConfigurationModel();
            var templatesSection = configuration.GetSection("OrderProgressTemplates");
            foreach (var templateSection in templatesSection.GetChildren())
            {
                var template = new OrderProgressTemplate();
                templateSection.Bind(template);
                orderProgressSettings.OrderProgressTemplates ??= new Dictionary<string, OrderProgressTemplate>(StringComparer.OrdinalIgnoreCase);
                orderProgressSettings.OrderProgressTemplates[templateSection.Key] = template;
            }
            //_orderProgressMappingOptions = new OrderProgressMappingOptions
            //{
            //    OrderProgressMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            //};
            //var configSection = configuration.GetSection("OrderProgressMapping");
            //var configValues = configSection?.GetChildren().ToList();

            //if (configValues != null)
            //{
            //    foreach (var item in configValues)
            //    {
            //        _orderProgressMappingOptions.OrderProgressMapping[item.Key] = item.Value;
            //    }
            //}
            this.correlationServices = correlationServices;
            _configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.goEfficientCredentials = goEfficientCredentials.Value;
            this.symbolForConcatenation = configuration.GetSection("MijnAansluiting:SymbolForConcatenation").Value ?? "+";
            this.symbolForPriority = configuration.GetSection("MijnAansluiting:SymbolForPriority").Value ?? "*";
            this.templateFolder = configuration.GetSection("TemplatesFolder").Value;
            this.clientId = configuration.GetSection("TrackTrace:YOUR_CLIENT_ID").Value!;
            this.clientSecret = configuration.GetSection("TrackTrace:YOUR_CLIENT_SECRET").Value!;
            this.endpointUrl = configuration.GetSection("TrackTrace:EndPointUrl").Value!;
            this.orgId = configuration.GetSection("orgId").Value!;
            this.token = configuration.GetSection("token").Value!;
            this.baseAddress = configuration.GetSection("BaseAddress").Value ?? throw new Exception("Base Address of WMS Not found");
        }

        public async Task<ResponseModel<OrderProcessingTemplateResponse>> GetTemplateIds()
        {
            var responseModel = new ResponseModel<OrderProcessingTemplateResponse>();
            try
            {
                List<TemplateClass> templates = new List<TemplateClass>();
                var orderProgressTemplates = orderProgressSettings.OrderProgressTemplates;

                if (orderProgressTemplates is not null)
                {
                    foreach (var template in orderProgressTemplates)
                    {
                        if (template.Value != null)
                        {
                            templates.Add(new TemplateClass
                            {
                                TemplateKey = template.Key,
                                ActionType = template.Value.ActionType,
                                GoEfficientStatus = template.Value.GoEfficientStatus,
                                TemplateID = template.Value.TemplateID,
                                WMSStatus = template.Value.WMSStatus
                            });
                        }
                    }
                    responseModel.Result = new OrderProcessingTemplateResponse
                    {
                        Templates = templates
                    };
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.ErrorMessage = "No order processing template found";
                }
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30001;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES7Model>> REQ7GetPro_IDs(REQ7Model model)
        {
            var responseModel = new ResponseModel<RES7Model>();
            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest7 = $@"<Request>
                                            {GetXMLHeader(model.RequestId)}
                                            <Body>
                                                <ReadOperation>
                                                    <Fields>
                                                        <Field>PRO.PRO_ID</Field>
                                                        <Field>PRO.PRO_OPENED</Field>
                                                        <Field>PRO.PRO_PRO_ID_DESC</Field>
                                                        <Field>PRO.PRO_CLOSED</Field>
                                                        <Field>PRO.PRO_TEMPLATE_ID</Field>
                                                    </Fields>
                                                    <Conditions>
                                                        <Condition RightVariableType=""LiteralValue"" RightValue=""{model.TemplateId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""PRO.PRO_TEMPLATE_ID""/>
                                                        <Condition RightVariableType=""LiteralScript"" RightValue=""'1753-01-01'"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""COALESCE(PRO.PRO_CLOSED,'1753-01-01')""/>
                                                    </Conditions>
                                                    <OperationName>PRO_READ_M_V1</OperationName>
                                                </ReadOperation>
                                            </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest7, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_ReadOpenTasksByTemplateID_RES7.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                XDocument doc = XDocument.Parse(xmlResponse);
                XNamespace ns = doc.Root.GetDefaultNamespace();

                List<RES7ProIDClass> pro_Ids = new();
                foreach (var row in doc.Descendants(ns + "Row"))
                {
                    var data = new RES7ProIDClass
                    {
                        Pro_Id = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_ID")?.Value,
                        ProIdDESC = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_PRO_ID_DESC")?.Value,
                        Opened = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_OPENED")?.Value,
                        Closed = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_CLOSED")?.Value
                    };
                    pro_Ids.Add(data);
                }

                responseModel.Result = new RES7Model
                {
                    Pro_IdList = pro_Ids
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30002;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30003;
            }
            return responseModel;
        }

        private string GetValueForKeyInFCField(string inputString, string keyToSearch)
        {
            var keyValuePairs = inputString.Split(';');
            foreach (var pair in keyValuePairs)
            {
                int startIndex = pair.IndexOf('<');
                int endIndex = pair.IndexOf('>');
                if (startIndex != -1 && endIndex != -1)
                {
                    var extractedKey = pair.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                    if (string.Equals(extractedKey, keyToSearch, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Substring(endIndex + 1).Trim('=', ';');
                    }
                }
            }
            return null;
        }

        public async Task<ResponseModel<OPRES4aModel>> REQ4a_GetInID(OrderProcessingREQ4aModel model)
        {
            var responseModel = new ResponseModel<OPRES4aModel>();

            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;
                string xmlRequest4a = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.RequestId)}
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
				                                        <Field>FIN.FIN_ADRESS_ID</Field>
				                                        <Field>FIN.FIN_MEMO</Field>
				                                        <Field>FIN.FIN_FILE_EXT</Field>
				                                        <Field>UDF.UDF_TYPEINFO</Field>
				                                        <Field>UDF.UDF_LABEL</Field>
				                                        <Field>PRO.PRO_ID</Field>
                                                        <Field>PRO.PRO_START</Field>
				                                        <Field>PRO.PRO_DEADLINE</Field>
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
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstantiatedAttacments_TTRES04a.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                RES4aTemplate template = new RES4aTemplate();
                XDocument xdoc = XDocument.Parse(xmlResponse);
                Dictionary<string, object?> selectListItems = new();
                var taskId = xdoc.Descendants("Row")
                            .Where(row => row.Elements("Value")
                                    .Any(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NAME" && e.Value == "CIFWMS-OrderUid"))
                            .Select(row => row.Elements("Value")
                                    .FirstOrDefault(e => e.Attribute("FieldName")!.Value == "FIN.FIN_PATH")?.Value)
                            .FirstOrDefault();
                var updateCount = xdoc.Descendants("Row")
                            .Where(row => row.Elements("Value")
                                    .Any(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NAME" && e.Value == "CIFWMS-updateCount"))
                            .Select(row => row.Elements("Value")
                                    .FirstOrDefault(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NUMBER")?.Value)
                            .FirstOrDefault();
                var priority = xdoc.Descendants("Row")
                            .Where(row => row.Elements("Value")
                                    .Any(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NAME" && e.Value == "CIFWMS-Urgency"))
                            .Select(row => row.Elements("Value")
                                    .FirstOrDefault(e => e.Attribute("FieldName")!.Value == "FIN.FIN_PATH")?.Value)
                            .FirstOrDefault();
                selectListItems.Add(key: "taskId", value: taskId);
                selectListItems.Add(key: "updateCount", value: updateCount);
                selectListItems.Add(key: "priority", value: priority);

                var firstRowFields = (from row in xdoc.Descendants("Row")
                                      let description = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "PRO.PRO_START")?.Value
                                      let templateId = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "PRO.PRO_DEADLINE")?.Value
                                      select new Res4aRowFields
                                      {
                                          Pro_Description = description,
                                          Pro_Template_Id = templateId
                                      }).FirstOrDefault();
                Res4aRowFields? res4ARowFields;
                if (firstRowFields is null ||
                    (firstRowFields is not null &&
                    ((string.IsNullOrEmpty(firstRowFields.Pro_Description) ||
                    string.IsNullOrEmpty(firstRowFields.Pro_Template_Id)) &&
                    model.Template.WMSStatus.ToLower() == "wip")))
                {
                    res4ARowFields = null;
                }
                else
                {
                    res4ARowFields = firstRowFields;
                }
                var actionContainingRow = (from row in xdoc.Descendants("Row")
                                      let finName = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value
                                      where finName == "CIFWMS-Aft-Action"
                                      let finPath = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_PATH")?.Value
                                      let udfTypeInfo = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPEINFO")?.Value
                                      select new Res4aActionFields
                                      {
                                          FIN_NAME = finName,
                                          FIN_PATH = finPath,
                                          UDF_TYPEINFO = udfTypeInfo
                                      }).FirstOrDefault();

                string? actionName = null;
                if (actionContainingRow != null)
                {
                    Dictionary<string, string> selectOptions = new();
                    var decodedUDFTypeInfo = System.Net.WebUtility.HtmlDecode(actionContainingRow.UDF_TYPEINFO);
                    if (decodedUDFTypeInfo.StartsWith("SEL:", StringComparison.OrdinalIgnoreCase))
                    {
                        decodedUDFTypeInfo = decodedUDFTypeInfo.Substring("SEL:".Length);
                    }
                    var keyValuePairs = decodedUDFTypeInfo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Split('=')).ToList();
                    foreach (var keyValuePair in keyValuePairs)
                    {
                        if (keyValuePair.Length == 2)
                        {
                            var value = keyValuePair[0].Trim();
                            value = value.Replace("<", "").Replace(">", "").Trim();
                            var text = keyValuePair[1].Trim();
                            if (selectOptions.Keys.Contains(text) && selectOptions.Values.Contains(value))
                            {
                                continue;
                            }
                            else
                            {
                                selectOptions.Add(key: value, value: text);
                            }
                        }
                    }
                    selectOptions.TryGetValue(actionContainingRow.FIN_PATH, out actionName);
                }
                
                
                responseModel.Result = new OPRES4aModel
                {
                    InID = taskId ?? string.Empty,
                    Res4ARowFields = res4ARowFields,
                    SelectListItems = selectListItems,
                    ActionName = actionName
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30004;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30005;
            }
            return responseModel;
        }
        public async Task<ResponseModel<Res4aGetTemplateModel>> REQ4a_GetTemplateData(REQ4aGetTemplateModel model)
        {
            var responseModel = new ResponseModel<Res4aGetTemplateModel>();

            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;
                string xmlRequest4a = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.RequestId)}
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
				                                        <Field>FIN.FIN_ADRESS_ID</Field>
				                                        <Field>FIN.FIN_MEMO</Field>
				                                        <Field>FIN.FIN_FILE_EXT</Field>
				                                        <Field>UDF.UDF_TYPEINFO</Field>
				                                        <Field>UDF.UDF_LABEL</Field>
				                                        <Field>PRO.PRO_ID</Field>
                                                        <Field>PRO.PRO_START</Field>
				                                        <Field>PRO.PRO_DEADLINE</Field>
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


                XDocument xdoc = XDocument.Parse(xmlResponse);
                //var templateDictionary = new Dictionary<string, string>();
                //var dataRows = xdoc.Descendants("Row")
                //            .Select(row => new KeyValuePair<string,string>
                //            (key: row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_LABEL")?.Value!,
                //            value: row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_RECORD_ID")?.Value!)).ToList();
                //foreach (var data in dataRows)
                //{
                //    if (!templateDictionary.ContainsKey(data.Key)) templateDictionary.Add(data.Key, data.Value);
                //}


                var templates = (from row in xdoc.Descendants("Row")
                                 let udfFIN_IDValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_ID")?.Value
                                 let udfTypeValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPE")?.Value
                                 let finNameValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value
                                 let finRecordIdValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_RECORD_ID")?.Value
                                 let finPathValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_PATH")?.Value
                                 let finDateValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_DATE")?.Value
                                 let finNumberValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_NUMBER")?.Value
                                 let finMemoValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_MEMO")?.Value
                                 let finFileExtValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_FILE_EXT")?.Value
                                 let udfTypeInfoValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPEINFO")?.Value
                                 let udfLabelValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_LABEL")?.Value
                                 let proIdValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "PRO.PRO_ID")?.Value
                                 let fin_Address_Id = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_ADRESS_ID")?.Value

                                 select new RES4aTemplateFields
                                 {
                                     FIN_ID = udfFIN_IDValue,
                                     UDF_TYPE = udfTypeValue,
                                     FIN_NAME = finNameValue,
                                     FIN_RECORD_ID = finRecordIdValue,
                                     FIN_PATH = finPathValue,
                                     FIN_DATE = finDateValue,
                                     FIN_NUMBER = finNumberValue,
                                     FIN_MEMO = finMemoValue,
                                     FIN_FILE_EXT = finFileExtValue,
                                     UDF_TYPEINFO = udfTypeInfoValue,
                                     UDF_LABEL = udfLabelValue,
                                     PRO_ID = proIdValue,
                                     FIN_ADRESS_ID = fin_Address_Id
                                 }).ToList();

                List<Fin_AddressClass> addresses = templates.Where(s => s.UDF_TYPE == "A" && !string.IsNullOrEmpty(s.FIN_ADRESS_ID))
                                .Select(s => new Fin_AddressClass
                                {
                                    FIN_ADRESS_ID = s.FIN_ADRESS_ID,
                                    FIN_ID = s.FIN_ID,
                                    FIN_NAME = s.FIN_NAME
                                }).ToList();

                responseModel.Result = new Res4aGetTemplateModel
                {
                    Templates = templates,
                    Addresses = addresses
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30006;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30007;
            }
            return responseModel;
        }

        public async Task<ResponseModel<UIARES5Model>> REQ05_UpdateInstantiatedAttachmentsRequest(UIAREQ5Model model)
        {
            var responseModel = new ResponseModel<UIARES5Model>();
            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;
                var date = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

                var xmlRequest5 = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.RequestId)}
                                            <Body>
		                                        <UpdateOperation>
                                                    <OperationName>PRO_FIN_UPDATE</OperationName>
                                                    <Values>
                                                        <Value FieldName=""CIFWMS-Aft-Status"">{model.Status}</Value>
                                                    </Values>
                                                    <Conditions>
                                                        <Condition FieldName=""PRO.PRO_ID"">{model.ProId}</Condition>
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
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_RES7aOpenTask.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                XDocument doc = XDocument.Parse(xmlResponse);

                responseModel.Result = new UIARES5Model { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30009;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES8Model>> REQ08_ReadAddress(REQ8Model model)
        {
            var responseModel = new ResponseModel<RES8Model>();
            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;
                var date = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

                var xmlRequest5 = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.RequestId)}
                                         <Body>
                                            <ReadOperation>
                                                <Fields>
                                                </Fields>
                                                <Conditions>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""{model.AddressId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""ADRESS.ADRESS_ID""/>
                                                </Conditions>
                                                <OperationName>ADRESS_READ_M_V1</OperationName>
                                            </ReadOperation>
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
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                XDocument xdoc = XDocument.Parse(xmlResponse);

                var templates = (from row in xdoc.Descendants("Row")
                                 let streetNameValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "ADRESS_STREET")?.Value
                                 let cityNameValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "ADRESS_TOWN")?.Value
                                 let countryValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "COUNTR_ISO3166_A3")?.Value
                                 let houseNumberValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "ADRESS_HOUSNR")?.Value
                                 let houseNumberExtensionValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "ADRESS_HOUSNR_SFX")?.Value
                                 let postalCodeValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "ADRESS_ZIPCODE")?.Value

                                 select new RES48AddressFields
                                 {
                                     StreetName = streetNameValue,
                                     CityName = cityNameValue,
                                     Country = countryValue,
                                     HouseNumber = houseNumberValue,
                                     HouseNumberExtension = houseNumberExtensionValue,
                                     PostalCode = postalCodeValue
                                 }).ToList();

                responseModel.Result = new RES8Model
                {
                    AddressFields = templates
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30009;
            }
            return responseModel;
        }

        public async Task<ResponseModel<TaskIndicationResponseModel>> RequestTaskIndication(TaskIndicationRequestModel model)
        {
            var responseModel = new ResponseModel<TaskIndicationResponseModel>();
            try
            {
                var correlationID = Guid.NewGuid().ToString();
                var correlationItem = new Repositories.CorrelationItem
                {
                    TaskId = model.taskId,
                    CorrelationID = correlationID
                };
                correlationServices.SaveCorrelationItem(correlationItem);

                using HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseAddress); //Url form wmssetteing
                httpClient.DefaultRequestHeaders.Add("CorrelationID", correlationID);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var dataJson = JsonConvert.SerializeObject(model);
                var content = new StringContent(dataJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"wms-beheerder-api/contractor/{orgId}/tasks", content); //OrgId form setting
                //HttpResponseMessage response = await httpClient.PostAsync($"wms-beheerder-api/contractor/Circet/tasks/{model.taskId}", content);
                if (response.IsSuccessStatusCode)
                {
                    //string responseContent = await response.Content.ReadAsStringAsync();
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.ErrorCode = (int)response.StatusCode;
                    responseModel.ErrorMessage = $"Task Sync call failure with status/code:{response.StatusCode}";

                }
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30010;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30011;
            }
            return responseModel;
        }

        public async Task<ResponseModel<TTRES4Model>> REQ4_TrackAndTrace(TTREQ4Model model)
        {
            var responseModel = new ResponseModel<TTRES4Model>();
            try
            {
                //var endpointAddress = new EndpointAddress(endpointUrl);
                //var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                //int resultCode = 0;
                //using (var client = new TracknTraceServicePortClient(binding, endpointAddress))
                //{
                //    client.ClientCredentials.UserName.UserName = clientId;
                //    client.ClientCredentials.UserName.Password = clientSecret;
                //    //client.Endpoint.EndpointBehaviors.Add(new LoggingEndpointBehavior());

                //    var isSuccess = Enum.TryParse(typeof(ProcesStatus), "Item" + model.Status.Replace(".", ""), out object? statusObject);

                //    if (isSuccess)
                //    {
                //        var result = await client.BijwerkenStatusAsync(new BijwerkenStatusInput
                //        {
                //            BijwerkenStatusRequest = new BijwerkenStatusRequest
                //            {
                //                Header = GetHeaderObject(),
                //                BijwerkenStatus = new BijwerkenStatusRequestBijwerkenStatus
                //                {
                //                    AanvraagId = model.AanvraagId,
                //                    DisciplineId = model.DisciplineId,
                //                    ObjectId = model.ObjectId,
                //                    Status = (ProcesStatus)statusObject,
                //                    StatusSpecified = true
                //                }
                //            }
                //        });
                //        resultCode = result.BijwerkenStatusResponse.Result.ResultCode;
                //    }
                //}
                var resultCode = 0;
                if (resultCode == 0)
                {
                    responseModel.Result = new TTRES4Model { ResultCode = resultCode };
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.IsSuccess = false;
                    responseModel.ErrorCode = 30012;
                }
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30013;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30014;
            }
            return responseModel;
        }

        public async Task<ResponseModel<CTRES7aModel>> REQ7a(CTREQ7aModel model)
        {
            var responseModel = new ResponseModel<CTRES7aModel>();
            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;
                var date = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

                var xmlRequest7a = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.RequestId)}
                                            <Body>
                                                <UpdateOperation>
                                                    <OperationName>PRO_UPDATE_V1</OperationName>
                                                    <Values>
                                                        <Value FieldName=""PRO.PRO_OPENED"">{date}</Value>
                                                        <Value FieldName=""PRO.PRO_CLOSED"">{date}</Value>
                                                    </Values>
                                                    <Conditions>
                                                        <Condition FieldName=""PRO.PRO_ID"">{model.ProId}</Condition>
                                                    </Conditions>
                                                </UpdateOperation>
                                            </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest7a, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_RES7aOpenTask.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                XDocument doc = XDocument.Parse(xmlResponse);

                responseModel.Result = new CTRES7aModel { };
                responseModel.IsSuccess = true;

                //var proId = doc.Descendants("Row")
                //			.Where(row => row.Elements("Value")
                //					.Any(e => e.Attribute("FieldName").Value == "PRO.PRO_ID"))
                //			.Select(row => row.Elements("Value")
                //					.FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_ID")?.Value)
                //			.FirstOrDefault();

                //if (proId is not null)
                //{
                //	responseModel.Result = new TTRES7aModel { };
                //	responseModel.IsSuccess = true;
                //}
                //else
                //{
                //	responseModel.IsSuccess = false;
                //	responseModel.ErrorMessage = "Pro.Pro_ID not received";
                //	responseModel.ErrorCode = 40009;
                //}
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30015;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30016;
            }
            return responseModel;
        }
        private string GetXMLHeader(string RequestId)
        {
            return $@"<Header>
                        <User>{goEfficientCredentials.Username}</User>
                        <PassWord>{goEfficientCredentials.Password}</PassWord>
                        <Culture>{goEfficientCredentials.Culture}</Culture>
		                <DataserverName>{goEfficientCredentials.DataserverName}</DataserverName>
                        <RequestId>{RequestId}</RequestId>
                        <ContinueOnError>true</ContinueOnError>
                    </Header>";
        }

        private async Task<ResponseModel<Dictionary<string, string>>> GetWMSBeheerderAttributesByActionName(string actionName)
        {
            var responseModel = new ResponseModel<Dictionary<string, string>>();
            try
            {
                var resultDictionary = new Dictionary<string, string>();
                var wMSBeheerderAttributesSection = _configuration.GetSection("WMSBeheerderAttributes");
                if (wMSBeheerderAttributesSection != null)
                {
                    var actionSection = wMSBeheerderAttributesSection.GetSection(actionName.ToLowerInvariant());
                    if (actionSection != null)
                    {
                        foreach (var child in actionSection.GetChildren())
                        {
                            resultDictionary[child.Key] = child.Value ?? "";
                        }
                    }
                }
                responseModel.Result = resultDictionary;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30017;
            }
            return responseModel;
        }

        private async Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientAttributes()
        {
            var responseModel = new ResponseModel<Dictionary<string, string>>();
            try
            {
                var resultDictionary = new Dictionary<string, string>();
                var goEfficientAttributesSection = _configuration.GetSection("GoEfficientAttributes");
                if (goEfficientAttributesSection != null)
                {
                    foreach (var child in goEfficientAttributesSection.GetChildren())
                    {
                        resultDictionary[child.Key] = child.Value ?? "";
                    }
                }
                responseModel.Result = resultDictionary;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30018;
            }
            return responseModel;
        }
        private string GetValueFrom4aResponseRow(RES4aTemplateFields row)
        {
            var result = string.Empty;
            switch (row.UDF_TYPE)
            {
                case "T":
                    result = row.FIN_PATH;
                    break;
                case "D":
                    result = row.FIN_DATE;
                    break;
                case "N":
                    result = row.FIN_NUMBER;
                    break;
                case "FC":
                    var code = row.FIN_PATH;
                    result = GetValueForKeyInFCField(row.UDF_TYPEINFO, code);
                    break;
                    //case "DT":
                    //    dataDictionary.Add(keyFromMappingDict, item.FIN_NUMBER);
                    //    break;
                    //case "B":
                    //    dataDictionary.Add(keyFromMappingDict, item.FIN_NUMBER);
                    //    break;
                    //case "A":
                    //    dataDictionary.Add(keyFromMappingDict, item.FIN_NUMBER);
                    //    break;
            }
            return result;
        }
        private async Task<ResponseModel<List<TaskFetchResponseMappedModel>>> MapDataForTaskFetchResponse(List<RES4aTemplateFields> dataList,
            Dictionary<string, object?> wmsBeheerderMapping, Dictionary<string, string> wmsBeheerderAttributes)
        {
            var responseModel = new ResponseModel<List<TaskFetchResponseMappedModel>>();
            try
            {
                var mappedList = new List<TaskFetchResponseMappedModel>();
                foreach (var item in dataList)
                {
                    if (wmsBeheerderMapping.TryGetValue(item.FIN_NAME, out var value))
                    {
                        if (value is not null && wmsBeheerderAttributes.TryGetValue(value.ToString(), out var wmsBeheerderActionPath))
                        {
                            mappedList.Add(new TaskFetchResponseMappedModel
                            {
                                FIN_NAME = item.FIN_NAME,
                                WMSBeheerderActionPath = wmsBeheerderActionPath, //contains path
                                Value = GetValueFrom4aResponseRow(item)
                            });
                        }
                    }
                }
                responseModel.Result = mappedList;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30019;
            }
            return responseModel;
        }
        public async Task<ResponseModel<WMSBeheerderRES2MappingClass>> GetWMSBeheerderRES2Mapping(string actionName)
        {
            var responseModel = new ResponseModel<WMSBeheerderRES2MappingClass>();
            try
            {
                var section = _configuration.GetSection($"WMSBeheerderRES2Mapping:Generic");
                var mappedValues = new Dictionary<string, object?>();
                foreach (var config in section.GetChildren())
                {
                    mappedValues.Add(config.Key, config.Value);
                }

                section = _configuration.GetSection($"WMSBeheerderRES2Mapping:{actionName}");
                foreach (var config in section.GetChildren())
                {
                    mappedValues.Add(config.Key, config.Value);
                }
                section = _configuration.GetSection($"WMSBeheerderRES2FCMapping");
                foreach (var config in section.GetChildren())
                {
                    mappedValues.Add(config.Key, config.Value);
                }

                responseModel.Result = new WMSBeheerderRES2MappingClass
                {
                    WMSBeheerderRES2MappingDictionary = mappedValues
                };
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10004;
            }
            return responseModel;
        }
        private string? GetValueForKeyFromSection(string keyName, string sectionName, bool isCaseSensitive = false)
        {
            string? value = null;
            var section = _configuration.GetSection(sectionName);
            if (section != null)
            {
                if (isCaseSensitive)
                {
                    value = section.GetValue<string>(keyName);
                }
                else
                {
                    var lowercaseKeyToCheck = keyName.ToLower();
                    if (section.GetChildren().Any(kv => kv.Key.ToLower() == lowercaseKeyToCheck))
                    {
                        value = section.GetValue<string>(keyName);
                    }
                }
            }
            return value;
        }
        private Dictionary<string, object?> GetKeyValueAsDictionary(string sectionName, bool ignoreNullValue = true)
        {
            var sectionDictionary = new Dictionary<string, object?>();
            var section = _configuration.GetSection(sectionName);
            if (section != null)
            {
                foreach (var config in section.GetChildren())
                {
                    if (!ignoreNullValue || config.Value != null)
                    {
                        sectionDictionary.Add(config.Key, config.Value);
                    }
                }
            }
            return sectionDictionary;
        }
        public async Task<ResponseModel<Dictionary<string, object?>>> GetWMSBeheerderAddressPaths(string addressKeyName)
        {
            var responseModel = new ResponseModel<Dictionary<string, object?>>();
            try
            {
                var addressValue = GetValueForKeyFromSection(addressKeyName, "WMSBeheerderRES4AddressMapping");
                var addressPaths = GetKeyValueAsDictionary($"WMSBeheerderAddresses:{addressValue}");
                responseModel.Result = addressPaths;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30020;
            }
            return responseModel;
        }
        public async Task<ResponseModel<JObject>> GetJsonResultForTaskFetchResponse(
            Res4aGetTemplateModel templateModel, string actionName, List<TaskFetchResponseAddressMappedModel> addressMappedPaths)
        {
            var responseModel = new ResponseModel<JObject>();

            try
            {
                var wmsBeheerderAttributesResponse = await GetWMSBeheerderAttributesByActionName(actionName).ConfigureAwait(false);
                if (!wmsBeheerderAttributesResponse.IsSuccess) { throw new Exception($"Error Code: {wmsBeheerderAttributesResponse.ErrorCode}; Error Message: {wmsBeheerderAttributesResponse.ErrorMessage}"); }

                var wmsBeheerderMappingResponse = await GetWMSBeheerderRES2Mapping(actionName).ConfigureAwait(false);

                if (!wmsBeheerderMappingResponse.IsSuccess) { throw new Exception($"Error Code: {wmsBeheerderMappingResponse.ErrorCode}; Error Message: {wmsBeheerderMappingResponse.ErrorMessage}"); }

                var mapDataForTaskFetchResponse = await MapDataForTaskFetchResponse(
                    templateModel.Templates, wmsBeheerderMappingResponse.Result.WMSBeheerderRES2MappingDictionary!,
                    wmsBeheerderAttributesResponse.Result!).ConfigureAwait(false);
                if (!mapDataForTaskFetchResponse.IsSuccess) { throw new Exception($"Error Code: {mapDataForTaskFetchResponse.ErrorCode}; Error Message: {mapDataForTaskFetchResponse.ErrorMessage}"); }
                JObject resultObject = new JObject();
                foreach (var item in mapDataForTaskFetchResponse.Result)
                {
                    if (item.Value is null || item.Value == "") continue;
                    var propertyNames = item.WMSBeheerderActionPath.Split('.');
                    BuildJsonStructure(resultObject, propertyNames, item.Value);
                }
                foreach (var item in addressMappedPaths)
                {
                    if (item.Value is null || item.Value == "") continue;
                    var propertyNames = item.WMSBeheerderAddressPath.Split('.');
                    BuildJsonStructure(resultObject, propertyNames, item.Value);
                }

                responseModel.Result = resultObject;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 30020;
            }
            return responseModel;
        }
        private void BuildJsonStructure(JObject currentObject, string[] propertyNames, object value)
        {
            if (propertyNames.Length == 1)
            {
                currentObject[propertyNames[0]] = JToken.FromObject(value);// If it's the last property, add the value
            }
            else
            {
                var propertyName = propertyNames[0];
                if (!currentObject.ContainsKey(propertyName))
                {
                    currentObject[propertyName] = new JObject();
                }
                BuildJsonStructure((JObject)currentObject[propertyName], propertyNames[1..], value);
            }
        }
    }
}
