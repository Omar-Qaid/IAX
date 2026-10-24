using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventItemLocation")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventItemLocationController(IInventItemLocationService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(InventItemLocationInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Location is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<ItemLocationRecordDto>.Ok(result.Location));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventItemLocationInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        if (result.Error == "Item location was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Location is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<ItemLocationRecordDto>.Ok(result.Location));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await service.DeleteAsync(recId, ct);
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : NotFound(APIResponse<object>.Fail(error));
    }
}
