using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Genders
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Genders")]
    public class GenderController : BaseController<Gender, GenderDto>
    {
        public GenderController(IGenderService service, ILogger<GenderController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<GenderController>.Instance)
        {
        }
    }
}
