using IAX.IXApi.Infrastructure.Persistence.Services;

using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    public class FiscalCalendarYearService : BaseService<IAX.IXApi.Modules.Finance.Entities.FiscalCalendarYear>, IFiscalCalendarYearService
    {
        public FiscalCalendarYearService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

