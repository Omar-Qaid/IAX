using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventOnHand")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventOnHandController(IInventOnHandService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<APIResponse<IEnumerable<InventOnHandDto>>>> List(
        [FromQuery] string? itemId = null, [FromQuery] string? siteId = null,
        [FromQuery] string? warehouseId = null, CancellationToken ct = default)
    {
        var rows = await service.SearchAsync(itemId, siteId, warehouseId, ct);
        if (rows is null)
            return BadRequest(APIResponse<IEnumerable<InventOnHandDto>>.Fail(
                "A company must be selected to view on-hand inventory."));
        return Ok(APIResponse<IEnumerable<InventOnHandDto>>.Ok(rows));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<APIResponse<InventOnHandPageDto>>> Paged(
        [FromQuery] string? itemId, [FromQuery] string? itemGroupId, [FromQuery] string? siteId,
        [FromQuery] string? warehouseId, [FromQuery] bool includeZero = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 200)
            return BadRequest(APIResponse<InventOnHandPageDto>.Fail("Page must be positive and page size must be 1 through 200."));
        var result = await service.SearchPageAsync(itemId, itemGroupId, siteId, warehouseId,
            includeZero, page, pageSize, ct);
        if (result is null)
            return BadRequest(APIResponse<InventOnHandPageDto>.Fail("A company must be selected to view on-hand inventory."));
        return Ok(APIResponse<InventOnHandPageDto>.Ok(result));
    }
}
