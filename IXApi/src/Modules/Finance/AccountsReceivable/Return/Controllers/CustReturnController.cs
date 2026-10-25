using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

[ApiController]
[Route("api/v1/return-orders")]
[DomainPermission("AccountsReceivable", "ReturnOrders", "View")]
public sealed class CustReturnController : BaseController<SalesTable, CustReturnDto>
{
    private readonly ICustReturnService _returnService;

    public CustReturnController(
        ICustReturnService service,
        ILogger<CustReturnController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CustReturnController>.Instance)
    {
        _returnService = service;
    }

    [HttpPost]
    [DomainPermission("AccountsReceivable", "ReturnOrders", "Create")]
    public async Task<IActionResult> Create([FromBody] CustReturnCreateDto input, CancellationToken ct) =>
        Present(await _returnService.CreateAsync(input, ct));

    [NonAction]
    public override Task<ActionResult<APIResponse<CustReturnDto>>> Create(CustReturnDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustReturnDto>>> Update(string id, CustReturnDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpPost("{recId:long}/receipts")]
    [DomainPermission("AccountsReceivable", "ReturnOrders", "Post")]
    public async Task<IActionResult> Receive(long recId, [FromBody] CustReturnReceiptDto input, CancellationToken ct) =>
        Present(await _returnService.ReceiveAsync(recId, input, ct));

    [HttpPost("{recId:long}/credit-notes")]
    [DomainPermission("AccountsReceivable", "ReturnOrders", "Post")]
    public async Task<IActionResult> Credit(long recId, [FromBody] CustCreditNoteDto? input, CancellationToken ct) =>
        Present(await _returnService.CreditAsync(recId, input ?? new CustCreditNoteDto(), ct));

    private IActionResult Present(CustReturnResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Return order operation failed."))
    };
}
