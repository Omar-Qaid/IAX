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
    public interface IElectronicAddressService : IBaseService<LogisticsElectronicAddress>
    {
        Task<LogisticsElectronicAddress> CreateElectronicAddressAsync(long locationRecId, ContactInfoDto dto, CancellationToken cancellationToken = default);
        Task<LogisticsElectronicAddress> UpdateElectronicAddressAsync(long locationRecId, ContactInfoDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteElectronicAddressAsync(long contactRecId, CancellationToken cancellationToken = default);
        Task<List<LogisticsElectronicAddress>> GetContactsByLocationsAsync(List<long> locationIds, CancellationToken cancellationToken = default);
    }
}
