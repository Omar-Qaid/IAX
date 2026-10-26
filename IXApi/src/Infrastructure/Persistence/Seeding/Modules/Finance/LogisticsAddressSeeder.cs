using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class LogisticsAddressSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.Foundation.Logistics";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            // 0. Location Roles
            var rolesToSeed = new[] {
                new LogisticsLocationRole { Name = "Business", IsPostalAddress = NoYes.Yes, IsContactInfo = NoYes.Yes },
                new LogisticsLocationRole { Name = "Delivery", IsPostalAddress = NoYes.Yes, IsContactInfo = NoYes.No },
                new LogisticsLocationRole { Name = "Invoice", IsPostalAddress = NoYes.Yes, IsContactInfo = NoYes.No },
                new LogisticsLocationRole { Name = "Home", IsPostalAddress = NoYes.Yes, IsContactInfo = NoYes.Yes },
                new LogisticsLocationRole { Name = "Remit-to", IsPostalAddress = NoYes.Yes, IsContactInfo = NoYes.No }
            };

            foreach (var r in rolesToSeed)
            {
                var existingRole = await db.Set<LogisticsLocationRole>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == r.Name, cancellationToken);
                if (existingRole == null)
                {
                    r.CreatedBy = createdBy;
                    r.OwnerAccountId = createdBy;
                    r.DataAreaId = "dat";
                    r.IsActive = true;
                    await db.Set<LogisticsLocationRole>().AddAsync(r, cancellationToken);
                }
            }
            await db.SaveChangesAsync(cancellationToken);

            // 1. Countries
            var countryUS = await db.Set<LogisticsAddressCountryRegion>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "US", cancellationToken);
            if (countryUS == null)
            {
                countryUS = new LogisticsAddressCountryRegion { CountryRegionId = "US", IsoCode = "US", AddrFormat = "USA", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCountryRegion>().AddAsync(countryUS, cancellationToken);
            }
            var countrySA = await db.Set<LogisticsAddressCountryRegion>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "SA", cancellationToken);
            if (countrySA == null)
            {
                countrySA = new LogisticsAddressCountryRegion { CountryRegionId = "SA", IsoCode = "SA", AddrFormat = "SAU", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCountryRegion>().AddAsync(countrySA, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 2. States
            var stateCA = await db.Set<LogisticsAddressState>().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StateId == "CA", cancellationToken);
            if (stateCA == null)
            {
                stateCA = new LogisticsAddressState { StateId = "CA", Name = "California", CountryRegionId = "US", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressState>().AddAsync(stateCA, cancellationToken);
            }
            var stateRIY = await db.Set<LogisticsAddressState>().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StateId == "RIY", cancellationToken);
            if (stateRIY == null)
            {
                stateRIY = new LogisticsAddressState { StateId = "RIY", Name = "Riyadh", CountryRegionId = "SA", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressState>().AddAsync(stateRIY, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 2.5. Counties
            var countyCA = await db.Set<LogisticsAddressCounty>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "US" && c.StateId == "CA" && c.CountyId == "", cancellationToken);
            if (countyCA == null)
            {
                countyCA = new LogisticsAddressCounty { CountryRegionId = "US", StateId = "CA", CountyId = "", Name = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCounty>().AddAsync(countyCA, cancellationToken);
            }
            var countyRIY = await db.Set<LogisticsAddressCounty>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryRegionId == "SA" && c.StateId == "RIY" && c.CountyId == "", cancellationToken);
            if (countyRIY == null)
            {
                countyRIY = new LogisticsAddressCounty { CountryRegionId = "SA", StateId = "RIY", CountyId = "", Name = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCounty>().AddAsync(countyRIY, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 3. Cities
            var cityLA = await db.Set<LogisticsAddressCity>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CityKey == "US-CA-LA", cancellationToken);
            if (cityLA == null)
            {
                cityLA = new LogisticsAddressCity { CityKey = "US-CA-LA", Name = "Los Angeles", Description = "Los Angeles City", CountryRegionId = "US", StateId = "CA", CountyId = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCity>().AddAsync(cityLA, cancellationToken);
            }
            var cityRuh = await db.Set<LogisticsAddressCity>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CityKey == "SA-RIY-RUH", cancellationToken);
            if (cityRuh == null)
            {
                cityRuh = new LogisticsAddressCity { CityKey = "SA-RIY-RUH", Name = "Riyadh", Description = "Riyadh City", CountryRegionId = "SA", StateId = "RIY", CountyId = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressCity>().AddAsync(cityRuh, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 3.5. Fallbacks and Auxiliaries (ZipCodes, Districts)
            var zipLA = await db.Set<LogisticsAddressZipCode>().IgnoreQueryFilters().FirstOrDefaultAsync(z => z.ZipCode == "90001", cancellationToken);
            if (zipLA == null)
            {
                zipLA = new LogisticsAddressZipCode { ZipCode = "90001", CountryRegionId = "US", State = "CA", County = "", City = "Los Angeles", CityAlias = "LA", DistrictName = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressZipCode>().AddAsync(zipLA, cancellationToken);
            }
            var zipRuh = await db.Set<LogisticsAddressZipCode>().IgnoreQueryFilters().FirstOrDefaultAsync(z => z.ZipCode == "12211", cancellationToken);
            if (zipRuh == null)
            {
                zipRuh = new LogisticsAddressZipCode { ZipCode = "12211", CountryRegionId = "SA", State = "RIY", County = "", City = "Riyadh", CityAlias = "RUH", DistrictName = "", CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressZipCode>().AddAsync(zipRuh, cancellationToken);
            }

            var districtFallback = await db.Set<LogisticsAddressDistrict>().IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Name == "", cancellationToken);
            if (districtFallback == null)
            {
                districtFallback = new LogisticsAddressDistrict { Name = "", Description = "Default Fallback", City = cityLA.RecId, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsAddressDistrict>().AddAsync(districtFallback, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 4. Locations
            var locDat = await db.Set<LogisticsLocation>().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LocationId == "DAT-LOC-1", cancellationToken);
            if (locDat == null)
            {
                locDat = new LogisticsLocation { LocationId = "DAT-LOC-1", Description = "DAT Headquarters", IsPostalAddress = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsLocation>().AddAsync(locDat, cancellationToken);
            }
            var locHbmc = await db.Set<LogisticsLocation>().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LocationId == "HBMC-LOC-1", cancellationToken);
            if (locHbmc == null)
            {
                locHbmc = new LogisticsLocation { LocationId = "HBMC-LOC-1", Description = "HBMC Headquarters", IsPostalAddress = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsLocation>().AddAsync(locHbmc, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 5. Postal Addresses
            var postalDat = await db.Set<LogisticsPostalAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Location == locDat.RecId, cancellationToken);
            if (postalDat == null)
            {
                postalDat = new LogisticsPostalAddress { Location = locDat.RecId, Address = "123 Main St\nLos Angeles, CA 90001\nUSA", Street = "Main St", StreetNumber = "123", City = "Los Angeles", CityRecId = cityLA.RecId, State = "CA", County = "", CountryRegionId = "US", ZipCode = "90001", ZipCodeRecId = zipLA.RecId, DistrictName = "", District = districtFallback.RecId, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(100), CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsPostalAddress>().AddAsync(postalDat, cancellationToken);
            }
            var postalHbmc = await db.Set<LogisticsPostalAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Location == locHbmc.RecId, cancellationToken);
            if (postalHbmc == null)
            {
                postalHbmc = new LogisticsPostalAddress { Location = locHbmc.RecId, Address = "King Fahd Rd\nRiyadh, RIY 12211\nSaudi Arabia", Street = "King Fahd Rd", StreetNumber = "1", City = "Riyadh", CityRecId = cityRuh.RecId, State = "RIY", County = "", CountryRegionId = "SA", ZipCode = "12211", ZipCodeRecId = zipRuh.RecId, DistrictName = "", District = districtFallback.RecId, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(100), CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsPostalAddress>().AddAsync(postalHbmc, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // 6. Electronic Addresses (Email, Phone)
            var emailDat = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "DAT-EML-1", cancellationToken);
            if (emailDat == null)
            {
                emailDat = new LogisticsElectronicAddress { ElectronicAddressId = "DAT-EML-1", Location = locDat.RecId, Description = "Primary Email", Type = ElectronicAddressType.Email, Locator = "info@dat.com", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(emailDat, cancellationToken);
            }
            var phoneDat = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "DAT-PHN-1", cancellationToken);
            if (phoneDat == null)
            {
                phoneDat = new LogisticsElectronicAddress { ElectronicAddressId = "DAT-PHN-1", Location = locDat.RecId, Description = "Primary Phone", Type = ElectronicAddressType.Phone, Locator = "+15551234567", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(phoneDat, cancellationToken);
            }
            var emailHbmc = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "HBMC-EML-1", cancellationToken);
            if (emailHbmc == null)
            {
                emailHbmc = new LogisticsElectronicAddress { ElectronicAddressId = "HBMC-EML-1", Location = locHbmc.RecId, Description = "Primary Email", Type = ElectronicAddressType.Email, Locator = "info@hbmc.com.sa", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(emailHbmc, cancellationToken);
            }
            var phoneHbmc = await db.Set<LogisticsElectronicAddress>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ElectronicAddressId == "HBMC-PHN-1", cancellationToken);
            if (phoneHbmc == null)
            {
                phoneHbmc = new LogisticsElectronicAddress { ElectronicAddressId = "HBMC-PHN-1", Location = locHbmc.RecId, Description = "Primary Phone", Type = ElectronicAddressType.Phone, Locator = "+966112345678", IsPrimary = NoYes.Yes, CreatedBy = createdBy, OwnerAccountId = createdBy, DataAreaId = "dat", IsActive = true };
                await db.Set<LogisticsElectronicAddress>().AddAsync(phoneHbmc, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
