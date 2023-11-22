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
    public class TaskFetchController : AppBaseController
    {
        public TaskFetchController(IGoEfficientService goEfficientService, IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService, ICorrelationServices correlationServices) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
        }

        [HttpGet]
        public async Task<IActionResult> BeginProcess(
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
            if(!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }

            return Ok(res4aResult.Result);
            //return Ok(new
            //{
            //    res4aResult.Result
            //});
        }
    }
}
