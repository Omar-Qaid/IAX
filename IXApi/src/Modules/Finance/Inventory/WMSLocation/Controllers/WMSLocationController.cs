using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/WMSLocation")]
[DomainPermission("Inventory", "Transactions")]
public sealed class WMSLocationController(IWMSLocationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<object>.Ok(await service.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(WMSLocationInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Location!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, WMSLocationInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        if (result.Error == "Inventory location was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Location!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await service.DeleteAsync(recId, ct);
        if (error == "Inventory location was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error == null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }
}
