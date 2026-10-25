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
        public LedgerJournalTableController(
            ILedgerJournalTableService service,
            ILogger<LedgerJournalTableController>? logger = null)
            : base(service, logger ?? NullLogger<LedgerJournalTableController>.Instance)
        {
        }
    }
}
