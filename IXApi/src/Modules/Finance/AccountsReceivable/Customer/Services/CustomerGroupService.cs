using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public sealed class CustomerGroupService : BaseService<CustGroup>, ICustomerGroupService
{
    public CustomerGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }
}
