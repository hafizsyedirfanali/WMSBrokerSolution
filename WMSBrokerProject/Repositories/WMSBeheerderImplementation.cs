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

namespace WMSBrokerProject.Repositories
{
	public class WMSBeheerderImplementation : IWMSBeheerderService
	{
		private readonly string? templateFolder;
		private readonly string symbolForConcatenation;
		private readonly string symbolForPriority;
		private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment hostEnvironment;
		private readonly GoEfficientCredentials goEfficientCredentials;
		public WMSBeheerderImplementation(IConfiguration configuration, IWebHostEnvironment hostEnvironment,
			IOptions<GoEfficientCredentials> goEfficientCredentials)
		{
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
				using HttpClient httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
				// httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");
				model.InID = "9245949";//this line to be removed
                HttpResponseMessage response = 
					await httpClient.GetAsync($"wms-beheerder-api/contractor/Circet/tasks/{model.InID}");
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

		public async Task<ResponseModel<string>> RequestTaskSync(string InID)
		{
			var responseModel = new ResponseModel<string>();
			try
			{
				using HttpClient httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
				// httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");
				HttpResponseMessage response = await httpClient.GetAsync("wms-beheerder-api/contractor/Circet/tasks/9245949");
				if (response.IsSuccessStatusCode)
				{
					string responseContent = await response.Content.ReadAsStringAsync();
					responseModel.Result = responseContent;

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
