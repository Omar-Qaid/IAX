using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Priorities
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Priorities")]
    public class WfPriorityController : BaseController<WfPriority, WfPriorityDto>
    {
        public WfPriorityController(IWfPriorityService service, ILogger<WfPriorityController> logger) : base(service, logger)
        {
        }
    }
}




