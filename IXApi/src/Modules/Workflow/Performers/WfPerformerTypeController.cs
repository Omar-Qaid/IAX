using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Workflow", "PerformerTypes")]
    public class WfPerformerTypeController : BaseController<WfPerformerType, WfPerformerTypeDto>
    {
        public WfPerformerTypeController(IWfPerformerTypeService service, ILogger<WfPerformerTypeController> logger)
            : base(service, logger)
        {
        }
    }
}
