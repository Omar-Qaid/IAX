using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public sealed class VendPaymModeService : BaseService<VendPaymModeTable>, IVendPaymModeService
    {
        public VendPaymModeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser) { }
    }
}
