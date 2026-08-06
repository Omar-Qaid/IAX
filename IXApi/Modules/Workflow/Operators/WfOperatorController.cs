using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Operators
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Operators")]
    public class WfOperatorController : BaseController<WfOperator, WfOperatorDto>
    {
        public WfOperatorController(IWfOperatorService service, ILogger<WfOperatorController> logger) : base(service, logger)
        {
        }
    }
}




