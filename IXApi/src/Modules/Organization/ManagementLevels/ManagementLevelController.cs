using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Managers")]
    public class ManagementLevelController : BaseController<ManagementLevel, ManagementLevelDto>
    {
        public ManagementLevelController(IManagementLevelService service, ILogger<ManagementLevelController> logger) : base(service, logger)
        {
        }
    }
}

