using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("Organization", "Structure")]
public class OrganizationUnitController : BaseController<OrganizationUnit, OrganizationUnitDto>
{
    public OrganizationUnitController(
        IOrganizationUnitService service,
        ILogger<OrganizationUnitController>? logger = null)
        : base(service, logger ?? NullLogger<OrganizationUnitController>.Instance)
    {
    }
}
