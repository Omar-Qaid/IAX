using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory.Transfer
{
    [ApiController]
    [Route("api/v1/InventTransferOrder")]
    [DomainPermission("Inventory", "Transactions")]
    public sealed class InventTransferOrderController(IInventTransferOrderService service) : ControllerBase
    {
        [HttpGet("{transferId}")]
        public async Task<IActionResult> Get(string transferId, CancellationToken ct)
        {
            var (order, error) = await service.GetAsync(transferId, ct);
            if (order != null) return Ok(order);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        [HttpPost]
        [DomainPermission("Inventory", "Transactions", "Edit")]
        public async Task<IActionResult> Create(InventTransferCreateDto input, CancellationToken ct) =>
            Result(await service.CreateAsync(input, ct));

        [HttpPost("{transferId}/ship")]
        [DomainPermission("Inventory", "Transactions", "Edit")]
        public async Task<IActionResult> Ship(string transferId, InventTransferPostDto input, CancellationToken ct) =>
            Result(await service.ShipAsync(transferId, input, ct));

        [HttpPost("{transferId}/receive")]
        [DomainPermission("Inventory", "Transactions", "Edit")]
        public async Task<IActionResult> Receive(string transferId, InventTransferPostDto input, CancellationToken ct) =>
            Result(await service.ReceiveAsync(transferId, input, ct));

        private IActionResult Result((InventTransferOrderDto? Order, string? Error) result) =>
            result.Order != null ? Ok(result.Order) : UnprocessableEntity(new { result.Error });
    }
}
