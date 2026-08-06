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
    public interface IPartyLocationService : IBaseService<DirPartyLocation>
    {
        Task<DirPartyLocation> LinkLocationToPartyAsync(long partyRecId, long locationRecId, bool isPostalAddress, bool isPrimary, CancellationToken cancellationToken = default);
        Task<DirPartyLocation> UpdatePartyLocationPrimaryAsync(long partyRecId, long locationRecId, bool isPostalAddress, bool isPrimary, CancellationToken cancellationToken = default);
        Task<bool> UnlinkLocationAsync(long partyRecId, long locationRecId, CancellationToken cancellationToken = default);
        Task<bool> DeleteOrphanedLocationAsync(long locationRecId, CancellationToken cancellationToken = default);
        Task<List<DirPartyLocation>> GetPartyLocationsAsync(long partyRecId, CancellationToken cancellationToken = default);
    }
}
