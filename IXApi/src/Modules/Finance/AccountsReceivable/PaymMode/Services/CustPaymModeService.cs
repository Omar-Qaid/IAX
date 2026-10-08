using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PaymMode;

public sealed class CustPaymModeService : BaseService<CustPaymModeTable>, ICustPaymModeService
{
    public CustPaymModeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }
}
