using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.ERP.GeneralLedger.FiscalCalendar
{
    public interface IFiscalCalendarPeriodService : IBaseService<IAX.IXApi.Modules.ERP.Entities.FiscalCalendarPeriod>
    {
    }
}
