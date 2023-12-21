using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using WMSBrokerProject.ConfigModels;

namespace WMSBrokerProject.Repositories
{
    public class WMSBeheerderImplementation : IWMSBeheerderService
	{
		private readonly string? templateFolder;
		private readonly string symbolForConcatenation;
		private readonly string symbolForPriority;
		private readonly string token;
		private readonly string orgId;
		private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment hostEnvironment;
		private readonly GoEfficientCredentials goEfficientCredentials;
		public WMSBeheerderImplementation(IConfiguration configuration, IWebHostEnvironment hostEnvironment,
			IOptions<GoEfficientCredentials> goEfficientCredentials)
		{
			//this.token = configuration.GetSection("token").Value!;
			this.token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im5rbWIiLCJuYW1laWQiOiJhN2M4YTAzYS0xYWU5LTQxYWEtOTA5ZC03Y2QwMWNiZTZjYzUiLCJyb2xlIjoiQmVoZWVyZGVyIiwiUGFydGllcyI6Ilt7XCJJZFwiOlwiMjQwXCIsXCJOYW1lXCI6XCJDaXJjZXRcIixcIlN5c3RlbU5hbWVcIjpcIk5LTVwiLFwiVHlwZVwiOjF9XSIsIm5ldHdvcmtPd25lcnMiOlsiREZOIiwiQ2l0aXVzIl0sIm5iZiI6MTcwMjM2Njc1NCwiZXhwIjoxNzMzOTIzNjgwLCJpYXQiOjE3MDIzNjY3NTR9.r1rndWf_X8fEtbnFdF-m22JtoyP0MxbBXVprLpdcVgY";
			this.orgId = configuration.GetSection("orgId").Value!;
			_configuration = configuration;
			this.hostEnvironment = hostEnvironment;
			this.goEfficientCredentials = goEfficientCredentials.Value;
			this.symbolForConcatenation = configuration.GetSection("MijnAansluiting:SymbolForConcatenation").Value ?? "+";
			this.symbolForPriority = configuration.GetSection("MijnAansluiting:SymbolForPriority").Value ?? "*";
			this.templateFolder = configuration.GetSection("TemplatesFolder").Value;
		}

        public async Task<ResponseModel<TaskFetchResponseModel>> Request2TaskFetch(REQ2Model model)
        {
            var responseModel = new ResponseModel<TaskFetchResponseModel>();
            try
            {
                Random rand = new Random();
                var correlationId = rand.Next(10000, 1000001).ToString();
                //var taskId = "WMS002530553";
                //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im5rbWIiLCJuYW1laWQiOiJhN2M4YTAzYS0xYWU5LTQxYWEtOTA5ZC03Y2QwMWNiZTZjYzUiLCJyb2xlIjoiQmVoZWVyZGVyIiwiUGFydGllcyI6Ilt7XCJJZFwiOlwiMjQwXCIsXCJOYW1lXCI6XCJDaXJjZXRcIixcIlN5c3RlbU5hbWVcIjpcIk5LTVwiLFwiVHlwZVwiOjF9XSIsIm5ldHdvcmtPd25lcnMiOlsiREZOIiwiQ2l0aXVzIl0sIm5iZiI6MTcwMjM2Njc1NCwiZXhwIjoxNzMzOTIzNjgwLCJpYXQiOjE3MDIzNjY3NTR9.r1rndWf_X8fEtbnFdF-m22JtoyP0MxbBXVprLpdcVgY";
                using HttpClient httpClient = new HttpClient();
                string? endPointUrl = " https://uat-gke.cif-operator.com/";
                string? requestUrl = Path.Combine(endPointUrl!, $"wms-beheerder-api/contractor/{orgId}/tasks/{model.InID}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //httpClient.DefaultRequestHeaders.Add("X-WMS-Test", "false");
                //httpClient.DefaultRequestHeaders.Add("X-Request-ID", model.InID);
                //httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
                //httpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
                //httpClient.DefaultRequestHeaders.Add("Cookie", "INGRESSCOOKIE=1701205238.251.103.617994|12428f53f11a724d940598e930467e0d");
                //// httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");

                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);

                    TaskFetchResponse taskFetchResponse = JsonConvert.DeserializeObject<TaskFetchResponse>(jsonResponse)!;
                    responseModel.Result = new TaskFetchResponseModel
                    {
                        TaskFetchResponseObject = taskFetchResponse,
                        JSONObject = jsonObject,
                    };
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.ErrorCode = (int)response.StatusCode;
                    responseModel.ErrorMessage = $"Task fetch call failure with status/code:{response.StatusCode}";
                }

                //string responseContent = File.ReadAllText("response2.json");
                //TaskFetchResponse taskFetchResponse = JsonConvert.DeserializeObject<TaskFetchResponse>(responseContent)!;
                //var jsonObject = JObject.Parse(responseContent);
                //responseModel.Result = new TaskFetchResponseModel
                //{
                //    TaskFetchResponseObject = taskFetchResponse,
                //    JSONObject = jsonObject
                //};
                //responseModel.IsSuccess = true;

                //var client = new HttpClient();
                //var request = new HttpRequestMessage(HttpMethod.Get, "https://uat-gke.cif-operator.com/wms-beheerder-api/contractor/Circet/tasks/WMS002530553");
                //request.Headers.Add("X-WMS-Test", "false");
                //request.Headers.Add("X-Request-ID", model.InID);
                //request.Headers.Add("X-Correlation-ID", correlationId);
                //request.Headers.Add("Accept", "text/plain");
                //request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im5rbWIiLCJuYW1laWQiOiJhN2M4YTAzYS0xYWU5LTQxYWEtOTA5ZC03Y2QwMWNiZTZjYzUiLCJyb2xlIjoiQmVoZWVyZGVyIiwiUGFydGllcyI6Ilt7XCJJZFwiOlwiMjQwXCIsXCJOYW1lXCI6XCJDaXJjZXRcIixcIlN5c3RlbU5hbWVcIjpcIk5LTVwiLFwiVHlwZVwiOjF9XSIsIm5ldHdvcmtPd25lcnMiOlsiQ2l0aXVzIiwiREZOIl0sIm5iZiI6MTY5NTk1NDMwNiwiZXhwIjoxNzI3NTExMjMyLCJpYXQiOjE2OTU5NTQzMDZ9.djzPpKcbSWOIV2MFw4VXxvjoSPDGSMwNMwO9LP3nVhI");
                //request.Headers.Add("Cookie", "INGRESSCOOKIE=1701205238.251.103.617994|12428f53f11a724d940598e930467e0d");
                //var response = await client.SendAsync(request);
                //response.EnsureSuccessStatusCode();
                //var result = await response.Content.ReadAsStringAsync();

            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 50001;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 50002;
            }
            return responseModel;
        }
        public async Task<ResponseModel<TaskFetchResponseModel>> Request2TaskFetchOld(REQ2Model model)
		{
			var responseModel = new ResponseModel<TaskFetchResponseModel>();
			try
			{
                //string responseContent = File.ReadAllText("response2.json");
                //TaskFetchResponseModel taskFetchResponse = JsonConvert.DeserializeObject<TaskFetchResponseModel>(responseContent)!;
                //responseModel.Result = taskFetchResponse;
                //responseModel.IsSuccess = true;
                //+++++++++++++++UnComment Following lines in live environment and comment above lines
                model.InID = "9245949";//this line to be removed
                using HttpClient httpClient = new HttpClient();
                string? endPointUrl = "https://uat-gke.cif-operator.com/";
                string? requestUrl = Path.Combine(endPointUrl!, $"wms-beheerder-api/contractor/{orgId}/tasks/{model.InID}");

                //httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");

                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    TaskFetchResponseModel taskFetchResponse = JsonConvert.DeserializeObject<TaskFetchResponseModel>(responseContent)!;
                    responseModel.Result = taskFetchResponse;
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.ErrorCode = (int)response.StatusCode;
                    responseModel.ErrorMessage = $"Task fetch call failure with status/code:{response.StatusCode}";
                }
                //            Random rand = new Random();
                //            var correlationId = rand.Next(10000, 1000001).ToString();


                //            var client = new HttpClient();
                //            var request = new HttpRequestMessage(HttpMethod.Get, "https://uat-gke.cif-operator.com/wms-beheerder-api/contractor/Circet/tasks/WMS002530553");
                //            request.Headers.Add("X-WMS-Test", "false");
                //            request.Headers.Add("X-Request-ID", model.InID);
                //            request.Headers.Add("X-Correlation-ID", correlationId);
                //            request.Headers.Add("Accept", "text/plain");
                //            request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im5rbWIiLCJuYW1laWQiOiJhN2M4YTAzYS0xYWU5LTQxYWEtOTA5ZC03Y2QwMWNiZTZjYzUiLCJyb2xlIjoiQmVoZWVyZGVyIiwiUGFydGllcyI6Ilt7XCJJZFwiOlwiMjQwXCIsXCJOYW1lXCI6XCJDaXJjZXRcIixcIlN5c3RlbU5hbWVcIjpcIk5LTVwiLFwiVHlwZVwiOjF9XSIsIm5ldHdvcmtPd25lcnMiOlsiQ2l0aXVzIiwiREZOIl0sIm5iZiI6MTY5NTk1NDMwNiwiZXhwIjoxNzI3NTExMjMyLCJpYXQiOjE2OTU5NTQzMDZ9.djzPpKcbSWOIV2MFw4VXxvjoSPDGSMwNMwO9LP3nVhI");
                //            request.Headers.Add("Cookie", "INGRESSCOOKIE=1701205238.251.103.617994|12428f53f11a724d940598e930467e0d");
                //            var response = await client.SendAsync(request);
                //            response.EnsureSuccessStatusCode();
                //var result = await response.Content.ReadAsStringAsync();

            }
            catch (HttpRequestException ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 50001;
			}
			catch (Exception ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 50002;
			}
			return responseModel;
		}

		public async Task<ResponseModel<TaskSyncResponseModel>> RequestTaskSync(TaskSyncRequestModel model)
		{
			var responseModel = new ResponseModel<TaskSyncResponseModel>();
			try
			{
				using HttpClient httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
				// httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");
				var dataJson = JsonConvert.SerializeObject(model);
				var content = new StringContent(dataJson, Encoding.UTF8, "application/json");

				model.taskId = "9245949";//this line to be removed
				HttpResponseMessage response =
					await httpClient.PutAsync($"wms-beheerder-api/contractor/Circet/tasks/{model.taskId}", content);
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
				responseModel.ErrorCode = 50003;
			}
			catch (Exception ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 50004;
			}
			return responseModel;
		}
	}
}
