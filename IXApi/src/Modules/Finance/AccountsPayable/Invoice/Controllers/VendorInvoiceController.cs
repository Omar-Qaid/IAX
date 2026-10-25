using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public class VendorInvoiceController : BaseController<VendInvoiceJour, VendInvoiceJourDto>
{
    private readonly IVendorInvoiceService _invoiceService;

    public VendorInvoiceController(
        IVendorInvoiceService service,
        ILogger<VendorInvoiceController> logger)
        : base(service, logger)
    {
        _invoiceService = service;
    }

    [HttpPost("{recId:long}")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Post(long recId, [FromBody] VendorInvoicePostDto input, CancellationToken ct) =>
        Present(await _invoiceService.PostAsync(recId, input, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<VendInvoiceJourDto>>> Create(
        VendInvoiceJourDto dto,
        CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendInvoiceJourDto>>> Update(
        string id,
        VendInvoiceJourDto dto,
        CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(
        string id,
        CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    private IActionResult Present(PurchResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Vendor invoice operation failed."))
    };
}
