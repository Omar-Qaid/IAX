using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
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

