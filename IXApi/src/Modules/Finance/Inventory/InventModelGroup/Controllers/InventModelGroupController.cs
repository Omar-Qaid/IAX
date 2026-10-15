using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventModelGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventModelGroupController(IInventModelGroupService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await service.ListAsync(ct);
        return rows is null ? BadRequest(APIResponse<List<InventModelGroupRecordDto>>.Fail("A legal entity must be selected.")) : Ok(APIResponse<List<InventModelGroupRecordDto>>.Ok(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventModelGroupRecordDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Group is null ? Conflict(APIResponse<InventModelGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventModelGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventModelGroupRecordDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        return result.Group is null ? BadRequest(APIResponse<InventModelGroupRecordDto>.Fail(result.Error!)) : Ok(APIResponse<InventModelGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await service.DeleteAsync(recId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!));
    }
}
