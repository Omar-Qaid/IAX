using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "RequestControls")]
    public class WfRequestControlController : BaseController<WfRequestControl, WfRequestControlDto>
    {
        public WfRequestControlController(IWfRequestControlService service, ILogger<WfRequestControlController> logger) : base(service, logger)
        {
        }
    }
}




