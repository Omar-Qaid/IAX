using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

[ApiController]
[Route("api/v1/SalesTable/{recId:long}/confirmations")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesOrderConfirmationController : BaseController<CustConfirmJour, CustConfirmJourDto>
{
    private readonly ISalesOrderConfirmationService _confirmationService;

    public SalesOrderConfirmationController(
        ISalesOrderConfirmationService service,
        ILogger<SalesOrderConfirmationController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SalesOrderConfirmationController>.Instance)
    {
        _confirmationService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(long recId, CancellationToken ct) =>
        Present(await _confirmationService.ListAsync(recId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustConfirmJourDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [HttpGet("{confirmationRecId:long}")]
    public async Task<IActionResult> Get(long recId, long confirmationRecId, CancellationToken ct) =>
        Present(await _confirmationService.GetAsync(recId, confirmationRecId, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<CustConfirmJourDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustConfirmJourDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [HttpGet("preview")]
    public async Task<IActionResult> Preview(long recId, CancellationToken ct) =>
        Present(await _confirmationService.PreviewAsync(recId, ct));

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Post(long recId, [FromBody] PostConfirmationRequest? request, CancellationToken ct) =>
        Present(await _confirmationService.PostAsync(recId, request, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<CustConfirmJourDto>>> Create(CustConfirmJourDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustConfirmJourDto>>> Update(string id, CustConfirmJourDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    private IActionResult Present(ConfirmationResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message!)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Confirmation failed."))
    };
}
