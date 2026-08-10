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

        private async Task<(string countryId, string stateId, string countyId, string city, long cityId, string zipCode, long zipCodeId, string districtName, long districtId)> ResolveGeographicalHierarchyAsync(string countryId, string stateId, string countyId, string city, string zipCode, string districtName, CancellationToken cancellationToken)
        {
            var suppliedCountryId = (countryId ?? string.Empty).Trim();
            var suppliedStateId = (stateId ?? string.Empty).Trim();
            var suppliedCountyId = (countyId ?? string.Empty).Trim();
            var cleanCity = (city ?? string.Empty).Trim();
            var cleanZipCode = zipCode ?? string.Empty;
            var cleanDistrictName = districtName ?? string.Empty;

            // The UI can provide an ISO code (SA), address format (SAU), or the
            // persisted country key. Always resolve that value before creating
            // dependent geography rows so their foreign keys remain valid.
            var normalizedCountry = suppliedCountryId.ToUpperInvariant();
            var countryEntity = await _unitOfWork.Context.Set<LogisticsAddressCountryRegion>()
                .FirstOrDefaultAsync(c =>
                    c.CountryRegionId.ToUpper() == normalizedCountry ||
                    c.IsoCode.ToUpper() == normalizedCountry ||
                    c.AddrFormat.ToUpper() == normalizedCountry,
                    cancellationToken);

            if (countryEntity == null)
            {
                throw new ArgumentException($"Unknown country/region '{suppliedCountryId}'.");
            }

            var cleanCountryId = countryEntity.CountryRegionId;
            var stateEntity = await _unitOfWork.Context.Set<LogisticsAddressState>()
                .FirstOrDefaultAsync(s => s.CountryRegionId == cleanCountryId &&
                    (s.StateId == suppliedStateId || s.Name == suppliedStateId), cancellationToken);
            if (stateEntity == null)
            {
                var stateKey = string.IsNullOrWhiteSpace(suppliedStateId) ? string.Empty : suppliedStateId[..Math.Min(suppliedStateId.Length, 30)];
                stateEntity = new LogisticsAddressState
                {
                    CountryRegionId = cleanCountryId,
                    StateId = stateKey,
                    Name = suppliedStateId,
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    DataAreaId = "dat",
                    IsActive = true
                };
                _unitOfWork.Context.Set<LogisticsAddressState>().Add(stateEntity);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            }

            var cleanStateId = stateEntity.StateId;
            var countyEntity = await _unitOfWork.Context.Set<LogisticsAddressCounty>()
                .FirstOrDefaultAsync(c => c.CountryRegionId == cleanCountryId &&
                    c.StateId == cleanStateId &&
                    (c.CountyId == suppliedCountyId || c.Name == suppliedCountyId), cancellationToken);
            if (countyEntity == null)
            {
                var countyKey = string.IsNullOrWhiteSpace(suppliedCountyId) ? string.Empty : suppliedCountyId[..Math.Min(suppliedCountyId.Length, 30)];
                countyEntity = new LogisticsAddressCounty
                {
                    CountryRegionId = cleanCountryId,
                    StateId = cleanStateId,
                    CountyId = countyKey,
                    Name = suppliedCountyId,
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    DataAreaId = "dat",
                    IsActive = true
                };
                _unitOfWork.Context.Set<LogisticsAddressCounty>().Add(countyEntity);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            }

            var cleanCountyId = countyEntity.CountyId;
            var cityEntity = await _unitOfWork.Context.Set<LogisticsAddressCity>()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == cleanCity.ToLower(), cancellationToken);
            if (cityEntity == null)
            {
                cityEntity = new LogisticsAddressCity
                {
                    CityKey = $"{cleanCountryId}-{cleanStateId}-{cleanCity}"[..Math.Min($"{cleanCountryId}-{cleanStateId}-{cleanCity}".Length, 60)],
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
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            }

            var zipEntity = await _unitOfWork.Context.Set<LogisticsAddressZipCode>()
                .FirstOrDefaultAsync(z => z.ZipCode == cleanZipCode, cancellationToken);
            if (zipEntity == null)
            {
                zipEntity = new LogisticsAddressZipCode
                {
                    ZipCode = cleanZipCode,
                    CountryRegionId = cleanCountryId,
                    State = cleanStateId,
                    County = cleanCountyId,
                    City = cityEntity.Name,
                    CityRecId = cityEntity.RecId,
                    CityAlias = cityEntity.Name,
                    DistrictName = cleanDistrictName,
                    StreetName = string.Empty,
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    DataAreaId = "dat",
                    IsActive = true
                };
                _unitOfWork.Context.Set<LogisticsAddressZipCode>().Add(zipEntity);
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            }

            var districtEntity = await _unitOfWork.Context.Set<LogisticsAddressDistrict>()
                .FirstOrDefaultAsync(d => d.Name.ToLower() == cleanDistrictName.ToLower(), cancellationToken);
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
                await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            }

            return (cleanCountryId, cleanStateId, cleanCountyId, cityEntity.Name, cityEntity.RecId, zipEntity.ZipCode, zipEntity.RecId, districtEntity.Name, districtEntity.RecId);
        }

        public async Task<LogisticsPostalAddress> CreatePostalAddressAsync(long locationRecId, AddressInfoDto dto, CancellationToken cancellationToken = default)
        {
            var geo = await ResolveGeographicalHierarchyAsync(dto.CountryRegionId, dto.State, dto.County, dto.City, dto.ZipCode, dto.DistrictName, cancellationToken);

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

            var geo = await ResolveGeographicalHierarchyAsync(dto.CountryRegionId, dto.State, dto.County, dto.City, dto.ZipCode, dto.DistrictName, cancellationToken);

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


