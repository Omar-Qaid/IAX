using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
namespace IAX.IXApi.Modules.Finance.Inventory;
[ApiController, Route("api/v1/InventParameters"), DomainPermission("Inventory", "Transactions")]
public sealed class InventParametersController(IInventParametersService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) { var value = await service.GetAsync(ct); return value is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<InventParametersDto>.Ok(value)); }
    [HttpGet("lookups")] public async Task<IActionResult> Lookups(CancellationToken ct) { var value = await service.GetLookupsAsync(ct); return value is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<InventParametersLookupsDto>.Ok(value)); }
    [HttpPut] public async Task<IActionResult> Save(InventParametersDto input, CancellationToken ct) { var result = await service.SaveAsync(input, ct); return result.Value is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<InventParametersDto>.Ok(result.Value)); }
}
