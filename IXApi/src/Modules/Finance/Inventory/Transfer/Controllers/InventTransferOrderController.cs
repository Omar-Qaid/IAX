using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory/transfers")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventTransferOrderController(IInventTransferOrderService service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        await service.GetAsync(id, ct) is { } order ? Ok(APIResponse<InventTransferOrder>.Ok(order)) : NotFound();
    [HttpPost]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Create(TransferOrderInput input, CancellationToken ct) => Result(await service.CreateAsync(input, ct));
    [HttpPost("{id}/ship")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Ship(string id, TransferPostingInput input, CancellationToken ct) => Result(await service.PostAsync(id, input, false, ct));
    [HttpPost("{id}/receive")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Receive(string id, TransferPostingInput input, CancellationToken ct) => Result(await service.PostAsync(id, input, true, ct));
    private IActionResult Result((InventTransferOrder? Order, string? Error) result) => result.Error == null
        ? Ok(APIResponse<InventTransferOrder>.Ok(result.Order!)) : UnprocessableEntity(APIResponse<InventTransferOrder>.Fail(result.Error));
}
