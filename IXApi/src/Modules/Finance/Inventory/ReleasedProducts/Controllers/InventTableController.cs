using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventTableEntity = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Inventory", "Products")]
    public class InventTableController : BaseController<InventTableEntity, InventTableDto>
    {
        public InventTableController(
            IInventTableService service,
            ILogger<InventTableController>? logger = null)
            : base(service, logger ?? NullLogger<InventTableController>.Instance)
        {
        }
    }
}
