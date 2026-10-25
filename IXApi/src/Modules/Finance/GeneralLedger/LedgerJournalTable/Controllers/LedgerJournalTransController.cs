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
    public class LedgerJournalTransController : BaseController<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans, LedgerJournalTransDto>
    {
        public LedgerJournalTransController(
            ILedgerJournalTransService service,
            ILogger<LedgerJournalTransController>? logger = null)
            : base(service, logger ?? NullLogger<LedgerJournalTransController>.Instance)
        {
        }
    }
}
