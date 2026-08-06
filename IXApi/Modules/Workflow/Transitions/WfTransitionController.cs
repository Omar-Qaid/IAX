using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Transitions
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Transitions")]
    public class WfTransitionController : BaseController<WfTransition, WfTransitionDto>
    {
        public WfTransitionController(IWfTransitionService service, ILogger<WfTransitionController> logger) : base(service, logger)
        {
        }
    }
}




