using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

[ApiController]
[Route("api/v1/inventory-statuses")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventoryStatusController(IInventoryStatusService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await service.ListAsync(ct);
        return rows is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<object>.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventoryStatusInputDto input, CancellationToken ct)
    {
        var result = await service.SaveAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventoryStatusInputDto input, CancellationToken ct)
    {
        var result = await service.SaveAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct) =>
        await service.DeleteAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));
}
