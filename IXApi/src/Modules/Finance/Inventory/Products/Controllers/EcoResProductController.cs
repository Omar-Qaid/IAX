using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/EcoResProduct")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class EcoResProductController(IEcoResProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<List<EcoResProductDto>>.Ok(await service.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(EcoResProductInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Product is null ? Conflict(APIResponse<EcoResProductDto>.Fail(result.Error!)) : Ok(APIResponse<EcoResProductDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, EcoResProductInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        return result.Product is null ? BadRequest(APIResponse<EcoResProductDto>.Fail(result.Error!)) : Ok(APIResponse<EcoResProductDto>.Ok(result.Product));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await service.DeleteAsync(recId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!));
    }
}
