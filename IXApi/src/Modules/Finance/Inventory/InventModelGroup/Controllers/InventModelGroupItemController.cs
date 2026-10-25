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
    [DomainPermission("Inventory", "InventModelGroup")]
    public class InventModelGroupItemController : BaseController<InventModelGroupItem, InventModelGroupItemDto>
    {
        public InventModelGroupItemController(
            IInventModelGroupItemService service,
            ILogger<InventModelGroupItemController>? logger = null)
            : base(service, logger ?? NullLogger<InventModelGroupItemController>.Instance)
        {
        }
    }
}
