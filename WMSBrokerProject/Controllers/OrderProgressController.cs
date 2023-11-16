using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderProgressController : ControllerBase
    {
        private readonly IGoEfficientService goEfficientService;
        private readonly IConfiguration configuration;
        private readonly IOptions<GoEfficientCredentials> goEfficientCredentials1;
        private readonly IOrderProgressService orderProgressService;
        private readonly OrderProgressSettingsModel orderProgressSettings;
        private readonly GoEfficientCredentials goEfficientCredentials;

        public OrderProgressController(IGoEfficientService goEfficientService, IConfiguration configuration,
            IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService)
        {
            this.goEfficientCredentials = goEfficientCredentials.Value;
            this.goEfficientService = goEfficientService;
            this.configuration = configuration;
            goEfficientCredentials1 = goEfficientCredentials;
            this.orderProgressService = orderProgressService;
            this.orderProgressSettings = orderProgressSettings;
        }
        [HttpGet("BeginOrderProgress")]
        public async Task<IActionResult> BeginOrderProgress()
        {
            var test = orderProgressSettings;
            Random rand = new Random();
            var requestId = rand.Next(10000, 1000001).ToString();
            var templateResponse = await orderProgressService.GetTemplateIds().ConfigureAwait(false);
            if (!templateResponse.IsSuccess) return StatusCode(StatusCodes.Status500InternalServerError, templateResponse);
            if (templateResponse.Result is null) return NotFound();
            var templates = templateResponse.Result?.Templates!;
            foreach (var template in templates)
            {
                var res7Result = await orderProgressService.REQ7GetTaskIDs(new REQ7Model
                {
                    RequestId = requestId,
                    TemplateId = template.TemplateId
                }).ConfigureAwait(false);
                if (!res7Result.IsSuccess) { }
                var taskIds = res7Result.Result!.TaskIdList;
                if (taskIds.Count != 0)
                {
                    foreach (var taskId in taskIds)
                    {
                        var res4aResult = await orderProgressService.REQ4a_GetTemplateFromGoEfficient(new TTREQ4aModel
                        {
                            RequestId = requestId,
                            ProId = taskId.ProIdDESC
                        }).ConfigureAwait(false);
                        if (!res4aResult.IsSuccess) { }

                        var res5Result = await orderProgressService.REQ05_UpdateInstantiatedAttachmentsRequest(new TTREQ5Model
                        {
                            RequestId = requestId,
                            Status = "" //wmsOderProgressSetting.json mian se WMSstatus se dalengey
                        }).ConfigureAwait(false);
                        if (!res4aResult.IsSuccess) { }

                        var taskSyncResponse = await orderProgressService.RequestTaskIndication(new TaskIndicationRequestModel
                        {
                            
                            header = new TaskIndicationRequestModel.Header
                            {
                                from = new TaskIndicationRequestModel.From
                                {
                                    orgId = "Circet",
                                    systemId = "NKM-GO"
                                },
                                updateCount = 1,
                                created = DateTime.Now,
                                priority = "BASIC"
                            },
                            inId = ""
                            //status = new TaskIndicationRequestModel.Status
                            //{
                            //    mainStatus = "NEW",
                            //    reason = taskFetchResponse.status.reason,
                            //    subStatus = taskFetchResponse.status.subStatus,
                            //    clarification = taskFetchResponse.status.clarification
                            //}
                        }).ConfigureAwait(false);
                        if (!taskSyncResponse.IsSuccess) { }
                    }
                }
            }

            return Ok("Process completed successfully");    
        }
    }
}
