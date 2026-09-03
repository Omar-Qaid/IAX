using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Workflow", "ActivityControls")]
    public class WfActivityMappingVariableController : BaseController<WfActivityMappingVariable, WfActivityMappingVariableDto>
    {
        public WfActivityMappingVariableController(IWfActivityMappingVariableService service, ILogger<WfActivityMappingVariableController> logger) : base(service, logger)
        {
        }
    }
}
