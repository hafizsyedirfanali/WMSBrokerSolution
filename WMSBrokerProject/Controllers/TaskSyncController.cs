using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskSyncController : AppBaseController
    {
        public TaskSyncController(IGoEfficientService goEfficientService, IConfiguration configuration, 
            IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService, 
            ICorrelationServices correlationServices) : 
            base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
        }

        [HttpGet]
        public async Task<IActionResult> BeginTaskSyncProcess([FromBody] TaskSyncOPRequestModel model,
            [FromHeader][StringLength(36, MinimumLength = 1)] string xCorrelationID)
        //[FromRoute][Required][StringLength(36, MinimumLength = 1)] string inId,
        //[FromHeader][StringLength(36, MinimumLength = 1)] string xCorrelationID,

        {
            var correlationItem = correlationServices.GetCorrelationItemByCorrelationId(xCorrelationID);
            if (correlationItem is null) return NotFound($"CorrelationID = {xCorrelationID} not found");

            if (model.taskId == correlationItem.TaskId)
            {
                if (string.IsNullOrEmpty(model.status.reason))
                {
                    var res7aResult = await orderProgressService.REQ7a(new CTREQ7aModel
                    {

                    }).ConfigureAwait(false);
                    if (!res7aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res7aResult.ErrorMessage); }
                    return Ok("Task Sync Process Completed Successfully");
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }
    }
}
