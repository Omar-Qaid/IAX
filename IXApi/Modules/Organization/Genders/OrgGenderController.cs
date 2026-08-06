using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.Genders
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Genders")]
    public class OrgGenderController : BaseController<OrgGender, OrgGenderDto>
    {
        public OrgGenderController(IOrgGenderService service, ILogger<OrgGenderController> logger) : base(service, logger)
        {
        }
    }
}
