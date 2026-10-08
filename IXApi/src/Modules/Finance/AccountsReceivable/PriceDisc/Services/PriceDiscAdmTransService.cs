using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscAdmTransService : BaseService<PriceDiscAdmTrans>, IPriceDiscAdmTransService
    {
        public PriceDiscAdmTransService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}
