using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public class ProductReceiptController : BaseController<VendPackingSlipJour, VendPackingSlipJourDto>
{
    private readonly IProductReceiptService _receiptService;

    public ProductReceiptController(
        IProductReceiptService service,
        ILogger<ProductReceiptController> logger)
        : base(service, logger)
    {
        _receiptService = service;
    }

    [HttpPost("{recId:long}")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Post(long recId, [FromBody] ProductReceiptPostDto input, CancellationToken ct) =>
        Present(await _receiptService.PostAsync(recId, input, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPackingSlipJourDto>>> Create(
        VendPackingSlipJourDto dto,
        CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPackingSlipJourDto>>> Update(
        string id,
        VendPackingSlipJourDto dto,
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
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Product receipt failed."))
    };
}
