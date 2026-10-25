using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventPackaging")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventPackagingController : BaseController<InventPackagingGroup, InventPackagingGroupRecordDto>
{
    private readonly IInventPackagingService _packagingService;

    public InventPackagingController(
        IInventPackagingService service,
        ILogger<InventPackagingController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventPackagingController>.Instance)
    {
        _packagingService = service;
    }

    [HttpGet("groups")]
    public async Task<IActionResult> Groups(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventPackagingGroupRecordDto>>.Ok(await _packagingService.GroupsAsync(ct)));

    [HttpGet("units")]
    public async Task<IActionResult> Units(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventPackagingUnitRecordDto>>.Ok(await _packagingService.UnitsAsync(ct)));

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup(InventPackagingGroupInputDto input, CancellationToken ct)
    {
        var result = await _packagingService.CreateGroupAsync(input, ct);
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventPackagingGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("groups/{recId:long}")]
    public async Task<IActionResult> UpdateGroup(long recId, InventPackagingGroupInputDto input, CancellationToken ct)
    {
        var result = await _packagingService.UpdateGroupAsync(recId, input, ct);
        if (result.Error == "Packaging group was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventPackagingGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("groups/{recId:long}")]
    public async Task<IActionResult> DeleteGroup(long recId, CancellationToken ct)
    {
        var error = await _packagingService.DeleteGroupAsync(recId, ct);
        if (error == "Packaging group was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventPackagingGroupRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventPackagingGroupRecordDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventPackagingGroupRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventPackagingGroupRecordDto>>> Create(InventPackagingGroupRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventPackagingGroupRecordDto>>> Update(string id, InventPackagingGroupRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
