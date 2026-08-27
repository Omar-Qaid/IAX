using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.Occupations
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Occupations")]
    public class OccupationController : BaseController<Occupation, OccupationDto>
    {
        public OccupationController(IOccupationService service, ILogger<OccupationController> logger) : base(service, logger)
        {
        }
    }
}
