using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WMSBrokerController : ControllerBase
    {
        private string inId;
        [HttpPost]
        public async Task<IActionResult> TaskIndication([FromBody] TaskIndicationRequestModel model)
        {
            if(model is null || string.IsNullOrEmpty(model.inId))
            {
                return BadRequest("The 'inId' is required.");
            }

            inId = model.inId;
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> TaskFetch()
        {

        }
    }
}
