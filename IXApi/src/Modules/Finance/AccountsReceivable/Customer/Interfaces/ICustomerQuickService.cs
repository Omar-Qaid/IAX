using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public interface ICustomerQuickService
{
    Task<(CustTable Customer, DirPartyTable Party)> QuickCreateAsync(
        CustomerQuickCreateDto input, CancellationToken cancellationToken = default);
    Task<CustomerQuickUpdateResult> QuickUpdateAsync(
        long id, CustomerQuickCreateDto input, CancellationToken cancellationToken = default);
}
