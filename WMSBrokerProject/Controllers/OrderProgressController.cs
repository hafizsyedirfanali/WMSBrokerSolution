using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;
using WMSBrokerProject.Repositories;

namespace WMSBrokerProject.Controllers
{
    [Route("contractor")]
    [ApiController]
    public class OrderProgressController : AppBaseController
    {
        private readonly string orgId;
        private readonly string systemId;

        public OrderProgressController(IGoEfficientService goEfficientService, 
            IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, 
            IOrderProgressService orderProgressService, ICorrelationServices correlationServices) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
            this.orgId = configuration.GetSection("orgId").Value!;
            this.systemId = configuration.GetSection("systemId").Value!;
        }

        [HttpGet]
        [Route("OrderProgress")]
        public async Task<IActionResult> BeginOrderProgress()
        {
            Random rand = new Random();
            var requestId = rand.Next(10000, 1000001).ToString();
            var templateResponse = await orderProgressService.GetTemplateIds().ConfigureAwait(false);
            if (!templateResponse.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, templateResponse);
            if (templateResponse.Result is null) return NotFound();
            var templates = templateResponse.Result?.Templates!;
            bool skipTemplateId = false;

            foreach (var template in templates)
            {
                var res7Result = await orderProgressService.REQ7GetPro_IDs(new REQ7Model
                {
                    RequestId = requestId,
                    TemplateId = template.TemplateID
                }).ConfigureAwait(false);
                if (!res7Result.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res7Result); }
                var pro_Ids = res7Result.Result!.Pro_IdList;
                if (pro_Ids is not null && pro_Ids.Any())
                {
                    foreach (var pro_Id in pro_Ids)
                    {
                        var res4aResult = await orderProgressService.REQ4a_GetInID(
                            new OrderProcessingREQ4aModel
                            {
                                RequestId = requestId,
                                ProId = pro_Id.ProIdDESC,
                                Template = template,
                            }).ConfigureAwait(false);
                        if (!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }
                        var dataDictionary = res4aResult.Result;

                        if (res4aResult.Result!.Res4ARowFields == null) 
                        {
                            skipTemplateId = true;
                            break;
                        } // Continue to next Template ID
                        
                        var res5Result = await orderProgressService.REQ05_UpdateInstantiatedAttachmentsRequest(new UIAREQ5Model
                        {
                            RequestId = requestId,
                            ProId = pro_Id.ProIdDESC,
                            Status = template.GoEfficientStatus
                        }).ConfigureAwait(false);
                        if (!res5Result.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res5Result); }

                        if (dataDictionary is not null)
                        {
                            dataDictionary.SelectListItems.TryGetValue("taskId", out object? taskId);
                            dataDictionary.SelectListItems.TryGetValue("updateCount", out object? updateCount);
                            dataDictionary.SelectListItems.TryGetValue("priority", out object? priority);
                            //var count = Convert.ToString(updateCount);
                            //var valueArray = count.Split(',');
                            //var item = valueArray[0];
                            correlationServices.SaveCorrelationItem(new Repositories.CorrelationItem
                            {
                                TaskId = taskId?.ToString() ?? "",
                                Pro_Id = pro_Id.ProIdDESC,
                                //Action = template.ActionType
                                Action = res4aResult.Result.ActionName
                            });
                            
                            var taskSyncResponse = await orderProgressService.RequestTaskIndication(new TaskIndicationRequestModel
                            {
                                header = new TaskIndicationRequestModel.Header
                                {
                                    from = new TaskIndicationRequestModel.From
                                    {
                                        orgId = orgId,
                                        systemId = systemId //Json
                                    },
                                    updateCount = 2, //Comming form 4a CIFWMS-UpdateCount Finmane
                                    created = DateTime.Now,
                                    priority = priority?.ToString() ?? ""
                                },
                                taskId = taskId?.ToString() ?? ""
                            }).ConfigureAwait(false);
                            if (!taskSyncResponse.IsSuccess) { }
                        }
                    }
                    if (skipTemplateId)
                    {
                        skipTemplateId = false;
                        continue;
                    }
                }
            }

            return Ok("Process completed successfully");    
        }


        [HttpGet]
        [Route("{orgId}/tasks/{taskId}")]
        public async Task<IActionResult> BeginTaskFetch(
        [FromHeader][StringLength(36, MinimumLength = 1)] string? xCorrelationID,
        [FromRoute][Required][StringLength(15, MinimumLength = 3)] string orgId,
        [FromRoute][Required][StringLength(36, MinimumLength = 1)] string taskId)
        {
            Random rand = new Random();
            var requestId = rand.Next(10000, 1000001).ToString();
            //var correlationItem = correlationServices.GetCorrelationItemByTaskId(taskId);
            //if (correlationItem is null) return NotFound($"TaskId = {taskId} not found");
            //if (correlationItem.Pro_Id is null) return NotFound("Pro_Id not found");
            //if (correlationItem.Action is null) return NotFound("Action not found");
            var res4aResult = await orderProgressService.REQ4a_GetTemplateData(new REQ4aGetTemplateModel
            {
                RequestId = requestId,
                //ProId = correlationItem.Pro_Id
                ProId = "9440957"
            }).ConfigureAwait(false);
            if (!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }

            var addresses = res4aResult.Result!.Addresses;
            var listItem = new List<AddressFieldsList>();

            foreach (var address in addresses)
            {
                var addressResponse = await orderProgressService.REQ08_ReadAddress(new REQ8Model
                {
                    RequestId = requestId,
                    AddressId = address.FIN_ADRESS_ID
                }).ConfigureAwait(false);
                if (addressResponse.IsSuccess)
                {
                    var addressField = addressResponse.Result!.AddressFields.FirstOrDefault();
                    if (addressField is not null)
                    {
                        listItem.Add(new AddressFieldsList
                        {
                            Fin_Name = address.FIN_NAME,
                            CityName = addressField.CityName,
                            StreetName = addressField.StreetName,
                            Country = addressField.Country,
                            HouseNumber = addressField.HouseNumber,
                            PostalCode = addressField.PostalCode,
                            HouseNumberExtension = addressField.HouseNumberExtension
                        });
                    }
                }
            }
            var addressMappedPaths = new List<TaskFetchResponseAddressMappedModel>();
            foreach (var item in listItem)
            {
                var responseAddressPaths = await orderProgressService.GetWMSBeheerderAddressPaths(item.Fin_Name).ConfigureAwait(false);
                if (!responseAddressPaths.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, responseAddressPaths); }
                Dictionary<string, object?> addressDictionary = responseAddressPaths.Result!;

                addressDictionary.TryGetValue("postalCode", out object? postalCodePath);
                if (postalCodePath != null) addressMappedPaths.Add(new TaskFetchResponseAddressMappedModel
                { WMSBeheerderAddressPath = postalCodePath.ToString(), Value = item.PostalCode });

                addressDictionary.TryGetValue("houseNumber", out object? houseNumberPath);
                if (houseNumberPath != null) addressMappedPaths.Add(new TaskFetchResponseAddressMappedModel
                { WMSBeheerderAddressPath = houseNumberPath.ToString(), Value = item.HouseNumber });

                addressDictionary.TryGetValue("houseNumberExtension", out object? houseNumberExtensionPath);
                if (houseNumberExtensionPath != null) addressMappedPaths.Add(new TaskFetchResponseAddressMappedModel
                { WMSBeheerderAddressPath = houseNumberExtensionPath.ToString(), Value = item.HouseNumberExtension });

                addressDictionary.TryGetValue("city", out object? cityPath);
                if (cityPath != null) addressMappedPaths.Add(new TaskFetchResponseAddressMappedModel
                { WMSBeheerderAddressPath = cityPath.ToString(), Value = item.CityName });

                addressDictionary.TryGetValue("streetName", out object? streetNamePath);
                if (streetNamePath != null) addressMappedPaths.Add(new TaskFetchResponseAddressMappedModel
                { WMSBeheerderAddressPath = streetNamePath.ToString(), Value = item.StreetName });

                addressDictionary.TryGetValue("country", out object? countryPath);
                if (countryPath != null) addressMappedPaths.Add(new TaskFetchResponseAddressMappedModel
                { WMSBeheerderAddressPath = countryPath.ToString(), Value = item.Country });
            }

            var jsonResultForTaskFetchResponse = await orderProgressService
                .GetJsonResultForTaskFetchResponse(res4aResult.Result!, "CONNECTION_INCIDENT", addressMappedPaths).ConfigureAwait(false);
            if (!jsonResultForTaskFetchResponse.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, jsonResultForTaskFetchResponse); }
			var jsonString = JsonConvert.SerializeObject(jsonResultForTaskFetchResponse.Result);
			//Json to be passed with OK Status
			return Ok(jsonString);
        }
    }
}
