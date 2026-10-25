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
    public class InventItemPurchSetupController : BaseController<InventItemPurchSetup, InventItemPurchSetupDto>
    {
        public InventItemPurchSetupController(
            IInventItemPurchSetupService service,
            ILogger<InventItemPurchSetupController>? logger = null)
            : base(service, logger ?? NullLogger<InventItemPurchSetupController>.Instance)
        {
        }
    }
}
