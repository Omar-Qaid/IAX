using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.Nationalities
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Nationalities")]
    public class NationalityController : BaseController<Nationality, NationalityDto>
    {
        public NationalityController(INationalityService service, ILogger<NationalityController> logger) : base(service, logger)
        {
        }
    }
}
