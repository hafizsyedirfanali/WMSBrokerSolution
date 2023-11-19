using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskFetchController : AppBaseController
    {
        public TaskFetchController(IGoEfficientService goEfficientService, IConfiguration configuration, IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService) : base(goEfficientService, configuration, goEfficientCredentials, orderProgressService) { }

        [HttpGet]
        public async Task<IActionResult> BeginProcess()
        {
            
            var res4aResult = await goEfficientService.REQ4a_GetTemplateFromGoEfficient(new REQ4aModel
            {
                
            }).ConfigureAwait(false);
            if(!res4aResult.IsSuccess) { return StatusCode(StatusCodes.Status500InternalServerError, res4aResult); }


            return Ok("Task Fetch Process Completed Successfully");
        }
    }
}
