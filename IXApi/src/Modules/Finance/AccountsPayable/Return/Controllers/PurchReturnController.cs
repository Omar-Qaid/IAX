using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/vendor-returns")]
[DomainPermission("AccountsPayable", "PurchaseOrders", "View")]
public sealed class PurchReturnController : BaseController<PurchTable, PurchReturnDto>
{
    private readonly IPurchReturnService _returnService;

    public PurchReturnController(
        IPurchReturnService service,
        ILogger<PurchReturnController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PurchReturnController>.Instance)
    {
        _returnService = service;
    }

    [HttpPost]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Create")]
    public async Task<IActionResult> Create([FromBody] PurchReturnCreateDto input, CancellationToken ct) =>
        Present(await _returnService.CreateAsync(input, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<PurchReturnDto>>> Create(PurchReturnDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<PurchReturnDto>>> Update(string id, PurchReturnDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<PurchReturnDto>>>> GetAll(
        CancellationToken cancellationToken = default) => base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<PurchReturnDto>>>> GetPaged(
        QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<PurchReturnDto>>> GetById(
        string id, CancellationToken cancellationToken = default) => base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(
        CancellationToken cancellationToken = default) => base.GetNumberSequence(cancellationToken);

    [HttpPost("{recId:long}/shipments")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Ship(long recId, [FromBody] PurchReturnShipDto input, CancellationToken ct) =>
        Present(await _returnService.ShipAsync(recId, input, ct));

    [HttpPost("{recId:long}/credit-notes")]
    [DomainPermission("AccountsPayable", "PurchaseOrders", "Post")]
    public async Task<IActionResult> Credit(long recId, [FromBody] VendCreditNoteDto input, CancellationToken ct) =>
        Present(await _returnService.CreditAsync(recId, input, ct));

    private IActionResult Present(PurchResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Vendor return operation failed."))
    };
}
