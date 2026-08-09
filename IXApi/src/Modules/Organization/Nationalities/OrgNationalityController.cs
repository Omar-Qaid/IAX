using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.Nationalities
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Nationalities")]
    public class OrgNationalityController : BaseController<OrgNationality, OrgNationalityDto>
    {
        public OrgNationalityController(IOrgNationalityService service, ILogger<OrgNationalityController> logger) : base(service, logger)
        {
        }
    }
}
