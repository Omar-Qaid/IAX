using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTable/released-products/{releasedProductRecId:long}/barcodes")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventItemBarcodeController(IInventItemBarcodeService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(long releasedProductRecId, CancellationToken ct)
    {
        var result = await service.ListAsync(releasedProductRecId, ct);
        return result.Rows is null ? NotFound(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(long releasedProductRecId, InventItemBarcodeInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(releasedProductRecId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("{barcodeRecId:long}")]
    public async Task<IActionResult> Update(long releasedProductRecId, long barcodeRecId, InventItemBarcodeInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(releasedProductRecId, barcodeRecId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("{barcodeRecId:long}")]
    public async Task<IActionResult> Delete(long releasedProductRecId, long barcodeRecId, CancellationToken ct)
    {
        var result = await service.DeleteAsync(releasedProductRecId, barcodeRecId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : NotFound(APIResponse<object>.Fail(result.Error!));
    }
}
