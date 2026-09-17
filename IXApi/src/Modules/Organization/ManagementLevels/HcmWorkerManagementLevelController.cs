using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Managers")]
    public class HcmWorkerManagementLevelController : BaseController<HcmWorkerManagementLevel, HcmWorkerManagementLevelDto>
    {
        public HcmWorkerManagementLevelController(IHcmWorkerManagementLevelService service, ILogger<HcmWorkerManagementLevelController> logger) : base(service, logger)
        {
        }
    }
}
