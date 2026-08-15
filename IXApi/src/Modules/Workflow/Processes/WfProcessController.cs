using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    [EnableRateLimiting("tight")]
    [Route("api/v1/[controller]")]
    [ApiController]
[DomainPermission("Workflow", "Processes")]
    public class WfProcessController : BaseController<WfProcess, WfProcessDto>
    {
        public WfProcessController(IWfProcessService service, ILogger<WfProcessController> logger)
            : base(service, logger)
        {
        }
    }
}



