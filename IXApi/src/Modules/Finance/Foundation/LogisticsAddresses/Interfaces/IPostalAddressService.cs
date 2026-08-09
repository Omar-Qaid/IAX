using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public interface IPostalAddressService : IBaseService<LogisticsPostalAddress>
    {
        Task<LogisticsPostalAddress> CreatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default);
        Task<LogisticsPostalAddress> UpdatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeletePostalAddressAsync(long postalAddressRecId, CancellationToken cancellationToken = default);
        Task<List<LogisticsPostalAddress>> GetAddressesByLocationsAsync(List<long> locationIds, CancellationToken cancellationToken = default);
    }
}

