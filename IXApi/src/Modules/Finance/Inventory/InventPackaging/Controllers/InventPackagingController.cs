using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventPackaging")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventPackagingController(IInventPackagingService service) : ControllerBase
{
    [HttpGet("groups")]
    public async Task<IActionResult> Groups(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventPackagingGroupRecordDto>>.Ok(await service.GroupsAsync(ct)));

    [HttpGet("units")]
    public async Task<IActionResult> Units(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventPackagingUnitRecordDto>>.Ok(await service.UnitsAsync(ct)));

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup(InventPackagingGroupInputDto input, CancellationToken ct)
    {
        var result = await service.CreateGroupAsync(input, ct);
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventPackagingGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("groups/{recId:long}")]
    public async Task<IActionResult> UpdateGroup(long recId, InventPackagingGroupInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateGroupAsync(recId, input, ct);
        if (result.Error == "Packaging group was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventPackagingGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("groups/{recId:long}")]
    public async Task<IActionResult> DeleteGroup(long recId, CancellationToken ct)
    {
        var error = await service.DeleteGroupAsync(recId, ct);
        if (error == "Packaging group was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }
}
