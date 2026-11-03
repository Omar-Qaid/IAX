using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory/markings")]
[DomainPermission("Inventory", "Inquiry", "View")]
public sealed class InventoryMarkingController(IInventoryMarkingService service) : ControllerBase
{
    /// <param name="IssueTransRecId">The movement to mark when the issue is split; null otherwise.</param>
    /// <param name="ReceiptTransRecId">The movement to mark when the receipt is split; null otherwise.</param>
    public sealed record MarkRequest(string IssueInventTransId, string ReceiptInventTransId,
        long? IssueTransRecId = null, long? ReceiptTransRecId = null);

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
        var (result, error) = await service.MarkAsync(request.IssueInventTransId, request.ReceiptInventTransId,
            request.IssueTransRecId, request.ReceiptTransRecId, ct);
        return error is null
            ? Ok(APIResponse<InventoryMarkingDto>.Ok(result!, "Inventory transaction marked."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpDelete("{inventTransId}")]
    [DomainPermission("Inventory", "Posting", "Edit")]
    public async Task<IActionResult> Unmark(string inventTransId, [FromQuery] long? inventTransRecId, CancellationToken ct)
    {
        var (result, error) = await service.UnmarkAsync(inventTransId, inventTransRecId, ct);
        return error is null
            ? Ok(APIResponse<InventoryMarkingDto>.Ok(result!, "Marking removed."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
