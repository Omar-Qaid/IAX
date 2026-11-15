using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public interface IContactPersonService : IBaseService<ContactPerson>
{
    Task<AccountContactsDto?> ListAccountAsync(string kind, string account, CancellationToken ct);
    Task<long?> SaveAccountAsync(string kind, string account, long? id, AccountContactInput input, CancellationToken ct);
}
