using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    public class FiscalCalendarPeriodService : BaseService<FiscalCalendarPeriod>, IFiscalCalendarPeriodService
    {
        public FiscalCalendarPeriodService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

