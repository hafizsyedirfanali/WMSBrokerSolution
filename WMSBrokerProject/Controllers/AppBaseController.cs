using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Controllers
{
    public class AppBaseController : ControllerBase
    {
        protected readonly IGoEfficientService goEfficientService;
        protected readonly IConfiguration configuration;
        protected readonly IOptions<GoEfficientCredentials> goEfficientCredentials1;
        protected readonly IOrderProgressService orderProgressService;
        protected readonly GoEfficientCredentials goEfficientCredentials;
        public AppBaseController(IGoEfficientService goEfficientService, IConfiguration configuration,
            IOptions<GoEfficientCredentials> goEfficientCredentials, IOrderProgressService orderProgressService)
        {
            this.goEfficientCredentials = goEfficientCredentials.Value;
            this.goEfficientService = goEfficientService;
            this.configuration = configuration;
            goEfficientCredentials1 = goEfficientCredentials;
            this.orderProgressService = orderProgressService;
        }
    }
}
