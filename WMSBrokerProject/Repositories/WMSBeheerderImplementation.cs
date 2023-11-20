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
			this.token = configuration.GetSection("token").Value!;
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
				string responseContent = File.ReadAllText("response2.json");
				TaskFetchResponseModel taskFetchResponse = JsonConvert.DeserializeObject<TaskFetchResponseModel>(responseContent)!;
				responseModel.Result = taskFetchResponse;
				responseModel.IsSuccess = true;
				//+++++++++++++++UnComment Following lines in live environment and comment above lines
				//model.InID = "9245949";//this line to be removed
				//using HttpClient httpClient = new HttpClient();
				//string? endPointUrl = "https://uat-gke.cif-operator.com/";
				//string? requestUrl = Path.Combine(endPointUrl!, $"wms-beheerder-api/contractor/{orgId}/tasks/{model.InID}");

				////httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
				//httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

				//// httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");

				//HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
				//if (response.IsSuccessStatusCode)
				//{
				//	string responseContent = await response.Content.ReadAsStringAsync();
				//	TaskFetchResponseModel taskFetchResponse = JsonConvert.DeserializeObject<TaskFetchResponseModel>(responseContent)!;
				//	responseModel.Result = taskFetchResponse;
				//	responseModel.IsSuccess = true;
				//}
				//else
				//{
				//	responseModel.ErrorCode = (int)response.StatusCode;
				//	responseModel.ErrorMessage = $"Task fetch call failure with status/code:{response.StatusCode}";
				//}
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
