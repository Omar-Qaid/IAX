using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Activities")]
    public class WfActivityController : BaseController<WfActivity, WfActivityDto>
    {
        public WfActivityController(IWfActivityService service, ILogger<WfActivityController> logger) : base(service, logger)
        {
        }
    }
}





