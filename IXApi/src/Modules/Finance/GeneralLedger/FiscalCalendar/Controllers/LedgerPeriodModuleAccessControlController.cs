using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("GeneralLedger", "FiscalCalendars")]
public sealed class LedgerPeriodModuleAccessControlController(
    ILedgerPeriodModuleAccessControlService service,
    ILogger<LedgerPeriodModuleAccessControlController> logger)
    : BaseController<LedgerPeriodModuleAccessControl, LedgerPeriodModuleAccessControlDto>(service, logger);
