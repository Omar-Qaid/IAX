using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesPoolService : BaseService<SalesPool>, ISalesPoolService
{
    public SalesPoolService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }
}
