using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

[ApiController]
[Route("api/v1/customer-settlements")]
[DomainPermission("AccountsReceivable", "CustomerPayments", "View")]
public sealed class CustSettlementReversalController : BaseController<CustSettlement, CustSettlementDto>
{
    private readonly ICustSettlementReversalService _reversal;
    private readonly ICustSettleOpenTransactionsService _settle;

    public CustSettlementReversalController(
        ICustSettlementReversalService service,
        ICustSettleOpenTransactionsService settle,
        ILogger<CustSettlementReversalController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CustSettlementReversalController>.Instance)
    {
        _reversal = service;
        _settle = settle;
    }

    /// <summary>
    /// D365's *Settle open transactions*: applies marked open transactions to each other, which is
    /// how a credit note or an on-account receipt is applied to an invoice without a new payment.
    /// </summary>
    [HttpPost("settle-open-transactions")]
    [DomainPermission("AccountsReceivable", "CustomerPayments", "Post")]
    public async Task<IActionResult> SettleOpenTransactions([FromBody] CustSettleOpenInput input, CancellationToken ct) =>
        Present(await _settle.SettleAsync(input, ct), "Settling open transactions failed.");

    /// <summary>Reverses a posted customer settlement, restoring the invoice's open balance.</summary>
    [HttpPost("{settlementVoucher}/reverse")]
    [DomainPermission("AccountsReceivable", "CustomerPayments", "Post")]
    public async Task<IActionResult> Reverse(string settlementVoucher, [FromQuery] DateTime? reversalDate, CancellationToken ct)
    {
        var result = await _reversal.ReverseAsync(settlementVoucher, reversalDate, ct);
        return Present(result, "Settlement reversal failed.");
    }

    private IActionResult Present(CustPaymentResult result, string fallback) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? fallback))
    };

    [NonAction]
    public override Task<ActionResult<APIResponse<CustSettlementDto>>> Create(
        CustSettlementDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustSettlementDto>>> Update(
        string id, CustSettlementDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(
        string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
