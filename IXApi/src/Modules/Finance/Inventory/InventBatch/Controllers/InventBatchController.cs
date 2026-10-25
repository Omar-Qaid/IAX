using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventBatch")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventBatchController : BaseController<InventBatch, InventBatchDto>
{
    private readonly IInventBatchService _batchService;

    public InventBatchController(
        IInventBatchService service,
        ILogger<InventBatchController>? logger = null)
        : base(service, logger ?? NullLogger<InventBatchController>.Instance)
    {
        _batchService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventBatchDto>>.Ok(await _batchService.ListAsync(itemId, ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var batch = await _batchService.GetAsync(recId, ct);
        return batch is null ? NotFound(APIResponse<object>.Fail("Batch was not found.")) : Ok(APIResponse<InventBatchDto>.Ok(batch));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventBatchInputDto input, CancellationToken ct)
    {
        var result = await _batchService.CreateAsync(input, ct);
        return result.Batch is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<InventBatchDto>.Ok(result.Batch));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventBatchInputDto input, CancellationToken ct)
    {
        var result = await _batchService.UpdateAsync(recId, input, ct);
        if (result.Error == "Batch was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Batch is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<InventBatchDto>.Ok(result.Batch));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await _batchService.DeleteAsync(recId, ct);
        if (error == "Batch was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventBatchDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventBatchDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventBatchDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventBatchDto>>> Create(InventBatchDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventBatchDto>>> Update(string id, InventBatchDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
