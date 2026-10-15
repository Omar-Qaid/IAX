using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/EcoResStorageDimensionGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class EcoResStorageDimensionGroupController(IStorageDimensionGroupService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<List<StorageDimensionGroupDto>>.Ok(await service.ListAsync(ct)));
    [HttpPost] public async Task<IActionResult> Create(StorageDimensionGroupInputDto input, CancellationToken ct) { var result = await service.CreateAsync(input, ct); return result.Group is null ? Conflict(APIResponse<StorageDimensionGroupDto>.Fail(result.Error!)) : Ok(APIResponse<StorageDimensionGroupDto>.Ok(result.Group)); }
    [HttpPut("{recId:long}")] public async Task<IActionResult> Update(long recId, StorageDimensionGroupInputDto input, CancellationToken ct) { var result = await service.UpdateAsync(recId, input, ct); return result.Group is null ? BadRequest(APIResponse<StorageDimensionGroupDto>.Fail(result.Error!)) : Ok(APIResponse<StorageDimensionGroupDto>.Ok(result.Group)); }
    [HttpDelete("{recId:long}")] public async Task<IActionResult> Delete(long recId, CancellationToken ct) { var result = await service.DeleteAsync(recId, ct); return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!)); }
}
