using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    public class FiscalCalendarYearService : BaseService<IAX.IXApi.Modules.Finance.Entities.FiscalCalendarYear>, IFiscalCalendarYearService
    {
        public FiscalCalendarYearService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public FiscalCalendarYearService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}

