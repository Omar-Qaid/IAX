using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

[ApiController]
[Route("api/v1/product-statuses")]
[DomainPermission("Inventory", "Transactions")]
public sealed class ProductLifecycleController(IProductLifecycleService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await service.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(ProductLifecycleStateInputDto input, CancellationToken ct)
    {
        var result = await service.SaveAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, ProductLifecycleStateInputDto input, CancellationToken ct)
    {
        var result = await service.SaveAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct) =>
        await service.DeleteAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [HttpPut("released-products/{releasedProductRecId:long}")]
    public async Task<IActionResult> Assign(long releasedProductRecId, ReleasedProductStatusInputDto input, CancellationToken ct)
    {
        var result = await service.AssignAsync(releasedProductRecId, input.StateId, ct);
        return result.Item is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Item));
    }
}
