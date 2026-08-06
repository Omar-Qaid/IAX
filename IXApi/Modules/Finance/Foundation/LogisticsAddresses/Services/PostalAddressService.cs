using System;
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
    public class PostalAddressService : BaseService<LogisticsPostalAddress>, IPostalAddressService
    {
        public PostalAddressService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser) 
            : base(unitOfWork, currentUser)
        {
        }

        private async Task<(string countryId, string stateId, string countyId, string city, long cityId, string zipCode, long zipCodeId, string districtName, long districtId)> ResolveGeographicalHierarchyAsync(string countryId, string stateId, string countyId, string city, string zipCode, string districtName)
        {
            var cleanCountryId = countryId ?? string.Empty;
            var cleanStateId = stateId ?? string.Empty;
            var cleanCountyId = countyId ?? string.Empty;
            var cleanCity = city ?? string.Empty;
            var cleanZipCode = zipCode ?? string.Empty;
            var cleanDistrictName = districtName ?? string.Empty;

            // Simple logic matching what LogisticsAddressService used to do
            var cityEntity = await _unitOfWork.Context.Set<LogisticsAddressCity>()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cleanCity.ToLower());
            if (cityEntity == null)
            {
                cityEntity = new LogisticsAddressCity
                {
                    Name = cleanCity,
                    Description = $"{cleanCity} City",
                    CountryRegionId = cleanCountryId,
                    StateId = cleanStateId,
                    CountyId = cleanCountyId,
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    DataAreaId = "dat",
                    IsActive = true
                };
                _unitOfWork.Context.Set<LogisticsAddressCity>().Add(cityEntity);
                await _unitOfWork.Context.SaveChangesAsync();
            }

            var zipEntity = await _unitOfWork.Context.Set<LogisticsAddressZipCode>()
                .FirstOrDefaultAsync(z => z.ZipCode == cleanZipCode);
            if (zipEntity == null)
            {
                zipEntity = new LogisticsAddressZipCode
                {
                    ZipCode = cleanZipCode,
                    City = cityEntity.RecId.ToString(),
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    DataAreaId = "dat",
                    IsActive = true
                };
                _unitOfWork.Context.Set<LogisticsAddressZipCode>().Add(zipEntity);
                await _unitOfWork.Context.SaveChangesAsync();
            }

            var districtEntity = await _unitOfWork.Context.Set<LogisticsAddressDistrict>()
                .FirstOrDefaultAsync(d => d.Name.ToLower() == cleanDistrictName.ToLower());
            if (districtEntity == null)
            {
                districtEntity = new LogisticsAddressDistrict
                {
                    Name = cleanDistrictName,
                    Description = $"{cleanDistrictName} District",
                    City = cityEntity.RecId,
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    DataAreaId = "dat",
                    IsActive = true
                };
                _unitOfWork.Context.Set<LogisticsAddressDistrict>().Add(districtEntity);
                await _unitOfWork.Context.SaveChangesAsync();
            }

            return (cleanCountryId, cleanStateId, cleanCountyId, cityEntity.Name, cityEntity.RecId, zipEntity.ZipCode, zipEntity.RecId, districtEntity.Name, districtEntity.RecId);
        }

        public async Task<LogisticsPostalAddress> CreatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default)
        {
            var geo = await ResolveGeographicalHierarchyAsync(dto.CountryRegionId, dto.State, dto.County, dto.City, dto.ZipCode, dto.DistrictName);

            var postal = new LogisticsPostalAddress 
            {
                Location = locationRecId,
                CountryRegionId = geo.countryId,
                ZipCode = geo.zipCode,
                State = geo.stateId,
                County = geo.countyId,
                City = geo.city,
                DistrictName = geo.districtName,
                CityRecId = geo.cityId,
                ZipCodeRecId = geo.zipCodeId,
                District = geo.districtId,
                Street = dto.Street ?? string.Empty,
                Address = $"{dto.Street}, {geo.city}, {geo.stateId} {geo.zipCode}, {geo.countryId}",
                ValidFrom = dto.ValidFrom ?? DateTime.MinValue,
                ValidTo = dto.ValidTo ?? DateTime.MaxValue
            };
            
            _unitOfWork.Context.Set<LogisticsPostalAddress>().Add(postal);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            return postal;
        }

        public async Task<LogisticsPostalAddress> UpdatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default)
        {
            long.TryParse(dto.Id, out long postalRecId);
            var postal = await _unitOfWork.Context.Set<LogisticsPostalAddress>().FirstOrDefaultAsync(x => x.RecId == postalRecId, cancellationToken);
            if (postal == null)
            {
                throw new ArgumentException($"LogisticsPostalAddress not found for RecId {postalRecId}");
            }

            var geo = await ResolveGeographicalHierarchyAsync(dto.CountryRegionId, dto.State, dto.County, dto.City, dto.ZipCode, dto.DistrictName);

            postal.CountryRegionId = geo.countryId;
            postal.ZipCode = geo.zipCode;
            postal.State = geo.stateId;
            postal.County = geo.countyId;
            postal.City = geo.city;
            postal.DistrictName = geo.districtName;
            postal.CityRecId = geo.cityId;
            postal.ZipCodeRecId = geo.zipCodeId;
            postal.District = geo.districtId;
            postal.Street = dto.Street ?? string.Empty;
            postal.Address = $"{dto.Street}, {geo.city}, {geo.stateId} {geo.zipCode}, {geo.countryId}";
            postal.ValidFrom = dto.ValidFrom ?? DateTime.MinValue;
            postal.ValidTo = dto.ValidTo ?? DateTime.MaxValue;

            _unitOfWork.Context.Set<LogisticsPostalAddress>().Update(postal);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            
            return postal;
        }

        public async Task<bool> DeletePostalAddressAsync(long postalAddressRecId, CancellationToken cancellationToken = default)
        {
            var postal = await _unitOfWork.Context.Set<LogisticsPostalAddress>().FirstOrDefaultAsync(x => x.RecId == postalAddressRecId, cancellationToken);
            if (postal != null)
            {
                _unitOfWork.Context.Set<LogisticsPostalAddress>().Remove(postal);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<List<LogisticsPostalAddress>> GetAddressesByLocationsAsync(List<long> locationIds, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Context.Set<LogisticsPostalAddress>()
                .Include(x => x.LogisticsLocationTable)
                .Where(x => locationIds.Contains(x.Location))
                .ToListAsync(cancellationToken);
        }
    }
}


