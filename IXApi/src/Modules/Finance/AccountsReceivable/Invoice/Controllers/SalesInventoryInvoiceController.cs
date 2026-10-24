using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

[ApiController]
[Route("api/v1/SalesTable/{salesId}/invoice")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesInventoryInvoiceController(ISalesInventoryInvoiceService service) : ControllerBase
{
    public sealed record InvoiceRequest(DateTime? InvoiceDate);

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Post(string salesId, [FromBody] InvoiceRequest? request, CancellationToken ct)
    {
        var (result, error) = await service.PostAsync(salesId, request?.InvoiceDate, ct);
        return error is null
            ? Ok(APIResponse<SalesInvoiceResultDto>.Ok(result!, "Sales invoice posted."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
