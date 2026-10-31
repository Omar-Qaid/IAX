using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "FiscalCalendars")]
    public class LedgerFiscalCalendarPeriodController : BaseController<LedgerFiscalCalendarPeriod, LedgerFiscalCalendarPeriodDto>
    {
        private readonly ILedgerFiscalCalendarPeriodService _periods;

        public LedgerFiscalCalendarPeriodController(ILedgerFiscalCalendarPeriodService service, ILogger<LedgerFiscalCalendarPeriodController> logger) : base(service, logger)
        {
            _periods = service;
        }

        /// <summary>
        /// Opens, holds or permanently closes a ledger period. A permanently closed period is not
        /// reopened here; posting into it is refused by the posting engine, which is the point of
        /// the restriction.
        /// </summary>
        [HttpPost("{recId:long}/status")]
        [DomainPermission("GeneralLedger", "FiscalCalendars", "Edit")]
        public async Task<IActionResult> SetStatus(long recId, LedgerFiscalCalendarPeriodStatusDto input, CancellationToken ct)
        {
            var (result, error) = await _periods.SetStatusAsync(recId, input.Status, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        // Status is posting control data: it changes only through the guarded action above, so that
        // a signed-off period cannot be reopened through generic CRUD.
        [NonAction]
        public override Task<ActionResult<APIResponse<LedgerFiscalCalendarPeriodDto>>> Update(
            string id, LedgerFiscalCalendarPeriodDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Use the period status action to change a ledger period.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<LedgerFiscalCalendarPeriodDto>>>> UpdateRange(
            IEnumerable<LedgerFiscalCalendarPeriodDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Use the period status action to change a ledger period.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(
            string id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("A ledger period status row cannot be deleted; hold or close the period instead.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(
            IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("A ledger period status row cannot be deleted; hold or close the period instead.");
    }
}
