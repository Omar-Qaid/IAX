using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Tax;

public interface ITaxLedgerAccountGroupService : IBaseService<TaxLedgerAccountGroup> {
    void Normalize(TaxLedgerAccountGroup entity);
    Task<object> LedgerAccountsAsync(CancellationToken ct);
}
