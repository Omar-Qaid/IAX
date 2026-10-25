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
        public HcmOccupationController(IHcmOccupationService service, ILogger<HcmOccupationController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<HcmOccupationController>.Instance)
        {
        }
    }

    [ApiController]
    [Route("api/v1/Occupation")]
    [DomainPermission("Organization", "Occupations")]
    public class LegacyOccupationController : BaseController<HcmOccupation, HcmOccupationDto>
    {
        public LegacyOccupationController(IHcmOccupationService service, ILogger<LegacyOccupationController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LegacyOccupationController>.Instance)
        {
        }
    }
}
