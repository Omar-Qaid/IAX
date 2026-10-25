using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventoryRecordController
    : BaseController<InventDim, InventoryDimensionRecordDto>
{
    private readonly IInventoryRecordQueryService _recordService;

    public InventoryRecordController(
        IInventoryRecordQueryService service,
        ILogger<InventoryRecordController> logger)
        : base(service, logger)
    {
        _recordService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventoryDimensionRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventoryDimensionRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventoryDimensionRecordDto>>> Create(InventoryDimensionRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventoryDimensionRecordDto>>> Update(string id, InventoryDimensionRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet("api/v1/InventDim")]
    public async Task<IActionResult> Dimensions([FromQuery] string? siteId, [FromQuery] string? warehouseId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventoryDimensionRecordDto>>.Ok(await _recordService.DimensionsAsync(siteId, warehouseId, ct)));

    [HttpGet("api/v1/InventItemGroupItem")]
    public async Task<IActionResult> ItemGroups([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<ItemGroupAssignmentRecordDto>>.Ok(await _recordService.ItemGroupsAsync(itemId, ct)));

    [HttpGet("api/v1/InventItemLocation")]
    public async Task<IActionResult> ItemLocations([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<ItemLocationRecordDto>>.Ok(await _recordService.ItemLocationsAsync(itemId, ct)));

    [HttpGet("api/v1/InventSettlement")]
    public async Task<IActionResult> Settlements([FromQuery] string? itemId, [FromQuery] string? voucher, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventorySettlementRecordDto>>.Ok(await _recordService.SettlementsAsync(itemId, voucher, ct)));
}
