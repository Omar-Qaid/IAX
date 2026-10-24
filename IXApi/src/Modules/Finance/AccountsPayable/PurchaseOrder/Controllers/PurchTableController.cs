using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/PurchTable")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public sealed class PurchTableController(IPurchOrderService orders, IPurchConfirmationService confirmations,
    IProductReceiptService receipts, IVendorInvoiceService invoices) : ControllerBase
{
    [HttpPost]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Create")]
    public async Task<IActionResult> Create([FromBody] PurchOrderCreateDto input, CancellationToken ct) =>
        Present(await orders.CreateAsync(input, ct));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct) =>
        Present(await orders.GetAsync(recId, ct));

    [HttpPost("{recId:long}/lines")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Edit")]
    public async Task<IActionResult> AddLine(long recId, [FromBody] PurchLineInputDto input, CancellationToken ct) =>
        Present(await orders.AddLineAsync(recId, input, ct));

    [HttpPost("{recId:long}/confirm")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Confirm")]
    public async Task<IActionResult> Confirm(long recId, CancellationToken ct) =>
        Present(await confirmations.ConfirmAsync(recId, ct));

    [HttpPost("{recId:long}/product-receipts")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Receive(long recId, [FromBody] ProductReceiptPostDto input, CancellationToken ct) =>
        Present(await receipts.PostAsync(recId, input, ct));

    [HttpPost("{recId:long}/invoices")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Invoice(long recId, [FromBody] VendorInvoicePostDto input, CancellationToken ct) =>
        Present(await invoices.PostAsync(recId, input, ct));

    private IActionResult Present(PurchResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Purchase operation failed."))
    };
}
