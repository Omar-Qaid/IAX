using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "ActivityTypes")]
    public class WfActivityTypeController : BaseController<WfActivityType, WfActivityTypeDto>
    {
        public WfActivityTypeController(IWfActivityTypeService service, ILogger<WfActivityTypeController> logger) : base(service, logger)
        {
        }
    }
}


