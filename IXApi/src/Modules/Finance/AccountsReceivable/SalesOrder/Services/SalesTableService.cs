using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public class SalesTableService : BaseService<SalesTable>, ISalesTableService
{
    public SalesTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }
}
