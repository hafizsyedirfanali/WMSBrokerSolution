using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskSyncController : AppBaseController
    {
        public TaskSyncController(IGoEfficientService goEfficientService, IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService, ICorrelationServices correlationServices) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
        }

        [HttpGet]
        public async Task<IActionResult> BeginProcess(
            [FromRoute][Required][StringLength(36, MinimumLength = 1)] string inId,
            [FromHeader][StringLength(36, MinimumLength = 1)] string xCorrelationID
            )
        {
            var res7aResult = await orderProgressService.REQ7a(new CTREQ7aModel
            {

            }).ConfigureAwait(false);
            if (!res7aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res7aResult.ErrorMessage); }

            return Ok("Task Sync Process Completed Successfully");
        }
    }
}
