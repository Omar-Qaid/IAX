using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventSerial")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventSerialController(IInventSerialService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventSerialRecordDto>>.Ok(await service.ListAsync(itemId, ct)));

    [HttpPost]
    public async Task<IActionResult> Create(InventSerialInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Serial is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventSerialRecordDto>.Ok(result.Serial));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventSerialInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        if (result.Error == "Serial was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Serial is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventSerialRecordDto>.Ok(result.Serial));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await service.DeleteAsync(recId, ct);
        if (error == "Serial was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }
}
