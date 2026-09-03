using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "FiscalCalendars")]
    public class FiscalCalendarYearController : BaseController<IAX.IXApi.Modules.Finance.Entities.FiscalCalendarYear, FiscalCalendarYearDto>
    {
        public FiscalCalendarYearController(IFiscalCalendarYearService service, ILogger<FiscalCalendarYearController> logger) : base(service, logger)
        {
        }
    }
}

