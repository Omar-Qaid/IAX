using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Bank")]
    public class BankAccountStatementController : BaseController<BankAccountStatement, BankAccountStatementDto>
    {
        private readonly IBankAccountStatementService _statementService;

        [HttpGet("reconciliations/{statementId:long}")]
        public async Task<IActionResult> Reconciliation(long statementId, CancellationToken ct)
        {
            var result = await _statementService.GetReconciliationAsync(statementId, ct);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("reconcile")]
        public async Task<IActionResult> Reconcile([FromBody] BankReconcileRequest request, CancellationToken ct)
        {
            var (statement, error) = await _statementService.ReconcileAsync(request.AccountId,
                request.StatementNumber, request.StatementDate, request.EndingBalance,
                request.TransactionIds ?? [], ct);
            return statement == null ? UnprocessableEntity(new { error }) : Ok(new
            {
                statement.RecId, statement.AccountId, statement.AccountStatementNum,
                statement.AccountStatementDate, statement.EndingBalance, statement.CurrencyCode
            });
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<BankAccountStatementDto>>> Create(BankAccountStatementDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Use the reconciliation command.");

        [NonAction]
        public override Task<ActionResult<APIResponse<BankAccountStatementDto>>> Update(string id, BankAccountStatementDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Reconciled statements cannot be edited.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Reconciled statements cannot be deleted.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BankAccountStatementDto>>>> CreateRange(IEnumerable<BankAccountStatementDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Use the reconciliation command.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BankAccountStatementDto>>>> UpdateRange(IEnumerable<BankAccountStatementDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Reconciled statements cannot be edited.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Reconciled statements cannot be deleted.");

        public BankAccountStatementController(
            IBankAccountStatementService service,
            ILogger<BankAccountStatementController>? logger = null)
            : base(service, logger ?? NullLogger<BankAccountStatementController>.Instance)
        {
            _statementService = service;
        }
    }

    public sealed record BankReconcileRequest(string AccountId, string StatementNumber,
        DateTime StatementDate, decimal EndingBalance, IReadOnlyList<long>? TransactionIds);
}
