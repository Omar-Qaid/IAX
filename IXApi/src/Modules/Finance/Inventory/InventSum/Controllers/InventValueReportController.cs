using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-reports/value")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventValueReportController(IInventValueReportService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<APIResponse<InventValueReportDto>>> Run(
        [FromQuery] DateOnly? asOfDate, [FromQuery] string? itemId, [FromQuery] string? itemGroupId,
        [FromQuery] string? siteId, [FromQuery] string? warehouseId, [FromQuery] bool includeZero = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (asOfDate is null)
            return BadRequest(APIResponse<InventValueReportDto>.Fail("An as-of date is required."));
        if (page < 1 || pageSize is < 1 or > 200)
            return BadRequest(APIResponse<InventValueReportDto>.Fail("Page must be positive and page size must be 1 through 200."));
        var result = await service.RunAsync(asOfDate.Value, itemId, itemGroupId, siteId, warehouseId,
            includeZero, page, pageSize, ct);
        return result is null
            ? BadRequest(APIResponse<InventValueReportDto>.Fail("A company must be selected."))
            : Ok(APIResponse<InventValueReportDto>.Ok(result));
    }
}
