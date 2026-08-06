using System;
using System.Collections.Generic;
using System.Linq;
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

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    [ScopedService]
    public class ElectronicAddressService : BaseService<LogisticsElectronicAddress>, IElectronicAddressService
    {
        public ElectronicAddressService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser) 
            : base(unitOfWork, currentUser)
        {
        }

        public async Task<LogisticsElectronicAddress> CreateElectronicAddressAsync(long locationRecId, ContactInfoDto dto, CancellationToken cancellationToken = default)
        {
            Enum.TryParse<IAX.IXApi.Modules.ERP.Common.ElectronicAddressType>(dto.Type, out var typeEnum);

            var electronic = new LogisticsElectronicAddress 
            {
                Location = locationRecId,
                Description = dto.Description ?? string.Empty,
                Type = typeEnum,
                Locator = dto.Number ?? string.Empty,
                LocatorExtension = dto.Extension ?? string.Empty,
                IsPrimary = dto.Primary ? IAX.IXApi.Modules.ERP.Common.NoYes.Yes : IAX.IXApi.Modules.ERP.Common.NoYes.No
            };
            
            _unitOfWork.Context.Set<LogisticsElectronicAddress>().Add(electronic);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            return electronic;
        }

        public async Task<LogisticsElectronicAddress> UpdateElectronicAddressAsync(long locationRecId, ContactInfoDto dto, CancellationToken cancellationToken = default)
        {
            long.TryParse(dto.Id, out long contactRecId);
            var electronic = await _unitOfWork.Context.Set<LogisticsElectronicAddress>().FirstOrDefaultAsync(x => x.RecId == contactRecId, cancellationToken);
            if (electronic == null)
            {
                throw new ArgumentException($"LogisticsElectronicAddress not found for RecId {contactRecId}");
            }

            Enum.TryParse<IAX.IXApi.Modules.ERP.Common.ElectronicAddressType>(dto.Type, out var typeEnum);
            
            electronic.Type = typeEnum;
            electronic.Locator = dto.Number ?? string.Empty;
            electronic.LocatorExtension = dto.Extension ?? string.Empty;
            electronic.Description = dto.Description ?? string.Empty;
            electronic.IsPrimary = dto.Primary ? IAX.IXApi.Modules.ERP.Common.NoYes.Yes : IAX.IXApi.Modules.ERP.Common.NoYes.No;

            _unitOfWork.Context.Set<LogisticsElectronicAddress>().Update(electronic);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            
            return electronic;
        }

        public async Task<bool> DeleteElectronicAddressAsync(long contactRecId, CancellationToken cancellationToken = default)
        {
            var electronic = await _unitOfWork.Context.Set<LogisticsElectronicAddress>().FirstOrDefaultAsync(x => x.RecId == contactRecId, cancellationToken);
            if (electronic != null)
            {
                _unitOfWork.Context.Set<LogisticsElectronicAddress>().Remove(electronic);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<List<LogisticsElectronicAddress>> GetContactsByLocationsAsync(List<long> locationIds, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Context.Set<LogisticsElectronicAddress>()
                .Where(x => locationIds.Contains(x.Location))
                .ToListAsync(cancellationToken);
        }
    }
}
