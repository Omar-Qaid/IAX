using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PostingProfile;

public sealed class CustLedgerAccountsService : BaseService<CustLedgerAccounts>, ICustLedgerAccountsService
{
    public CustLedgerAccountsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }
}
