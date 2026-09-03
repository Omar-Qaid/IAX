using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Workflow", "RequestControls")]
    public class WfRequestMappingVariableController : BaseController<WfRequestMappingVariable, WfRequestMappingVariableDto>
    {
        public WfRequestMappingVariableController(IWfRequestMappingVariableService service, ILogger<WfRequestMappingVariableController> logger) : base(service, logger)
        {
        }
    }
}
