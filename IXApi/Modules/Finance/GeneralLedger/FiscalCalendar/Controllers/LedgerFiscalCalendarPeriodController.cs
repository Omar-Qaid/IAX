using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LedgerFiscalCalendarPeriodController : BaseController<LedgerFiscalCalendarPeriod, LedgerFiscalCalendarPeriodDto>
    {
        public LedgerFiscalCalendarPeriodController(ILedgerFiscalCalendarPeriodService service, ILogger<LedgerFiscalCalendarPeriodController> logger) : base(service, logger)
        {
        }
    }
}

