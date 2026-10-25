using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "JournalEntries")]
    public class GeneralJournalEntryController : BaseController<GeneralJournalEntry, GeneralJournalEntryDto>
    {
        public GeneralJournalEntryController(
            IGeneralJournalEntryService service,
            ILogger<GeneralJournalEntryController>? logger = null)
            : base(service, logger ?? NullLogger<GeneralJournalEntryController>.Instance)
        {
        }
    }
}
