using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    public interface IPostalAddressService : IBaseService<LogisticsPostalAddress>
    {
        Task<LogisticsPostalAddress> CreatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default);
        Task<LogisticsPostalAddress> UpdatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeletePostalAddressAsync(long postalAddressRecId, CancellationToken cancellationToken = default);
        Task<List<LogisticsPostalAddress>> GetAddressesByLocationsAsync(List<long> locationIds, CancellationToken cancellationToken = default);
    }
}
