﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.IO.Pipelines;
using System.Reflection.Emit;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WMSBrokerController : ControllerBase
    {
        private string inId;
		private readonly IGoEfficientService goEfficientService;
		private readonly IWMSBeheerderService WMSBeheerderService;
        private readonly IOptions<Dictionary<string, ActionConfiguration>> _actionOptions;

        public WMSBrokerController(IGoEfficientService goEfficientService, IOptions<Dictionary<string, ActionConfiguration>> actionOptions)
        {
            this.goEfficientService = goEfficientService;
            _actionOptions = actionOptions;
        }
        [Route("TaskIndication")]
        [HttpPost]
        public async Task<IActionResult> BeginProcess([FromBody] TaskIndicationRequestModel model)
        {
            ///Request 1 
            if(model is null || string.IsNullOrEmpty(model.inId))
            {
                return BadRequest("The 'inId' is required.");
            }

            inId = model.inId;

			var response2TaskFetch = await WMSBeheerderService.Request2TaskFetch(new Models.REQ2Model { InID = inId }).ConfigureAwait(false);
			if (!response2TaskFetch.IsSuccess) { }
			TaskFetchResponseModel taskFetchResponse = response2TaskFetch.Result!;
            _actionOptions.Value.TryGetValue(taskFetchResponse.action, out var actionConfiguration);
            var responseREQ6 = await goEfficientService.REQ6_IsRecordExist(new REQ6Model
            {
                InId = inId,
                HuurderId = actionConfiguration.HuurderId
            }).ConfigureAwait(false);
            if (!responseREQ6.IsSuccess) { }
            if (!responseREQ6.Result.IsRecordExist)
            {
                var taskFetchResponse2 = await goEfficientService
                    .FillSourcePathInBeheerderAttributesDictionary(taskFetchResponse).ConfigureAwait(false);
                if (!taskFetchResponse2.IsSuccess) { }
                //above service gives key and path
                var taskFetchResponseData = await goEfficientService.FillDataInBeheerderAttributesDictionary(taskFetchResponse, taskFetchResponse2.Result).ConfigureAwait(false);
                if (!taskFetchResponseData.IsSuccess) { }


                #region RES4 RHS for PRO.PRO_ID
                var dataDictionary = taskFetchResponseData.Result;

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
                    InId = inId,
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
                var responseGoEfficientAttr = await goEfficientService.GetGoEfficientAttributes();
                var responseGoEfficientFileAttr = await goEfficientService.GetGoEfficientFileAttributes();
                #region REQ4a RHS for Template
                ///Here we have to pass the responseGoEfficientFileAttr
                var res4aResult = await goEfficientService.REQ4a_GetTemplateFromGoEfficient(new Models.REQ4aModel
                {
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
                        WMSBeheerderAttributes = taskFetchResponse2.Result
                    });

                var responseFilledAddressDataResult = await goEfficientService
                    .FillDataIn4aAddressTemplate(res4aResult.Result.Template, new TaskFetchResponse2Model
                    {
                        WMSBeheerderAttributes = taskFetchResponse2.Result
                    });

                var goEfficientTemplateValues = responseFilledDataResult.Result.GoEfficientTemplateValues;

                #region REQ5 RHS Save Record
                var res5Result = await goEfficientService.REQ5_SaveRecordToGoEfficient(new Models.REQ5Model
                {
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


                foreach (var address in addresses)
                {
                    #region REQ5a RHS Save Addresses
                    var res5aResult = await goEfficientService.REQ5a_SaveAddressToGoEfficient(new Models.REQ5aModel
                    {
                        InId = inId,
                        PRO_ID_3 = proId,
                        Username = "",
                        Password = "",
                        Address_FIN_ID = fin_Id,
                        City = address.City,
                        HouseNo = address.HouseNo,
                        HouseNoSuffix = address.HouseNoSuffix,
                        PostalCode = address.PostalCode,
                        Street = address.Street,
                        Template = responseFilledAddressDataResult.Result
                    }).ConfigureAwait(false);
                    if (res5aResult is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Save Address service returned null" });
                    if (!res5aResult.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res5aResult);
                    #endregion
                }

            }

            return Ok("Process completed successfully");
        }
        
    }
}
