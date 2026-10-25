using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

[ApiController]
[Route("api/v1/SalesTable/{salesOrderRecId:long}/invoices")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesInvoiceController : BaseController<CustInvoiceJour, CustInvoiceJourDto>
{
    private readonly ISalesInvoiceService _salesInvoiceService;

    public SalesInvoiceController(
        ISalesInvoiceService service,
        ILogger<SalesInvoiceController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SalesInvoiceController>.Instance)
    {
        _salesInvoiceService = service;
    }

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Post")]
    public async Task<IActionResult> Post(long salesOrderRecId, [FromBody] SalesInvoicePostDto? request, CancellationToken ct)
    {
        var result = await _salesInvoiceService.PostAsync(salesOrderRecId, request ?? new SalesInvoicePostDto(), ct);
        return result.StatusCode switch
        {
            200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
            400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
            404 => NotFound(APIResponse<object>.Fail(result.Message!)),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Sales invoice posting failed."))
        };
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustInvoiceJourDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustInvoiceJourDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustInvoiceJourDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustInvoiceJourDto>>> Create(CustInvoiceJourDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustInvoiceJourDto>>> Update(string id, CustInvoiceJourDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
