using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTable/released-products/{releasedProductRecId:long}/barcodes")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventItemBarcodeController
    : BaseController<InventItemBarcode, InventItemBarcodeDto>
{
    private readonly IInventItemBarcodeService _barcodeService;

    public InventItemBarcodeController(
        IInventItemBarcodeService service,
        ILogger<InventItemBarcodeController> logger)
        : base(service, logger)
    {
        _barcodeService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventItemBarcodeDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemBarcodeDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemBarcodeDto>>> Create(InventItemBarcodeDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemBarcodeDto>>> Update(string id, InventItemBarcodeDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet("/api/v1/InventTable/barcodes/lookup")]
    public async Task<IActionResult> Find([FromQuery] string barcode, CancellationToken ct)
    {
        var result = await _barcodeService.FindAsync(barcode, ct);
        return result.Rows is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Rows));
    }

    [HttpGet]
    public async Task<IActionResult> List(long releasedProductRecId, CancellationToken ct)
    {
        var result = await _barcodeService.ListAsync(releasedProductRecId, ct);
        return result.Rows is null ? NotFound(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Rows));
    }

    [HttpPost]
    public async Task<IActionResult> Create(long releasedProductRecId, InventItemBarcodeInputDto input, CancellationToken ct)
    {
        var result = await _barcodeService.CreateAsync(releasedProductRecId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("{barcodeRecId:long}")]
    public async Task<IActionResult> Update(long releasedProductRecId, long barcodeRecId, InventItemBarcodeInputDto input, CancellationToken ct)
    {
        var result = await _barcodeService.UpdateAsync(releasedProductRecId, barcodeRecId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("{barcodeRecId:long}")]
    public async Task<IActionResult> Delete(long releasedProductRecId, long barcodeRecId, CancellationToken ct)
    {
        var result = await _barcodeService.DeleteAsync(releasedProductRecId, barcodeRecId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : NotFound(APIResponse<object>.Fail(result.Error!));
    }
}
