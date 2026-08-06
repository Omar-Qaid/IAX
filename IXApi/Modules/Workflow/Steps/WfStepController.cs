using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Steps
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Steps")]
    public class WfStepController : BaseController<WfStep, WfStepDto>
    {
        public WfStepController(IWfStepService service, ILogger<WfStepController> logger) : base(service, logger)
        {
        }
    }
}




