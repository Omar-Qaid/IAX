using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory/markings")]
[DomainPermission("Inventory", "Inquiry", "View")]
public sealed class InventoryMarkingController(IInventoryMarkingService service) : ControllerBase
{
    public sealed record MarkRequest(string IssueInventTransId, string ReceiptInventTransId);

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? itemId, CancellationToken ct)
    {
        var rows = await service.ListAsync(itemId, ct);
        return rows is null
            ? BadRequest(APIResponse<object>.Fail("A company must be selected."))
            : Ok(APIResponse<IReadOnlyList<InventoryMarkingRowDto>>.Ok(rows));
    }

    [HttpPost]
    [DomainPermission("Inventory", "Posting", "Edit")]
    public async Task<IActionResult> Mark([FromBody] MarkRequest request, CancellationToken ct)
    {
        var (result, error) = await service.MarkAsync(request.IssueInventTransId, request.ReceiptInventTransId, ct);
        return error is null
            ? Ok(APIResponse<InventoryMarkingDto>.Ok(result!, "Inventory transaction marked."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpDelete("{inventTransId}")]
    [DomainPermission("Inventory", "Posting", "Edit")]
    public async Task<IActionResult> Unmark(string inventTransId, CancellationToken ct)
    {
        var (result, error) = await service.UnmarkAsync(inventTransId, ct);
        return error is null
            ? Ok(APIResponse<InventoryMarkingDto>.Ok(result!, "Marking removed."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
