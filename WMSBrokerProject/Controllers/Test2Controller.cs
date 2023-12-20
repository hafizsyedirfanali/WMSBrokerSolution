using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Test2Controller : AppBaseController
    {
        private readonly IWMSBeheerderService wMSBeheerderService;
        private readonly IOptions<Dictionary<string, ActionConfiguration>> actionOptions;

        public Test2Controller(IGoEfficientService goEfficientService, IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService, ICorrelationServices correlationServices, IWMSBeheerderService wMSBeheerderService, IOptions<Dictionary<string, ActionConfiguration>> actionOptions) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
            this.wMSBeheerderService = wMSBeheerderService;
            this.actionOptions = actionOptions;
        }

        [HttpGet]
        public async Task<IActionResult> BeginTestProcess()
        {
            var response2TaskFetch = await wMSBeheerderService.Request2TaskFetch(new REQ2Model { InID = "WMS002530553" }).ConfigureAwait(false);
            if (!response2TaskFetch.IsSuccess) { }//{ return StatusCode(StatusCodes.Status500InternalServerError, response2TaskFetch); }
            JObject taskFetchJsonObject = response2TaskFetch.Result!.JSONObject;
            TaskFetchResponse taskFetchResponse = response2TaskFetch.Result!.TaskFetchResponseObject!;

           
            actionOptions.Value.TryGetValue(taskFetchResponse.action, out var actionConfiguration);
            var responseREQ6 = await goEfficientService.REQ6_IsRecordExist(new REQ6Model
            {
                //InId = inId,
                InId = taskFetchResponse.originatorId,
                Huurder_UDF_Id = actionConfiguration!.Huurder_UDF_Id!
            }).ConfigureAwait(false);
            if (!responseREQ6.IsSuccess) { }

            var responseGoEfficientAttr = await goEfficientService.GetGoEfficientAttributes();
            var responseGoEfficientFileAttr = await goEfficientService.GetGoEfficientFileAttributes();

            var taskFetchResponse2 = await goEfficientService
                    .FillSourcePathInBeheerderAttributesDictionary(taskFetchResponse).ConfigureAwait(false);
            if (!taskFetchResponse2.IsSuccess) { }
            //above service gives key and path
            var taskFetchResponseData = await goEfficientService.FillDataInBeheerderAttributesDictionary(taskFetchResponse, taskFetchResponse2.Result!).ConfigureAwait(false);
            if (!taskFetchResponseData.IsSuccess) { }

            var dataDictionary = taskFetchResponseData.Result;

            #region REQ4a RHS for Template
            ///Here we have to pass the responseGoEfficientFileAttr
            var res4aResult = await goEfficientService.REQ4a_GetTemplateFromGoEfficient(new Models.REQ4aModel
            {
                InId = "145",
                ProId = "123",
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
                        ActionName = taskFetchResponse.action
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

            #region REQ5a RHS Save Addresses
            foreach (var address in addresses)
            {
                var addressKeyName = await goEfficientService.GetWMSBeheerderRES4AddressMappingValue(address.FIN_Name);
                if (addressKeyName is null || !addressKeyName.IsSuccess) { continue; }
                var addressDictionary = await goEfficientService.GetKeyValuesFromWMSBeheerderAddresses(addressKeyName.Result!);
                if (addressDictionary is null || !addressDictionary.IsSuccess) { continue; }
                var extractedAddressValues = new Dictionary<string, string>();
                foreach (var kvp in addressDictionary.Result!)
                {
                    var path = kvp.Value;
                    var token = taskFetchJsonObject.SelectToken(path);
                    if (token != null)
                    {
                        extractedAddressValues[kvp.Key] = token.ToString();
                    }
                    else
                    {
                        // Handle the case where the path does not exist in the JSON
                    }
                }
                var res5aResult = await goEfficientService.REQ5a_SaveAddressToGoEfficient(new Models.REQ5aModel
                {
                    ExtractedAddressValues = extractedAddressValues,
                    InId = "145",
                    PRO_ID_3 = "123",
                    Username = "",
                    Password = "",
                    Address_FIN_ID = fin_Id,
                    City = "MU",
                    HouseNo = "11",
                    HouseNoSuffix = "A",
                    PostalCode = "4400",
                    Street = "last",
                    Template = responseFilledAddressDataResult.Result
                }).ConfigureAwait(false);
                if (res5aResult is null) return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = "Save Address service returned null" });
                if (!res5aResult.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, res5aResult);
            }


            #endregion

            return Ok("Process completed successfully");
        }
    }
}
