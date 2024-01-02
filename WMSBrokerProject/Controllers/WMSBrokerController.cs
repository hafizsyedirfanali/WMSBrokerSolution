using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>get full Active-Operator oriented task databased on InId from AO</remarks>
    /// <param name="orgId">registered code for sender of the message (AO or passive operator)</param>
    /// <param name="inId">ID in the source system of AO. used for doing updates</param>
    /// <param name="xRequestID"> unique X-Request-Id header which is used to ensure idempotent message processing in case of a retry</param>
    /// <param name="xCorrelationID">Correlates HTTP requests between a client and server. Should be same value in TaskIndication</param>
    /// <param name="xWMSTest">when value is true, this message is part of a test. Process the message as normal, same business logic. in the end, don&#x27;t actually physically execute the order. This order is not supposed to have a material impact. No customer impacts is allowed to happen because of this task. This flag can trigger extra logging at all involved systems during processing.  When other messages are sent because of a message with the this test flag set, those other messages *must* also include a test flag. </param>
    /// <param name="xWMSAPIVersion">request response in given API version</param>
    /// <response code="200">Success</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Missing required credentials</response>
    /// <response code="403">Forbidden because these credentials do not give the needed access rights</response>
    /// <response code="404">connection not found</response>
    /// <response code="429">too many requests too fast</response>
    /// <response code="500">Internal server error, dont try this request again</response>
    /// <response code="503">Temporary failure in service , try again later</response>
    [Route("contractor")]
    [ApiController]
    public class WMSBrokerController : ControllerBase
    {
        private string inId;
        private readonly IGoEfficientService goEfficientService;
        private readonly IWMSBeheerderService wMSBeheerderService;
		private readonly IConfiguration configuration;
		private readonly IOptions<Dictionary<string, ActionConfiguration>> actionOptions;

		public WMSBrokerController(IGoEfficientService goEfficientService, IOptions<Dictionary<string, ActionConfiguration>> actionOptions,
			IWMSBeheerderService wMSBeheerderService, IConfiguration configuration)
        {
            this.goEfficientService = goEfficientService;
            this.actionOptions = actionOptions;
            this.wMSBeheerderService = wMSBeheerderService;
			this.configuration = configuration;
		}


        [Route("{orgId}/tasks/{taskId}")]
        //[Route("orgId/tasks/taskId")]
        [HttpPost]
        //public async Task<IActionResult> BeginTaskIndicationProcess([FromBody] TaskIndicationRequestModel model)
        public async Task<IActionResult> BeginTaskIndicationProcess(
			[FromRoute][Required][StringLength(15, MinimumLength = 3)] string orgId,
            [FromRoute][Required][StringLength(36, MinimumLength = 1)] string taskId)
        {
            ///Request 1 
            //if (model is null || string.IsNullOrEmpty(model.inId))
            //{
            //    return BadRequest("The 'inId' is required.");
            //}

            
            Random rand = new Random();
            var requestId = rand.Next(10000, 1000001).ToString();

            var response2TaskFetch = await wMSBeheerderService.Request2TaskFetch(new REQ2Model { InID = taskId }).ConfigureAwait(false);
            if (!response2TaskFetch.IsSuccess) { }//{ return StatusCode(StatusCodes.Status500InternalServerError, response2TaskFetch); }
            JObject taskFetchJsonObject = response2TaskFetch.Result!.JSONObject;

            //TaskFetchResponse taskFetchResponse = response2TaskFetch.Result!.TaskFetchResponseObject!;
            var responseGetAction = await goEfficientService.GetPathValue(taskFetchJsonObject,configuration.GetSection("TaskAsyncRES2Attributes:action").Value!).ConfigureAwait(false);
            if (!responseGetAction.IsSuccess) { }
            var action = responseGetAction.Result!.ToString()!;
            var responseOriginatorId = await goEfficientService.GetPathValue(taskFetchJsonObject, configuration.GetSection("TaskAsyncRES2Attributes:originatorId").Value!).ConfigureAwait(false);
			if (!responseGetAction.IsSuccess) { }
			var originatorId = responseOriginatorId.Result!.ToString()!;

			actionOptions.Value.TryGetValue(action, out var actionConfiguration);

			var taskFetchResponse2 = await goEfficientService
					.FillSourcePathInBeheerderAttributesDictionary(action).ConfigureAwait(false);
			if (!taskFetchResponse2.IsSuccess) { }
			//above service gives key and path
			var taskFetchResponseData = await goEfficientService.FillDataInBeheerderAttributesDictionary(taskFetchJsonObject, taskFetchResponse2.Result!).ConfigureAwait(false);
			if (!taskFetchResponseData.IsSuccess) { }
			var dataDictionary = taskFetchResponseData.Result;

            var responseREQ6 = await goEfficientService.REQ6_IsRecordExist(new REQ6Model
            {
                //InId = inId,
                RequestId = requestId,
                InId = originatorId,
                Huurder_UDF_Id = actionConfiguration!.Huurder_UDF_Id!
            }).ConfigureAwait(false);
            if (!responseREQ6.IsSuccess) { }
            if (!responseREQ6.Result!.IsRecordExist)
            {

                #region RES4 RHS for PRO.PRO_ID

                var taskFetchForReq4 = await goEfficientService.FillDataForRequest4(dataDictionary!);
                if (!taskFetchForReq4.IsSuccess) { }
                //var street = taskFetchResponse.taskInfo.hasInfo.connectionAddress.streetName;
                //var cityName = taskFetchResponse.taskInfo.hasInfo.connectionAddress.city;
                //var houseNumber = taskFetchResponse.taskInfo.hasInfo.connectionAddress.houseNumber;
                //var zipCode = taskFetchResponse.taskInfo.hasInfo.connectionAddress.postalCode;
                ////var houseNumberSuffix = taskFetchResponse.taskInfo.hasInfo.connectionAddress.;
                //var houseNumberSuffix = "";
                var res4Result = await goEfficientService.REQ4_GetProIDAsync(new Models.REQ4Model
                {
                    RequestId = requestId,
                    InId = inId,
                    PRO_ID = actionConfiguration.PRO_ID,
                    Indicator2 = actionConfiguration.Indicator2,
                    CityName = taskFetchForReq4.Result!.CityName,
                    StreetName = taskFetchForReq4.Result!.StreetName,
                    HouseNumber = taskFetchForReq4.Result!.HouseNumber,
                    PostalCode = taskFetchForReq4.Result!.PostalCode,
                    HouseNumberExtension = taskFetchForReq4.Result!.HouseNumberExtension
                });
                if (res4Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "GetProId service returned null" });
                if (res4Result.Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "GetProId service returned null" });
                if (!res4Result.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res4Result);
                #endregion
                var proId = res4Result.Result.ProId3;
                if (proId is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "ProId is null" });
                var res4_1Result = await goEfficientService.REQ4_1_ReadExistingExecutionTask(new Models.REQ4_1Model
                {
                    RequestId = requestId,
                    Pro_Id_Desc = proId,
                    Pro_Template_Id = actionConfiguration.ExecutionTask_TemplateId!
                });
                if (res4_1Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "GetProId service returned null" });
                if (res4_1Result.Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "GetProId service returned null" });
                if (!res4_1Result.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res4_1Result);
                var proIdReq4_1 = res4_1Result.Result.Pro_Id;
                if (proIdReq4_1 is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "ProId is null in Rquest 4.1" });

                #region Names Logic
                var attributeValueDictResult = await goEfficientService
                    .GetAttributeValueDictionaryByAction(action, taskFetchJsonObject).ConfigureAwait(false);
                if (!attributeValueDictResult.IsSuccess) { }
                var attributeValueDict = attributeValueDictResult.Result;
                var namesArray = actionConfiguration.Naming!.Split('.');
                var processedNames = new List<string>();
                foreach (var item in namesArray)
                {
                    var match = Regex.Match(item, @"\[(.*?)\]");
                    if (match.Success)
                    {
                        string key = match.Groups[1].Value;
                        if (attributeValueDict != null && attributeValueDict.TryGetValue(key, out var value))
                        {
                            processedNames.Add(value?.ToString() ?? "");
                        }
                    }
                    else
                    {
                        processedNames.Add(item);
                    }
                }
                actionConfiguration.Naming = string.Join(".", processedNames);
                #endregion

                var res4_2Result = await goEfficientService.REQ4_2UpdateExeceutionTaskDes(new Models.REQ4_2Model
                {
                    RequestId = requestId,
                    Pro_Id = proIdReq4_1,
                    Naming = actionConfiguration.Naming

                }).ConfigureAwait(false);
                if (res4_2Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Update Execeution Task Des Service Returned Null" });
                if (!res4_2Result.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res4_2Result);



                var responseGoEfficientAttr = await goEfficientService.GetGoEfficientAttributes();
                var responseGoEfficientFileAttr = await goEfficientService.GetGoEfficientFileAttributes();
                #region REQ4a RHS for Template
                ///Here we have to pass the responseGoEfficientFileAttr
                var res4aResult = await goEfficientService.REQ4a_GetTemplateFromGoEfficient(new Models.REQ4aModel
                {
                    RequestId = requestId,
                    InId = inId,
                    ProId = proId,
                    Username = "",
                    Password = "",
                    GoEfficientAttributes = responseGoEfficientAttr.Result
                }).ConfigureAwait(false);
                if (res4aResult is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Get Template service returned null" });
                if (res4aResult.Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Get Template service returned null" });
                if (!res4aResult.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res4aResult);
                #endregion
                var fin_Id = res4aResult.Result.FIN_ID;

                var addresses = res4aResult.Result.Addresses;



                var responseFilledDataResult = await goEfficientService
                    .FillDataIn4aTemplate(res4aResult.Result.Template, new TaskFetchResponse2Model
                    {
                        WMSBeheerderAttributes = dataDictionary!,
                        ActionName = action
                    });
                if (!responseFilledDataResult.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new
                    {
                        responseFilledDataResult.ErrorMessage,
                        responseFilledDataResult.ErrorCode
                    });
                }
                var responseFilledFCDataResult = await goEfficientService
                    .FillFCDataIn4aTemplate(res4aResult.Result, new TaskFetchResponse2Model
                    {
                        WMSBeheerderAttributes = dataDictionary!
                    });
                if (!responseFilledFCDataResult.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new
                    {
                        responseFilledFCDataResult.ErrorMessage,
                        responseFilledFCDataResult.ErrorCode
                    });
                }
                var responseFilledAddressDataResult = await goEfficientService
                    .FillDataIn4aAddressTemplate(res4aResult.Result.Template, new TaskFetchResponse2Model
                    {
                        WMSBeheerderAttributes = dataDictionary
                    });

                Dictionary<string, object?> goEfficientTemplateValues = responseFilledDataResult.Result!
                    .GoEfficientTemplateValues;

                //var goEfficientAddressTemplateValues = responseFilledDataResult.Result.GoEfficientAddressTemplateValues;

                #region REQ5 RHS Save Record
                var res5Result = await goEfficientService.REQ5_SaveRecordToGoEfficient(new Models.REQ5Model
                {
                    RequestId = requestId,
                    Username = "",
                    Password = "",
                    InId = inId,
                    PRO_ID_3 = proId,
                    RES4aTemplate = responseFilledDataResult.Result,
                    GoEfficientTemplateValues = goEfficientTemplateValues
                }).ConfigureAwait(false);
                if (res5Result is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Save Record service returned null" });
                if (!res5Result.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res5Result);
                #endregion

                #region REQ5a RHS Save Addresses
                foreach (var address in addresses)
                {
                    var addressKeyName = await goEfficientService.GetWMSBeheerderRES4AddressMappingValue(address.FIN_Name);
                    if (addressKeyName is null || !addressKeyName.IsSuccess) { continue; }
                    var addressDictionary = await goEfficientService.GetKeyValuesFromWMSBeheerderAddresses(addressKeyName.Result!);
                    if (addressDictionary is null || !addressDictionary.IsSuccess) { continue; }

                    var addressMappingDataResult = await goEfficientService.GetAddressMappingDictionary(taskFetchJsonObject, addressDictionary.Result);
                    if (addressMappingDataResult is null ||
                        addressMappingDataResult.Result is null || !addressMappingDataResult.IsSuccess ||
                        !addressMappingDataResult.Result.Any()) { continue; }

                    var res5aResult = await goEfficientService.REQ5a_SaveAddressToGoEfficient(new Models.REQ5aModel
                    {
                        RequestId = requestId,
                        ExtractedAddressValues = addressMappingDataResult.Result,
                        InId = inId,
                        PRO_ID_3 = proId,
                        Address_FIN_ID = fin_Id,
                        Template = responseFilledAddressDataResult.Result
                        //Username = "",
                        //Password = "",
                        //City = taskFetchForReq4.Result!.CityName,
                        //HouseNo = taskFetchForReq4.Result!.HouseNumber,
                        //HouseNoSuffix = taskFetchForReq4.Result!.HouseNumberExtension,
                        //PostalCode = taskFetchForReq4.Result!.PostalCode,
                        //Street = taskFetchForReq4.Result!.StreetName,
                    }).ConfigureAwait(false);
                    if (res5aResult is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Save Address service returned null" });
                    if (!res5aResult.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res5aResult);
                }


                #endregion

                //foreach (var address in addresses)
                //{
                //    #region REQ5a RHS Save Addresses
                //    var res5aResult = await goEfficientService.REQ5a_SaveAddressToGoEfficient(new Models.REQ5aModel
                //    {
                //        InId = inId,
                //        PRO_ID_3 = proId,
                //        Username = "",
                //        Password = "",
                //        Address_FIN_ID = fin_Id,
                //        City = address.City,
                //        HouseNo = address.HouseNo,
                //        HouseNoSuffix = address.HouseNoSuffix,
                //        PostalCode = address.PostalCode,
                //        Street = address.Street,
                //        Template = responseFilledAddressDataResult.Result
                //    }).ConfigureAwait(false);
                //    if (res5aResult is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Save Address service returned null" });
                //    if (!res5aResult.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res5aResult);
                //    #endregion
                //}

            }

            if (dataDictionary is not null)
            {
				try
				{
					dataDictionary.TryGetValue("inId", out object? inId);
					dataDictionary.TryGetValue("orgId", out object? orgID);
					dataDictionary.TryGetValue("systemId", out object? systemId);
					dataDictionary.TryGetValue("updateCount", out object? updateCount);
					dataDictionary.TryGetValue("created", out object? created);
					dataDictionary.TryGetValue("reason", out object? reason);
					dataDictionary.TryGetValue("subStatus", out object? subStatus);
					dataDictionary.TryGetValue("clarification", out object? clarification);
					dataDictionary.TryGetValue("status", out object? status);
					DateTime createdDate = Convert.ToDateTime(created);
					var createdDateISO = createdDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
					var count = Convert.ToInt64(updateCount);
					//var clarifi = clarification != null ? clarification.ToString() : null;
					//var subStatusvalue = subStatus != null ? subStatus.ToString() : null;
					//var reasonvale = reason != null ? reason.ToString() : null;
					var taskSyncResponse = await wMSBeheerderService.RequestTaskSync(new TaskSyncRequestModel
					{
						//configuration.GetSection("TaskAsyncRES2Attributes:originatorId").Value!
						taskId = inId?.ToString() ?? "",
						header = new TaskSyncRequestModel.Header
						{
							from = new TaskSyncRequestModel.From
							{
								orgId =  "Circet",
								systemId = "NKM"
								//orgId = orgID?.ToString() ?? "",
								//systemId = systemId?.ToString() ?? ""
							},
							updateCount = (int)count,
							created = createdDateISO //Convert Date in ISO formate
						},
						status = new TaskSyncRequestModel.Status
						{
							mainStatus = status?.ToString() ?? ""
							//reason = reason?.ToString() ?? "",
							//subStatus = subStatus?.ToString() ?? "",
							//clarification = clarification?.ToString() ?? ""
						}
					}).ConfigureAwait(false);
					if (!taskSyncResponse.IsSuccess) { }
				}
				catch (Exception ex)
				{

					throw;
				}
				
			}

			//DateTime dt2 = DateTime.ParseExact(dt, "yyyy-MM-dd HH:mm tt", null);

			//DateTime UpdatedTime = created;
			//var dt = Convert.ToString(created);
			//DateTime d2 = DateTime.Parse(dt, null, DateTimeStyles.RoundtripKind);

			//created.ToUniversalTime().ToString("u").Replace(" ", "T");

			//dt.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz", CultureInfo.InvariantCulture);
			//var date = ((DateTime)created).ToString("o", CultureInfo.InvariantCulture);


			return Ok("Process completed successfully");
        }




    }
}
