using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

[ApiController]
[Route("api/v1/customer-payments")]
[DomainPermission("AccountsReceivable", "CustomerPayments", "View")]
public sealed class CustPaymentController : BaseController<CustTrans, CustPaymentPostDto>
{
    private readonly ICustPaymentService _paymentService;
    private readonly ICustPaymentReversalService _reversalService;

    public CustPaymentController(
        ICustPaymentService service,
        ICustPaymentReversalService reversalService,
        ILogger<CustPaymentController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CustPaymentController>.Instance)
    {
        _paymentService = service;
        _reversalService = reversalService;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustPaymentPostDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustPaymentPostDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CustPaymentPostDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustPaymentPostDto>>> Create(CustPaymentPostDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustPaymentPostDto>>> Update(string id, CustPaymentPostDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpPost]
    [DomainPermission("AccountsReceivable", "CustomerPayments", "Post")]
    public async Task<IActionResult> Post([FromBody] CustPaymentPostDto request, CancellationToken ct)
    {
        var result = await _paymentService.PostAsync(request, ct);
        return result.StatusCode switch
        {
            200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
            400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
            404 => NotFound(APIResponse<object>.Fail(result.Message!)),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Customer payment posting failed."))
        };
    }

    [HttpPost("refund")]
    [DomainPermission("AccountsReceivable", "CustomerPayments", "Post")]
    public async Task<IActionResult> Refund([FromBody] CustRefundPostDto request, CancellationToken ct)
    {
        var result = await _paymentService.RefundAsync(request, ct);
        return StatusCode(result.StatusCode, result.StatusCode == 200
            ? APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)
            : APIResponse<object>.Fail(result.Message ?? "Customer refund failed."));
    }

    /// <summary>
    /// Reverses a posted customer payment: unwinds its settlements, posts the counter-voucher and
    /// reverses the bank movement.
    /// </summary>
    [HttpPost("{journalNum}/reverse")]
    [DomainPermission("AccountsReceivable", "CustomerPayments", "Post")]
    public async Task<IActionResult> Reverse(string journalNum, [FromQuery] DateTime? reversalDate, CancellationToken ct)
    {
        var result = await _reversalService.ReverseAsync(journalNum, reversalDate, ct);
        return result.StatusCode switch
        {
            200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
            400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
            404 => NotFound(APIResponse<object>.Fail(result.Message!)),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Customer payment reversal failed."))
        };
    }
}
