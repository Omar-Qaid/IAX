using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public interface IElectronicAddressService : IBaseService<LogisticsElectronicAddress>
    {
        Task<LogisticsElectronicAddress> CreateElectronicAddressAsync(long locationRecId, ContactInfoDto dto, CancellationToken cancellationToken = default);
        Task<LogisticsElectronicAddress> UpdateElectronicAddressAsync(long locationRecId, ContactInfoDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteElectronicAddressAsync(long contactRecId, CancellationToken cancellationToken = default);
        Task<List<LogisticsElectronicAddress>> GetContactsByLocationsAsync(List<long> locationIds, CancellationToken cancellationToken = default);
    }
}

