using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/EcoResTrackingDimensionGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class EcoResTrackingDimensionGroupController(ITrackingDimensionGroupService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<List<TrackingDimensionGroupDto>>.Ok(await service.ListAsync(ct)));
    [HttpPost] public async Task<IActionResult> Create(TrackingDimensionGroupInputDto input, CancellationToken ct) { var result = await service.CreateAsync(input, ct); return result.Group is null ? Conflict(APIResponse<TrackingDimensionGroupDto>.Fail(result.Error!)) : Ok(APIResponse<TrackingDimensionGroupDto>.Ok(result.Group)); }
    [HttpPut("{recId:long}")] public async Task<IActionResult> Update(long recId, TrackingDimensionGroupInputDto input, CancellationToken ct) { var result = await service.UpdateAsync(recId, input, ct); return result.Group is null ? BadRequest(APIResponse<TrackingDimensionGroupDto>.Fail(result.Error!)) : Ok(APIResponse<TrackingDimensionGroupDto>.Ok(result.Group)); }
    [HttpDelete("{recId:long}")] public async Task<IActionResult> Delete(long recId, CancellationToken ct) { var result = await service.DeleteAsync(recId, ct); return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!)); }
}
