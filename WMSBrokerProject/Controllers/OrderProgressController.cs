using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderProgressController : AppBaseController
    {
        public OrderProgressController(IGoEfficientService goEfficientService, 
            IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, 
            IOrderProgressService orderProgressService, ICorrelationServices correlationServices) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
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
                var res7Result = await orderProgressService.REQ7GetTaskIDs(new REQ7Model
                {
                    RequestId = requestId,
                    TemplateId = template.TemplateID
                }).ConfigureAwait(false);
                if (!res7Result.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res7Result); }
                var taskIds = res7Result.Result!.TaskIdList;
                if (taskIds is not null && taskIds.Any())
                {
                    foreach (var taskId in taskIds)
                    {
                        var res4aResult = await orderProgressService.REQ4a_GetInID(
                            new OrderProcessingREQ4aModel
                            {
                                RequestId = requestId,
                                ProId = taskId.ProIdDESC,
                                Template = template,
                            }).ConfigureAwait(false);
                        if (!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }
                        
                        if(res4aResult.Result!.Res4ARowFields == null) 
                        {
                            skipTemplateId = true;
                            break;
                        } // Continue to next Template ID
                        
                        var res5Result = await orderProgressService.REQ05_UpdateInstantiatedAttachmentsRequest(new UIAREQ5Model
                        {
                            RequestId = requestId,
                            Status = template.GoEfficientStatus
                        }).ConfigureAwait(false);
                        if (!res5Result.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res5Result); }

                        var taskSyncResponse = await orderProgressService.RequestTaskIndication(new TaskIndicationRequestModel
                        {
                            header = new TaskIndicationRequestModel.Header
                            {
                                from = new TaskIndicationRequestModel.From
                                {
                                    orgId = "Circet",
                                    systemId = "NKM-GO" //Json
                                },
                                updateCount = 1, //Comming form 4a CIFWMS-UpdateCount Finmane
                                created = DateTime.Now,
                                priority = "BASIC"
                            },
                            taskId = "" //CIFWMS-OrderUid 4a
                        }).ConfigureAwait(false);
                        if (!taskSyncResponse.IsSuccess) { }

                       


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
        [Route("TaskFetchProcess")]
        public async Task<IActionResult> BeginTaskFetch(
            [FromRoute][Required][StringLength(15, MinimumLength = 3)] string orgId,
            [FromRoute][Required][StringLength(36, MinimumLength = 1)] string inId,
            [FromHeader][StringLength(36, MinimumLength = 1)] string xRequestID,
            [FromHeader][StringLength(36, MinimumLength = 1)] string xCorrelationID,
            [FromHeader] bool? xWMSTest,
            [FromHeader][StringLength(8, MinimumLength = 1)] string xWMSAPIVersion)
        {

            var res4aResult = await orderProgressService.REQ4a_GetTemplateData(new REQ4aGetTemplateModel
            {
                RequestId = xRequestID,
                ProId = inId
            }).ConfigureAwait(false);
            if (!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }

            var jsonResultForTaskFetchResponse = await orderProgressService.GetJsonResultForTaskFetchResponse(
                res4aResult.Result!, "PATCH").ConfigureAwait(false);
            if (!jsonResultForTaskFetchResponse.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, jsonResultForTaskFetchResponse); }

            //Json to be passed with OK Status
            return Ok(jsonResultForTaskFetchResponse.Result);
        }
    }
}
