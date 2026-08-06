using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Identity.Roles
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("SystemAdministration", "Roles")]
    public class RoleController : BaseController<AspNetRole, AspNetRoleDto>
    {
        public RoleController(IRoleService service, ILogger<RoleController> logger) : base(service, logger)
        {
        }
    }
}

