using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Managers")]
    public class OrgManagementLevelController : BaseController<OrgManagementLevel, OrgManagementLevelDto>
    {
        public OrgManagementLevelController(IOrgManagementLevelService service, ILogger<OrgManagementLevelController> logger) : base(service, logger)
        {
        }
    }
}

