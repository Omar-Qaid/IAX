using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public interface ICustomerService : IBaseService<CustTable>
{
    Task<object> GetCountryRegionsAsync(CancellationToken cancellationToken);
    Task<List<CustomerListDto>> GetCustomerListAsync(CancellationToken cancellationToken);
    Task<CustomerSalesOrderDefaultsDto?> GetSalesOrderDefaultsAsync(string accountNumber, CancellationToken cancellationToken);
    CustomerListDto ToListDto(CustTable customer, DirPartyTable? party);
}
