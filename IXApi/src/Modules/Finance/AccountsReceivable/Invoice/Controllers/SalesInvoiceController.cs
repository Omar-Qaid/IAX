using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

[ApiController]
[Route("api/v1/SalesTable/{salesOrderRecId:long}/invoices")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesInvoiceController(ISalesInvoiceService service) : ControllerBase
{
    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Post")]
    public async Task<IActionResult> Post(long salesOrderRecId, [FromBody] SalesInvoicePostDto? request, CancellationToken ct)
    {
        var result = await service.PostAsync(salesOrderRecId, request ?? new SalesInvoicePostDto(), ct);
        return result.StatusCode switch
        {
            200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
            400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
            404 => NotFound(APIResponse<object>.Fail(result.Message!)),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Sales invoice posting failed."))
        };
    }
}
