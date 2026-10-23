using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-reports/physical-as-of")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventPhysicalAsOfController(IInventPhysicalAsOfService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<APIResponse<InventPhysicalAsOfPageDto>>> Search(
        [FromQuery] DateOnly? asOfDate, [FromQuery] string? itemId,
        [FromQuery] string? siteId, [FromQuery] string? warehouseId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (asOfDate is null)
            return BadRequest(APIResponse<InventPhysicalAsOfPageDto>.Fail("An as-of date is required."));
        if (page < 1 || pageSize is < 1 or > 200)
            return BadRequest(APIResponse<InventPhysicalAsOfPageDto>.Fail("Page must be positive and page size must be 1 through 200."));
        var result = await service.SearchAsync(asOfDate.Value, itemId, siteId, warehouseId, page, pageSize, ct);
        if (result is null)
            return BadRequest(APIResponse<InventPhysicalAsOfPageDto>.Fail("A company must be selected."));
        return Ok(APIResponse<InventPhysicalAsOfPageDto>.Ok(result));
    }
}
