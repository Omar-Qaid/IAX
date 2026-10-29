using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
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
        // Posted accounting is produced by posting services, never by generic CRUD endpoints.
        [NonAction]
        public override Task<ActionResult<APIResponse<GeneralJournalEntryDto>>> Create(
            GeneralJournalEntryDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted accounting cannot be created through generic CRUD.");

        [NonAction]
        public override Task<ActionResult<APIResponse<GeneralJournalEntryDto>>> Update(
            string id, GeneralJournalEntryDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted accounting cannot be changed through generic CRUD.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(
            string id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted accounting cannot be deleted through generic CRUD.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<GeneralJournalEntryDto>>>> CreateRange(
            IEnumerable<GeneralJournalEntryDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted accounting cannot be created through generic CRUD.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<GeneralJournalEntryDto>>>> UpdateRange(
            IEnumerable<GeneralJournalEntryDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted accounting cannot be changed through generic CRUD.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(
            IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted accounting cannot be deleted through generic CRUD.");

        public GeneralJournalEntryController(
            IGeneralJournalEntryService service,
            ILogger<GeneralJournalEntryController>? logger = null)
            : base(service, logger ?? NullLogger<GeneralJournalEntryController>.Instance)
        {
        }
    }
}
