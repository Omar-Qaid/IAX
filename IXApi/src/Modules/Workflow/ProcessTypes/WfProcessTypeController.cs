using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.ProcessTypes
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Workflow", "ProcessTypes")]
    public class WfProcessTypeController : BaseController<WfProcessType, WfProcessTypeDto>
    {
        public WfProcessTypeController(IWfProcessTypeService service, ILogger<WfProcessTypeController> logger)
            : base(service, logger)
        {
        }
    }
}
