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
        public HcmNationalityController(IHcmNationalityService service, ILogger<HcmNationalityController> logger) : base(service, logger)
        {
        }
    }

    [ApiController]
    [Route("api/v1/Nationality")]
    [DomainPermission("Organization", "Nationalities")]
    public class LegacyNationalityController : BaseController<HcmNationality, HcmNationalityDto>
    {
        public LegacyNationalityController(IHcmNationalityService service, ILogger<LegacyNationalityController> logger) : base(service, logger)
        {
        }
    }
}
