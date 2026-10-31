using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Journals")]
    public class LedgerJournalTableController : BaseController<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable, LedgerJournalTableDto>
    {
        private readonly ILedgerJournalTableService _journalService;
        private readonly ILedgerJournalReversalService _reversalService;

        [HttpGet("{recId:long}/setup-check")]
        public async Task<IActionResult> CheckSetup(long recId, CancellationToken ct)
        {
            var result = await _journalService.CheckSetupAsync(recId, ct);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("{recId:long}/voucher-balance-check")]
        public async Task<IActionResult> CheckVoucherBalances(long recId, CancellationToken ct)
        {
            var errors = await _journalService.CheckVoucherBalancesAsync(recId, ct);
            return errors == null ? NotFound() : Ok(new
            {
                errors,
                balanced = errors.Count == 0,
                scope = "Transaction-currency arithmetic only; not posting validation or approval."
            });
        }

        /// <summary>The static checks, combined. Reads only and authorizes nothing.</summary>
        [HttpGet("{recId:long}/validate")]
        public async Task<IActionResult> Validate(long recId, CancellationToken ct)
        {
            var result = await _journalService.ValidateAsync(recId, ct);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Runs the posting path and rolls it back, reporting what would post or why it is refused.
        /// A POST because it opens a transaction and takes the posting lock, not because it writes:
        /// nothing survives the call.
        /// </summary>
        [HttpPost("{recId:long}/simulate")]
        public async Task<IActionResult> Simulate(long recId, CancellationToken ct)
        {
            var (result, error) = await _journalService.SimulateAsync(recId, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        [HttpPost("{recId:long}/post")]
        [DomainPermission("GeneralLedger", "Journals", "Edit")]
        public async Task<IActionResult> Post(long recId, CancellationToken ct)
        {
            var (result, error) = await _journalService.PostAsync(recId, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        [HttpPost("{recId:long}/reverse")]
        [DomainPermission("GeneralLedger", "Journals", "Edit")]
        public async Task<IActionResult> Reverse(long recId, [FromQuery] DateTime? reversalDate, CancellationToken ct)
        {
            var (result, error) = await _reversalService.ReverseAsync(recId, reversalDate, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        public LedgerJournalTableController(
            ILedgerJournalTableService service,
            ILedgerJournalReversalService reversalService,
            ILogger<LedgerJournalTableController>? logger = null)
            : base(service, logger ?? NullLogger<LedgerJournalTableController>.Instance)
        {
            _journalService = service;
            _reversalService = reversalService;
        }
    }
}
