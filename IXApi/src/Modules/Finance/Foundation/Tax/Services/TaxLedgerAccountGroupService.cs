using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxLedgerAccountGroupService : BaseService<TaxLedgerAccountGroup>, ITaxLedgerAccountGroupService
{
    public TaxLedgerAccountGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }
}
