using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventBatch")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventBatchController(IInventBatchService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventBatchDto>>.Ok(await service.ListAsync(itemId, ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var batch = await service.GetAsync(recId, ct);
        return batch is null ? NotFound(APIResponse<object>.Fail("Batch was not found.")) : Ok(APIResponse<InventBatchDto>.Ok(batch));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventBatchInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Batch is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<InventBatchDto>.Ok(result.Batch));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventBatchInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        if (result.Error == "Batch was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Batch is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<InventBatchDto>.Ok(result.Batch));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await service.DeleteAsync(recId, ct);
        if (error == "Batch was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }
}
