using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventItemGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventItemGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventItemGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventItemGroupController : BaseController<InventItemGroupEntity, InventItemGroupRecordDto>
{
    private readonly IInventItemGroupService _itemGroupService;

    public InventItemGroupController(
        IInventItemGroupService service,
        ILogger<InventItemGroupController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventItemGroupController>.Instance)
    {
        _itemGroupService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await _itemGroupService.ListAsync(ct);
        return rows is null ? BadRequest(APIResponse<List<InventItemGroupRecordDto>>.Fail("A legal entity must be selected.")) : Ok(APIResponse<List<InventItemGroupRecordDto>>.Ok(rows));
    }

    [HttpGet("ledger-accounts")]
    public async Task<IActionResult> LedgerAccounts(CancellationToken ct)
    {
        var rows = await _itemGroupService.LedgerAccountsAsync(ct);
        return rows is null ? BadRequest(APIResponse<List<InventItemGroupPostingDto>>.Fail("A legal entity must be selected.")) : Ok(APIResponse<List<InventItemGroupPostingDto>>.Ok(rows));
    }

    [HttpPost]
    public override async Task<ActionResult<APIResponse<InventItemGroupRecordDto>>> Create(InventItemGroupRecordDto input, CancellationToken cancellationToken = default)
    {
        var result = await _itemGroupService.CreateAsync(input, cancellationToken);
        return result.Group is null ? Conflict(APIResponse<InventItemGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventItemGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventItemGroupRecordDto input, CancellationToken ct)
    {
        var result = await _itemGroupService.UpdateAsync(recId, input, ct);
        return result.Group is null ? BadRequest(APIResponse<InventItemGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventItemGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await _itemGroupService.DeleteAsync(recId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventItemGroupRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventItemGroupRecordDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemGroupRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemGroupRecordDto>>> Update(string id, InventItemGroupRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
