using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;

using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    public class LedgerFiscalCalendarPeriodService : BaseService<LedgerFiscalCalendarPeriod>, ILedgerFiscalCalendarPeriodService
    {
        public LedgerFiscalCalendarPeriodService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

