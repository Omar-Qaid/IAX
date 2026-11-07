using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/[controller]")]
[Route("api/[controller]")]
[DomainPermission("AccountsReceivable", "Transactions", "View")]
public class CustTransController : BaseController<CustTrans, CustTransDto>
{
    private readonly ICustTransService _custTransService;

    public CustTransController(
        ICustTransService service,
        ILogger<CustTransController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CustTransController>.Instance)
    {
        _custTransService = service;
    }

    /// <summary>The customer's open balance and the open transactions behind it.</summary>
    [HttpGet("open-balance")]
    public async Task<ActionResult<APIResponse<CustOpenBalanceDto>>> OpenBalance(
        [FromQuery] string? accountNum, [FromQuery] DateTime? asOfDate, CancellationToken ct) =>
        Ok(APIResponse<CustOpenBalanceDto>.Ok(await _custTransService.GetOpenBalanceAsync(accountNum, asOfDate, ct)));

    [HttpGet("statement")]
    public async Task<ActionResult<APIResponse<CustStatementDto>>> Statement(
        [FromQuery] string accountNum, [FromQuery, BindRequired] DateTime fromDate,
        [FromQuery, BindRequired] DateTime toDate, CancellationToken ct) =>
        Ok(APIResponse<CustStatementDto>.Ok(await _custTransService.GetStatementAsync(accountNum, fromDate, toDate, ct)));

    [HttpGet("aging")]
    public async Task<ActionResult<APIResponse<CustAgingDto>>> Aging(
        [FromQuery] string accountNum, [FromQuery, BindRequired] DateTime balanceAsOf,
        [FromQuery] DateTime? agingAsOf, CancellationToken ct) =>
        Ok(APIResponse<CustAgingDto>.Ok(await _custTransService.GetAgingAsync(
            accountNum, balanceAsOf, agingAsOf ?? balanceAsOf, ct)));

    [NonAction]
    public override Task<ActionResult<APIResponse<CustTransDto>>> Create(CustTransDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustTransDto>>> Update(string id, CustTransDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
