using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry
{
    [ApiController]
    [Route("api/v1/LedgerTrialBalance")]
    [DomainPermission("GeneralLedger", "JournalEntries")]
    public sealed class LedgerTrialBalanceController(ILedgerTrialBalanceService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate,
            [FromQuery] CurrentToOperationsPostingLayer postingLayer = CurrentToOperationsPostingLayer.Current,
            CancellationToken ct = default)
        {
            var (result, error) = await service.GetAsync(fromDate, toDate, postingLayer, ct);
            return result != null ? Ok(result) : UnprocessableEntity(new { error });
        }
    }
}
