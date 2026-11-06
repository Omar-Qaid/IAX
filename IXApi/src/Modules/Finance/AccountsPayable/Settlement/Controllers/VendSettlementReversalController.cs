using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/vendor-settlements")]
[DomainPermission("AccountsPayable", "VendorPayments", "View")]
public sealed class VendSettlementReversalController : BaseController<VendSettlement, VendSettlementDto>
{
    private readonly IVendSettlementReversalService _reversal;
    private readonly IVendSettleOpenTransactionsService _settle;

    public VendSettlementReversalController(
        IVendSettlementReversalService service,
        IVendSettleOpenTransactionsService settle,
        ILogger<VendSettlementReversalController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<VendSettlementReversalController>.Instance)
    {
        _reversal = service;
        _settle = settle;
    }

    /// <summary>
    /// D365's *Settle open transactions*: applies marked open transactions to each other, which is
    /// how a vendor credit note or an on-account payment is applied to an invoice.
    /// </summary>
    [HttpPost("settle-open-transactions")]
    [DomainPermission("AccountsPayable", "VendorPayments", "Post")]
    public async Task<IActionResult> SettleOpenTransactions([FromBody] VendSettleOpenInput input, CancellationToken ct) =>
        Present(await _settle.SettleAsync(input, ct), "Settling open transactions failed.");

    /// <summary>Reverses a posted vendor settlement, restoring the invoice's open balance.</summary>
    [HttpPost("{voucher}/reverse")]
    [DomainPermission("AccountsPayable", "VendorPayments", "Post")]
    public async Task<IActionResult> Reverse(string voucher, [FromQuery] DateTime? reversalDate, CancellationToken ct)
    {
        var result = await _reversal.ReverseAsync(voucher, reversalDate, ct);
        return Present(result, "Settlement reversal failed.");
    }

    private IActionResult Present(PurchResult result, string fallback) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? fallback))
    };

    [NonAction]
    public override Task<ActionResult<APIResponse<VendSettlementDto>>> Create(
        VendSettlementDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendSettlementDto>>> Update(
        string id, VendSettlementDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(
        string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
