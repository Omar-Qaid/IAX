using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Inventory", "InventSettlement")]
    public class InventSettlementController : BaseController<InventSettlement, InventSettlementDto>
    {
        public InventSettlementController(
            IInventSettlementService service,
            ILogger<InventSettlementController>? logger = null)
            : base(service, logger ?? NullLogger<InventSettlementController>.Instance)
        {
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<InventSettlementDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);
    }
}
