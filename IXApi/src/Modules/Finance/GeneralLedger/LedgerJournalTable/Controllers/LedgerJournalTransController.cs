using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Journals")]
    public class LedgerJournalTransController : BaseController<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans, LedgerJournalTransDto>
    {
        public LedgerJournalTransController(
            ILedgerJournalTransService service,
            ILogger<LedgerJournalTransController>? logger = null)
            : base(service, logger ?? NullLogger<LedgerJournalTransController>.Instance)
        {
        }

        /// <summary>The lines of one journal (current company), in line order.</summary>
        [HttpGet("by-journal/{journalNum}")]
        public async Task<ActionResult<APIResponse<IEnumerable<LedgerJournalTransDto>>>> GetByJournal(
            string journalNum, CancellationToken cancellationToken = default)
        {
            var lines = await _service.FindAsync(x => x.JournalNum == journalNum, cancellationToken: cancellationToken);
            var dtos = lines.OrderBy(x => x.LineNum).Adapt<List<LedgerJournalTransDto>>();
            return Ok(APIResponse<IEnumerable<LedgerJournalTransDto>>.Ok(dtos));
        }
    }
}
