using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxLedgerAccountGroupService : IBaseService<TaxLedgerAccountGroup> {
    void Normalize(TaxLedgerAccountGroup entity);
}
