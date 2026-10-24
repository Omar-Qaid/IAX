using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventoryRecordController(IInventoryRecordQueryService service) : ControllerBase
{
    [HttpGet("api/v1/InventDim")]
    public async Task<IActionResult> Dimensions([FromQuery] string? siteId, [FromQuery] string? warehouseId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventoryDimensionRecordDto>>.Ok(await service.DimensionsAsync(siteId, warehouseId, ct)));

    [HttpGet("api/v1/InventItemGroupItem")]
    public async Task<IActionResult> ItemGroups([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<ItemGroupAssignmentRecordDto>>.Ok(await service.ItemGroupsAsync(itemId, ct)));

    [HttpGet("api/v1/InventItemLocation")]
    public async Task<IActionResult> ItemLocations([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<ItemLocationRecordDto>>.Ok(await service.ItemLocationsAsync(itemId, ct)));

    [HttpGet("api/v1/InventSettlement")]
    public async Task<IActionResult> Settlements([FromQuery] string? itemId, [FromQuery] string? voucher, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventorySettlementRecordDto>>.Ok(await service.SettlementsAsync(itemId, voucher, ct)));
}
