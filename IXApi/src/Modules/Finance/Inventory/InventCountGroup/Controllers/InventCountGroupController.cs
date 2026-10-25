using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventCountGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventCountGroupController : BaseController<InventCountGroup, InventCountGroupRecordDto>
{
    private readonly IInventCountGroupService _groupService;

    public InventCountGroupController(
        IInventCountGroupService service,
        ILogger<InventCountGroupController>? logger = null)
        : base(service, logger ?? NullLogger<InventCountGroupController>.Instance)
    {
        _groupService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventCountGroupRecordDto>>.Ok(await _groupService.ListAsync(ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var group = await _groupService.GetAsync(recId, ct);
        return group is null ? NotFound(APIResponse<object>.Fail("Count group was not found."))
            : Ok(APIResponse<InventCountGroupRecordDto>.Ok(group));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventCountGroupInputDto input, CancellationToken ct)
    {
        var result = await _groupService.CreateAsync(input, ct);
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventCountGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventCountGroupInputDto input, CancellationToken ct)
    {
        var result = await _groupService.UpdateAsync(recId, input, ct);
        if (result.Error == "Count group was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventCountGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await _groupService.DeleteAsync(recId, ct);
        if (error == "Count group was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventCountGroupRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventCountGroupRecordDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventCountGroupRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventCountGroupRecordDto>>> Create(InventCountGroupRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventCountGroupRecordDto>>> Update(string id, InventCountGroupRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
