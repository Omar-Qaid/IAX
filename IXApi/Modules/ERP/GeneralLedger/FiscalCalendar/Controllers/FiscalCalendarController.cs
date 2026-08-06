using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.ERP.GeneralLedger.FiscalCalendar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FiscalCalendarController : BaseController<IAX.IXApi.Modules.ERP.Entities.FiscalCalendar, FiscalCalendarDto>
    {
        public FiscalCalendarController(IFiscalCalendarService service, ILogger<FiscalCalendarController> logger) : base(service, logger)
        {
        }
    }
}
