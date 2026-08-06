using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "ActivityControls")]
    public class WfActivityControlController : BaseController<WfActivityControl, WfActivityControlDto>
    {
        public WfActivityControlController(IWfActivityControlService service, ILogger<WfActivityControlController> logger) : base(service, logger)
        {
        }
    }
}
