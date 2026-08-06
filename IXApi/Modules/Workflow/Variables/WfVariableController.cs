using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Variables")]
    public class WfVariableController : BaseController<WfVariable, WfVariableDto>
    {
        public WfVariableController(IWfVariableService service, ILogger<WfVariableController> logger) : base(service, logger)
        {
        }
    }
}




