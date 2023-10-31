using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WMSBrokerController : ControllerBase
    {
        private string inId;
        private readonly IGoEfficientService goEfficientService;
        public WMSBrokerController(IGoEfficientService goEfficientService)
        {
            this.goEfficientService = goEfficientService;
        }
        [Route("TaskIndication")]
        [HttpPost]
        public async Task<IActionResult> BeginProcess([FromBody] TaskIndicationRequestModel model)
        {
            ///Request 1 
            if(model is null || string.IsNullOrEmpty(model.inId))
            {
                return BadRequest("The 'inId' is required.");
            }

            inId = model.inId;

            ///Request 2
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://uat-gke.cif-operator.com/");
                // httpClient.DefaultRequestHeaders.Add("headerName", "headerValue");
                HttpResponseMessage response = await httpClient.GetAsync("wms-beheerder-api/contractor/Circet/tasks/9245949");

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    TaskFetchResponseModel responseData = JsonConvert.DeserializeObject<TaskFetchResponseModel>(responseContent)!;

                    //string inIdValue = responseData.inId;
                    var responseREQ6 = await goEfficientService.REQ6_IsRecordExist(new REQ6Model
                    {
                        InId = inId
                    }).ConfigureAwait(false);

                }
            }

            ///Request 3 onwardas




            return Ok();
        }
        
    }
}
