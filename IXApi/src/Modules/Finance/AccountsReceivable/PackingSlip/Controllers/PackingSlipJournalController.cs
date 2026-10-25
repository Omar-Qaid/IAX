using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

[ApiController]
[Route("api/v1/SalesTable/{salesOrderRecId:long}/packing-slips")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class PackingSlipJournalController : BaseController<CustPackingSlipJour, CustPackingSlipJourDto>
{
    private readonly IPackingSlipJournalService _journalService;

    public PackingSlipJournalController(
        IPackingSlipJournalService service,
        ILogger<PackingSlipJournalController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PackingSlipJournalController>.Instance)
    {
        _journalService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(long salesOrderRecId, CancellationToken ct) =>
        Present(await _journalService.ListAsync(salesOrderRecId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustPackingSlipJourDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [HttpGet("posting-preview")]
    public async Task<IActionResult> PreviewPosting(long salesOrderRecId, [FromQuery] string quantityMode = "all",
        CancellationToken ct = default) =>
        Present(await _journalService.PreviewPostingAsync(salesOrderRecId, quantityMode, ct));

    [HttpGet("{packingSlipRecId:long}")]
    public async Task<IActionResult> Get(long salesOrderRecId, long packingSlipRecId, CancellationToken ct) =>
        Present(await _journalService.GetAsync(salesOrderRecId, packingSlipRecId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<CustPackingSlipJourDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustPackingSlipJourDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Post(long salesOrderRecId, [FromBody] PostPackingSlipRequest? request,
        CancellationToken ct) => Present(await _journalService.PostAsync(salesOrderRecId, request, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<CustPackingSlipJourDto>>> Create(CustPackingSlipJourDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustPackingSlipJourDto>>> Update(string id, CustPackingSlipJourDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    private IActionResult Present(PackingSlipJournalResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message!)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Packing slip request failed."))
    };
}
