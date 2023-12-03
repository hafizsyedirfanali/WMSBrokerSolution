using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : AppBaseController
    {
        public TestController(IGoEfficientService goEfficientService, IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService, ICorrelationServices correlationServices) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService, correlationServices)
        {
        }

        [HttpGet]
        public async Task<IActionResult> BeginTestProcess()
        {
            Random rand = new Random();
            var requestId = rand.Next(10000, 1000001).ToString();
            var inId = "1452";
            var res4aResult = await orderProgressService.REQ4a_GetTemplateData(new REQ4aGetTemplateModel
            {
                RequestId = requestId,
                ProId = inId
            }).ConfigureAwait(false);
            if (!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }
            var templates = res4aResult.Result!.Templates;
            var opAttributeDataResult = await orderProgressService.GetOPAttributeData(new ReqOPDataDictionaryModel { Templates = templates }).ConfigureAwait(false);
            if (!opAttributeDataResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, opAttributeDataResult); }


            return Ok();
        }
    }
}
