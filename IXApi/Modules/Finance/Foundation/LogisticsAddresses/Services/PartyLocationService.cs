using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class PartyLocationService : BaseService<DirPartyLocation>, IPartyLocationService
    {
        public PartyLocationService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser) 
            : base(unitOfWork, currentUser)
        {
        }

        private async Task EnforceSinglePrimaryAsync(long partyRecId, long excludeLocationRecId, bool isPostalAddress, CancellationToken cancellationToken)
        {
            var noYesPostal = isPostalAddress ? IAX.IXApi.Modules.Finance.Common.NoYes.Yes : IAX.IXApi.Modules.Finance.Common.NoYes.No;
            var others = await _unitOfWork.Context.Set<DirPartyLocation>()
                .Where(x => x.Party == partyRecId && x.IsPostalAddress == noYesPostal && x.Location != excludeLocationRecId)
                .ToListAsync(cancellationToken);
                
            foreach (var addr in others)
            {
                addr.IsPrimary = IAX.IXApi.Modules.Finance.Common.NoYes.No;
            }
        }

        public async Task<DirPartyLocation> LinkLocationToPartyAsync(long partyRecId, long locationRecId, bool isPostalAddress, bool isPrimary, CancellationToken cancellationToken = default)
        {
            var noYesPostal = isPostalAddress ? IAX.IXApi.Modules.Finance.Common.NoYes.Yes : IAX.IXApi.Modules.Finance.Common.NoYes.No;
            
            var partyLoc = new DirPartyLocation 
            {
                Party = partyRecId,
                Location = locationRecId,
                IsPrimary = isPrimary ? IAX.IXApi.Modules.Finance.Common.NoYes.Yes : IAX.IXApi.Modules.Finance.Common.NoYes.No,
                IsPostalAddress = noYesPostal
            };
            
            _unitOfWork.Context.Set<DirPartyLocation>().Add(partyLoc);
            
            if (isPrimary)
            {
                await EnforceSinglePrimaryAsync(partyRecId, locationRecId, isPostalAddress, cancellationToken);
            }
            
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            return partyLoc;
        }

        public async Task<DirPartyLocation> UpdatePartyLocationPrimaryAsync(long partyRecId, long locationRecId, bool isPostalAddress, bool isPrimary, CancellationToken cancellationToken = default)
        {
            var partyLoc = await _unitOfWork.Context.Set<DirPartyLocation>().FirstOrDefaultAsync(x => x.Party == partyRecId && x.Location == locationRecId, cancellationToken);
            if (partyLoc == null)
            {
                throw new ArgumentException($"DirPartyLocation link not found for Party {partyRecId} and Location {locationRecId}");
            }
            partyLoc.IsPrimary = isPrimary ? IAX.IXApi.Modules.Finance.Common.NoYes.Yes : IAX.IXApi.Modules.Finance.Common.NoYes.No;
            _unitOfWork.Context.Set<DirPartyLocation>().Update(partyLoc);
            
            if (isPrimary)
            {
                await EnforceSinglePrimaryAsync(partyRecId, locationRecId, isPostalAddress, cancellationToken);
            }
            
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            return partyLoc;
        }

        public async Task<bool> UnlinkLocationAsync(long partyRecId, long locationRecId, CancellationToken cancellationToken = default)
        {
            var partyLoc = await _unitOfWork.Context.Set<DirPartyLocation>().FirstOrDefaultAsync(x => x.Party == partyRecId && x.Location == locationRecId, cancellationToken);
            if (partyLoc != null)
            {
                _unitOfWork.Context.Set<DirPartyLocation>().Remove(partyLoc);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteOrphanedLocationAsync(long locationRecId, CancellationToken cancellationToken = default)
        {
            var otherContactsCount = await _unitOfWork.Context.Set<LogisticsElectronicAddress>().CountAsync(x => x.Location == locationRecId, cancellationToken);
            var otherAddressesCount = await _unitOfWork.Context.Set<LogisticsPostalAddress>().CountAsync(x => x.Location == locationRecId, cancellationToken);

            if (otherContactsCount == 0 && otherAddressesCount == 0)
            {
                var loc = await _unitOfWork.Context.Set<LogisticsLocation>().FirstOrDefaultAsync(l => l.RecId == locationRecId, cancellationToken);
                if (loc != null) 
                {
                    var referencingPartyLocs = await _unitOfWork.Context.Set<DirPartyLocation>()
                        .Where(x => x.Location == locationRecId)
                        .ToListAsync(cancellationToken);
                    if (referencingPartyLocs.Any())
                    {
                        _unitOfWork.Context.Set<DirPartyLocation>().RemoveRange(referencingPartyLocs);
                        await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
                    }

                    var trackedEntries = _unitOfWork.Context.ChangeTracker.Entries<DirPartyLocation>()
                        .Where(e => e.Entity.Location == locationRecId)
                        .ToList();
                    foreach (var entry in trackedEntries)
                    {
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }

                    var trackedPostal = _unitOfWork.Context.ChangeTracker.Entries<LogisticsPostalAddress>()
                        .Where(e => e.Entity.Location == locationRecId)
                        .ToList();
                    foreach (var entry in trackedPostal)
                    {
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }

                    var trackedContact = _unitOfWork.Context.ChangeTracker.Entries<LogisticsElectronicAddress>()
                        .Where(e => e.Entity.Location == locationRecId)
                        .ToList();
                    foreach (var entry in trackedContact)
                    {
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }

                    _unitOfWork.Context.Set<LogisticsLocation>().Remove(loc);
                    await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
                    return true;
                }
            }
            return false;
        }

        public async Task<List<DirPartyLocation>> GetPartyLocationsAsync(long partyRecId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Context.Set<DirPartyLocation>()
                .Where(x => x.Party == partyRecId)
                .ToListAsync(cancellationToken);
        }
    }
}


