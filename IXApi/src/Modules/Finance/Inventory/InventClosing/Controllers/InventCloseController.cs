using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-costing")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventCloseController(IInventCloseService service) : ControllerBase
{
    public sealed record CloseRequest(DateTime Cutoff);

    [HttpPost("items/{itemId}/close")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> CloseItem(string itemId, [FromBody] CloseRequest request, CancellationToken ct)
    {
        if (request.Cutoff == default) return BadRequest(APIResponse<object>.Fail("A cutoff date is required."));
        var (result, error) = await service.CloseItemAsync(itemId, request.Cutoff, ct);
        return error == null ? Ok(APIResponse<InventCloseResultDto>.Ok(result!)) : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("close")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Close([FromBody] CloseRequest request, CancellationToken ct)
    {
        if (request.Cutoff == default)
            return BadRequest(APIResponse<object>.Fail("A cutoff date is required."));
        var (result, error) = await service.CloseAsync(request.Cutoff, ct);
        return error is null
            ? Ok(APIResponse<InventCloseResultDto>.Ok(result!, "Inventory close completed."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("recalculate")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Recalculate([FromBody] CloseRequest request, CancellationToken ct)
    {
        if (request.Cutoff == default)
            return BadRequest(APIResponse<object>.Fail("A cutoff date is required."));
        var (result, error) = await service.RecalculateAsync(request.Cutoff, ct);
        return error is null
            ? Ok(APIResponse<InventCloseResultDto>.Ok(result!, "Inventory recalculation completed."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("items/{itemId}/recalculate")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> RecalculateItem(string itemId, [FromBody] CloseRequest request, CancellationToken ct)
    {
        if (request.Cutoff == default) return BadRequest(APIResponse<object>.Fail("A cutoff date is required."));
        var (result, error) = await service.RecalculateItemAsync(itemId, request.Cutoff, ct);
        return error == null ? Ok(APIResponse<InventCloseResultDto>.Ok(result!)) : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("closings/{closingRecId:long}/cancel")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Cancel(long closingRecId, CancellationToken ct)
    {
        var (result, error) = await service.CancelAsync(closingRecId, ct);
        return error is null
            ? Ok(APIResponse<InventCloseCancelResultDto>.Ok(result!, "Inventory cost run cancelled."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
