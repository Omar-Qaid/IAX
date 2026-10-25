using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

[ApiController]
[Route("api/v1/inventory-statuses")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventoryStatusController : BaseController<WHSInventStatus, InventoryStatusDto>
{
    private readonly IInventoryStatusService _statusService;

    public InventoryStatusController(
        IInventoryStatusService service,
        ILogger<InventoryStatusController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventoryStatusController>.Instance)
    {
        _statusService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await _statusService.ListAsync(ct);
        return rows is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<object>.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventoryStatusInputDto input, CancellationToken ct)
    {
        var result = await _statusService.SaveAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventoryStatusInputDto input, CancellationToken ct)
    {
        var result = await _statusService.SaveAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct) =>
        await _statusService.DeleteAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventoryStatusDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventoryStatusDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventoryStatusDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventoryStatusDto>>> Create(InventoryStatusDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventoryStatusDto>>> Update(string id, InventoryStatusDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
