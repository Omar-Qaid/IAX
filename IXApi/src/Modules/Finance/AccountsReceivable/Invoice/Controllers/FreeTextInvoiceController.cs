using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

[ApiController]
[Route("api/v1/CustInvoiceTable")]
[DomainPermission("AccountsReceivable", "Invoices", "View")]
public sealed class FreeTextInvoiceController(IFreeTextInvoiceService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<FreeTextInvoiceDto>>.Ok(await service.ListAsync(ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var invoice = await service.GetAsync(recId, ct);
        return invoice == null
            ? NotFound(APIResponse<object>.Fail("Free text invoice was not found."))
            : Ok(APIResponse<FreeTextInvoiceDto>.Ok(invoice));
    }

    [HttpPost]
    [DomainPermission("AccountsReceivable", "Invoices", "Create")]
    public Task<IActionResult> Create([FromBody] FreeTextInvoiceSaveDto input, CancellationToken ct) =>
        Present(service.CreateAsync(input, ct));

    [HttpPut("{recId:long}")]
    [DomainPermission("AccountsReceivable", "Invoices", "Edit")]
    public Task<IActionResult> Update(long recId, [FromBody] FreeTextInvoiceSaveDto input, CancellationToken ct) =>
        Present(service.UpdateAsync(recId, input, ct));

    [HttpDelete("{recId:long}")]
    [DomainPermission("AccountsReceivable", "Invoices", "Delete")]
    public Task<IActionResult> Delete(long recId, CancellationToken ct) =>
        Present(service.DeleteAsync(recId, ct));

    [HttpPost("{recId:long}/post")]
    [DomainPermission("AccountsReceivable", "Invoices", "Post")]
    public Task<IActionResult> Post(long recId, CancellationToken ct) =>
        Present(service.PostAsync(recId, ct));

    private async Task<IActionResult> Present(Task<FreeTextInvoiceResult> operation)
    {
        var result = await operation;
        var response = result.Invoice == null
            ? APIResponse<object>.Ok(new { }, result.Message ?? string.Empty)
            : APIResponse<object>.Ok(result.Invoice, result.Message ?? string.Empty);
        return result.StatusCode switch
        {
            200 => Ok(response),
            201 => StatusCode(201, response),
            400 => BadRequest(APIResponse<object>.Fail(result.Message ?? "Invalid free text invoice.")),
            404 => NotFound(APIResponse<object>.Fail(result.Message ?? "Free text invoice was not found.")),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Free text invoice operation failed."))
        };
    }
}
