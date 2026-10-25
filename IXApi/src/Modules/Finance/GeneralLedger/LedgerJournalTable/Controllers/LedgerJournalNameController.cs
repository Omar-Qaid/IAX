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
    public class LedgerJournalNameController : BaseController<IAX.IXApi.Modules.Finance.Entities.LedgerJournalName, LedgerJournalNameDto>
    {
        public LedgerJournalNameController(
            ILedgerJournalNameService service,
            ILogger<LedgerJournalNameController>? logger = null)
            : base(service, logger ?? NullLogger<LedgerJournalNameController>.Instance)
        {
        }
    }
}
