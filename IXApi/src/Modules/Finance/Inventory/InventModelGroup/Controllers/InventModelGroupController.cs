using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventModelGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventModelGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventModelGroupController : BaseController<InventModelGroupEntity, InventModelGroupRecordDto>
{
    private readonly IInventModelGroupService _modelGroupService;

    public InventModelGroupController(
        IInventModelGroupService service,
        ILogger<InventModelGroupController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventModelGroupController>.Instance)
    {
        _modelGroupService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await _modelGroupService.ListAsync(ct);
        return rows is null ? BadRequest(APIResponse<List<InventModelGroupRecordDto>>.Fail("A legal entity must be selected.")) : Ok(APIResponse<List<InventModelGroupRecordDto>>.Ok(rows));
    }

    [HttpPost]
    public override async Task<ActionResult<APIResponse<InventModelGroupRecordDto>>> Create(InventModelGroupRecordDto input, CancellationToken cancellationToken = default)
    {
        var result = await _modelGroupService.CreateAsync(input, cancellationToken);
        return result.Group is null ? Conflict(APIResponse<InventModelGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventModelGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventModelGroupRecordDto input, CancellationToken ct)
    {
        var result = await _modelGroupService.UpdateAsync(recId, input, ct);
        return result.Group is null ? BadRequest(APIResponse<InventModelGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventModelGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await _modelGroupService.DeleteAsync(recId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventModelGroupRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventModelGroupRecordDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventModelGroupRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventModelGroupRecordDto>>> Update(string id, InventModelGroupRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
