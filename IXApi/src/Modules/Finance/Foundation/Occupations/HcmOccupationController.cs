using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Occupations
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Occupations")]
    public class HcmOccupationController : BaseController<HcmOccupation, HcmOccupationDto>
    {
        public HcmOccupationController(IHcmOccupationService service, ILogger<HcmOccupationController> logger) : base(service, logger)
        {
        }
    }

    [ApiController]
    [Route("api/v1/Occupation")]
    [DomainPermission("Organization", "Occupations")]
    public class LegacyOccupationController : BaseController<HcmOccupation, HcmOccupationDto>
    {
        public LegacyOccupationController(IHcmOccupationService service, ILogger<LegacyOccupationController> logger) : base(service, logger)
        {
        }
    }
}
