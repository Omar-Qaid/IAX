using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    [ScopedService]
    public class LocationService : BaseService<LogisticsLocation>, ILocationService
    {
        private readonly ISysNumberSequenceService _numberSequenceService;

        public LocationService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser, 
            ISysNumberSequenceService numberSequenceService) 
            : base(unitOfWork, currentUser)
        {
            _numberSequenceService = numberSequenceService;
        }

        public async Task<LogisticsLocation> CreateLocationAsync(string description, bool isPostalAddress, CancellationToken cancellationToken = default)
        {
            var nsResult = await _numberSequenceService.NextAsync("LogisticsLocation", cancellationToken: cancellationToken);
            string locationId = nsResult.Code ?? Guid.NewGuid().ToString("N").Substring(0, 20);

            var location = new LogisticsLocation 
            {
                LocationId = locationId,
                Description = description ?? string.Empty,
                IsPostalAddress = isPostalAddress ? IAX.IXApi.Modules.ERP.Common.NoYes.Yes : IAX.IXApi.Modules.ERP.Common.NoYes.No
            };

            _unitOfWork.Context.Set<LogisticsLocation>().Add(location);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);

            return location;
        }

        public async Task<LogisticsLocation> UpdateLocationDescriptionAsync(long locationRecId, string description, CancellationToken cancellationToken = default)
        {
            var loc = await _unitOfWork.Context.Set<LogisticsLocation>().FirstOrDefaultAsync(l => l.RecId == locationRecId, cancellationToken);
            if (loc == null)
            {
                throw new ArgumentException($"LogisticsLocation not found for RecId {locationRecId}");
            }
            loc.Description = description ?? string.Empty;
            _unitOfWork.Context.Set<LogisticsLocation>().Update(loc);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            
            return loc;
        }

        public async Task<bool> DeleteLocationAsync(long locationRecId, CancellationToken cancellationToken = default)
        {
            var loc = await _unitOfWork.Context.Set<LogisticsLocation>().FirstOrDefaultAsync(l => l.RecId == locationRecId, cancellationToken);
            if (loc != null) 
            {
                _unitOfWork.Context.Set<LogisticsLocation>().Remove(loc);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
    }
}
