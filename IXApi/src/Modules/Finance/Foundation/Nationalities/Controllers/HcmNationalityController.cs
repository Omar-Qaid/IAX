using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.Nationalities
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Nationalities")]
    public class HcmNationalityController : BaseController<HcmNationality, HcmNationalityDto>
    {
        public HcmNationalityController(IHcmNationalityService service, ILogger<HcmNationalityController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<HcmNationalityController>.Instance)
        {
        }
    }

    [ApiController]
    [Route("api/v1/Nationality")]
    [DomainPermission("Organization", "Nationalities")]
    public class LegacyNationalityController : BaseController<HcmNationality, HcmNationalityDto>
    {
        public LegacyNationalityController(IHcmNationalityService service, ILogger<LegacyNationalityController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LegacyNationalityController>.Instance)
        {
        }
    }
}
