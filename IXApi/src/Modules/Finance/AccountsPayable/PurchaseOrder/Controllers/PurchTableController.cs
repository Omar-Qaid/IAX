using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/PurchTable")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public sealed class PurchTableController : BaseController<PurchTable, PurchTableDto>
{
    private readonly IPurchOrderService _orders;
    private readonly IPurchConfirmationService _confirmations;
    private readonly IProductReceiptService _receipts;
    private readonly IVendorInvoiceService _invoices;

    public PurchTableController(
        IPurchOrderService orders,
        ILogger<PurchTableController> logger,
        IPurchConfirmationService confirmations,
        IProductReceiptService receipts,
        IVendorInvoiceService invoices)
        : base(orders, logger)
    {
        _orders = orders;
        _confirmations = confirmations;
        _receipts = receipts;
        _invoices = invoices;
    }

    [HttpPost]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Create")]
    public async Task<IActionResult> Create([FromBody] PurchOrderCreateDto input, CancellationToken ct) =>
        Present(await _orders.CreateAsync(input, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<PurchTableDto>>> Create(PurchTableDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct) =>
        Present(await _orders.GetAsync(recId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<PurchTableDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<PurchTableDto>>> Update(
        string id, PurchTableDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(
        string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpPost("{recId:long}/lines")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Edit")]
    public async Task<IActionResult> AddLine(long recId, [FromBody] PurchLineInputDto input, CancellationToken ct) =>
        Present(await _orders.AddLineAsync(recId, input, ct));

    [HttpPost("{recId:long}/confirm")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Confirm")]
    public async Task<IActionResult> Confirm(long recId, CancellationToken ct) =>
        Present(await _confirmations.ConfirmAsync(recId, ct));

    [HttpPost("{recId:long}/product-receipts")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Receive(long recId, [FromBody] ProductReceiptPostDto input, CancellationToken ct) =>
        Present(await _receipts.PostAsync(recId, input, ct));

    [HttpPost("{recId:long}/invoices")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Invoice(long recId, [FromBody] VendorInvoicePostDto input, CancellationToken ct) =>
        Present(await _invoices.PostAsync(recId, input, ct));

    private IActionResult Present(PurchResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Purchase operation failed."))
    };
}
