using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FiscalCalendarController : BaseController<IAX.IXApi.Modules.Finance.Entities.FiscalCalendar, FiscalCalendarDto>
    {
        public FiscalCalendarController(IFiscalCalendarService service, ILogger<FiscalCalendarController> logger) : base(service, logger)
        {
        }
    }
}

