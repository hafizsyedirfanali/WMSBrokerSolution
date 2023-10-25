using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WMSBrokerProject.Models
{
    [Route("api/[controller]")]
    [ApiController]
    public class WMSBrokerController : ControllerBase
    {
        private string inId;
        [HttpPost]
        public async Task<IActionResult> TaskIndication(TaskIndicationRequestModel model)
        {
            inId = model.inId;
            return Ok();
        }
    }
}
