using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Controls
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Controls")]
    public class WfControlController : BaseController<WfControl, WfControlDto>
    {
        public WfControlController(IWfControlService service, ILogger<WfControlController> logger) : base(service, logger)
        {
        }
    }
}




