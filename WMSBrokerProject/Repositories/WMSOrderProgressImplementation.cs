using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Repositories
{
    public class WMSOrderProgressImplementation : IOrderProgressService
    {
        private readonly string? templateFolder;
        private readonly string symbolForConcatenation;
        private readonly string symbolForPriority;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly GoEfficientCredentials goEfficientCredentials;
        private readonly OrderProgressConfigurationModel orderProgressSettings;
        
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string endpointUrl;
        public WMSOrderProgressImplementation(IConfiguration configuration, IWebHostEnvironment hostEnvironment,
            IOptions<GoEfficientCredentials> goEfficientCredentials)
        {
            orderProgressSettings = new OrderProgressConfigurationModel();
            var templatesSection = configuration.GetSection("OrderProgressTemplates");
            foreach (var templateSection in templatesSection.GetChildren())
            {
                var template = new OrderProgressTemplate();
                templateSection.Bind(template);
                orderProgressSettings.OrderProgressTemplates ??= new Dictionary<string, OrderProgressTemplate>();
                orderProgressSettings.OrderProgressTemplates[templateSection.Key] = template;
            }

            _configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.goEfficientCredentials = goEfficientCredentials.Value;
            this.symbolForConcatenation = configuration.GetSection("MijnAansluiting:SymbolForConcatenation").Value ?? "+";
            this.symbolForPriority = configuration.GetSection("MijnAansluiting:SymbolForPriority").Value ?? "*";
            this.templateFolder = configuration.GetSection("TemplatesFolder").Value;
            this.clientId = configuration.GetSection("TrackTrace:YOUR_CLIENT_ID").Value!;
            this.clientSecret = configuration.GetSection("TrackTrace:YOUR_CLIENT_SECRET").Value!;
            this.endpointUrl = configuration.GetSection("TrackTrace:EndPointUrl").Value!;
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
                responseModel.ErrorCode = 60001;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES7Model>> REQ7GetTaskIDs(REQ7Model model)
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

                List<RES7ProIDClass> taskIds = new();
                foreach (var row in doc.Descendants(ns + "Row"))
                {
                    var data = new RES7ProIDClass
                    {
                        TaskId = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_ID")?.Value,
                        ProIdDESC = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_PRO_ID_DESC")?.Value,
                        Opened = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_OPENED")?.Value,
                        Closed = row.Elements(ns + "Value").FirstOrDefault(e => e.Attribute("FieldName").Value == "PRO.PRO_CLOSED")?.Value
                    };
                    taskIds.Add(data);
                }

                responseModel.Result = new RES7Model
                {
                    TaskIdList = taskIds
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 60002;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 60003;
            }
            return responseModel;
        }

        public async Task<ResponseModel<TTRES4aModel>> REQ4a_GetTemplateFromGoEfficient(OrderProcessingREQ4aModel model)
        {
            var responseModel = new ResponseModel<TTRES4aModel>();

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
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstantiatedAttacments_TTRES04a.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                RES4aTemplate template = new RES4aTemplate();
                XDocument xdoc = XDocument.Parse(xmlResponse);

                var disciplineId = xdoc.Descendants("Row")
                            .Where(row => row.Elements("Value")
                                    .Any(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NAME" && e.Value == "MA.nl-DisciplineID"))
                            .Select(row => row.Elements("Value")
                                    .FirstOrDefault(e => e.Attribute("FieldName")!.Value == "FIN.FIN_PATH")?.Value)
                            .FirstOrDefault();
                //var disciplineId = 1000222873;
                var objectId = xdoc.Descendants("Row")
                            .Where(row => row.Elements("Value")
                                    .Any(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NAME" && e.Value == "MA.nl-ObjectID"))
                            .Select(row => row.Elements("Value")
                                    .FirstOrDefault(e => e.Attribute("FieldName")!.Value == "FIN.FIN_PATH")?.Value)
                            .FirstOrDefault();
                var aanvraagId = xdoc.Descendants("Row")
                            .Where(row => row.Elements("Value")
                                    .Any(e => e.Attribute("FieldName")!.Value == "FIN.FIN_NAME" && e.Value == "MA.nl-AanvraagID"))
                            .Select(row => row.Elements("Value")
                                    .FirstOrDefault(e => e.Attribute("FieldName")!.Value == "FIN.FIN_PATH")?.Value)
                            .FirstOrDefault();

                responseModel.Result = new TTRES4aModel
                {
                    DisciplineId = disciplineId!,
                    AanvraagId = aanvraagId!,
                    ObjectId = objectId!
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 60004;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 60005;
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
                                                    <OperationName>PRO_UPDATE_V1</OperationName>
                                                    <Values>
                                                        <Value FieldName=""CIFWMS-AFT"">{model.Status}</Value>
                                                    </Values>
                                                    <Conditions>
                                                        <Condition FieldName=""PRO.PRO_ID""></Condition>
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
                responseModel.ErrorCode = 40006;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 40007;
            }
            return responseModel;
        }

        public async Task<ResponseModel<TaskIndicationResponseModel>> RequestTaskIndication(TaskIndicationRequestModel model)
        {
            var responseModel = new ResponseModel<TaskIndicationResponseModel>();
            try
            {
                using HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
                // httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");
                var dataJson = JsonConvert.SerializeObject(model);
                var content = new StringContent(dataJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await httpClient.PostAsync($"wms-beheerder-api/contractor/Circet/tasks/{model.inId}", content);
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
                responseModel.ErrorCode = 50008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 50009;
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
                    responseModel.ErrorCode = 60010;
                }
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 60011;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 60012;
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
                responseModel.ErrorCode = 40013;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 40014;
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
        //private HeaderType GetHeaderObject()
        //{
        //    var time = DateTime.UtcNow;
        //    return new HeaderType
        //    {
        //        MessageID = "1000053176",
        //        RepeatCount = 0,
        //        SenderID = "RBCIF",
        //        RecipientID = "LIP",
        //        MessageVersion = "v0103",
        //        //MessageID = Guid.NewGuid().ToString(),
        //        //RepeatCount = 0,
        //        //SenderID = "Waternet",
        //        //RecipientID = "LIP",
        //        //MessageVersion = "v0103",
        //        //SendTime = time,
        //        //CreateTime = time,
        //    };
        //}

    }
}
