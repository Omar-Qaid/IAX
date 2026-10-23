using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTrans")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventTransController(
    IInventTransService service) : ControllerBase
{
    [HttpGet("list")]
    public async Task<ActionResult<APIResponse<IEnumerable<InventTransListDto>>>> GetList(
        [FromQuery] string? itemId = null, [FromQuery] string? sourceInventTransId = null,
        [FromQuery] string? siteId = null, [FromQuery] string? warehouseId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await service.SearchAsync(itemId, sourceInventTransId, siteId, warehouseId, cancellationToken);
        if (result == null)
            return BadRequest(APIResponse<IEnumerable<InventTransListDto>>.Fail(
                "A company must be selected to view inventory transactions."));
        return Ok(APIResponse<IEnumerable<InventTransListDto>>.Ok(result));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<APIResponse<InventTransPageDto>>> Paged(
        [FromQuery] string? itemId, [FromQuery] string? sourceInventTransId,
        [FromQuery] string? siteId, [FromQuery] string? warehouseId,
        [FromQuery] DateTime? physicalFrom, [FromQuery] DateTime? physicalTo,
        [FromQuery] DateTime? financialFrom, [FromQuery] DateTime? financialTo,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 200)
            return BadRequest(APIResponse<InventTransPageDto>.Fail("Page must be positive and page size must be 1 through 200."));
        if (physicalFrom > physicalTo || financialFrom > financialTo)
            return BadRequest(APIResponse<InventTransPageDto>.Fail("The date range start must not follow its end."));
        var result = await service.SearchPageAsync(itemId, sourceInventTransId, siteId, warehouseId,
            physicalFrom, physicalTo, financialFrom, financialTo, page, pageSize, cancellationToken);
        if (result is null)
            return BadRequest(APIResponse<InventTransPageDto>.Fail("A company must be selected to view inventory transactions."));
        return Ok(APIResponse<InventTransPageDto>.Ok(result));
    }
}
