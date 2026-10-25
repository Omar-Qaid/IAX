using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Inventory", "ItemOrderSetup")]
    public class InventItemInventSetupController : BaseController<InventItemInventSetup, InventItemInventSetupDto>
    {
        public InventItemInventSetupController(
            IInventItemInventSetupService service,
            ILogger<InventItemInventSetupController>? logger = null)
            : base(service, logger ?? NullLogger<InventItemInventSetupController>.Instance)
        {
        }
    }
}
