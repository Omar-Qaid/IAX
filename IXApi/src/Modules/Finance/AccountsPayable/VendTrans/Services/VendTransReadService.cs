using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendTransReadService : VendTransService
{
    public VendTransReadService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : base(unitOfWork, company)
    {
    }

    public VendTransReadService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser, company)
    {
    }
}
