using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>
/// Location roles, the country, and each company's headquarters: its geography (region, governorate, city,
/// district, postal code), location, postal address, e-mail and phone. Real geography with illustrative building
/// numbers. All rows live in the baseline company area, as the address book reads them.
/// </summary>
public sealed class BaselineLogisticsSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Foundation.Logistics";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = FoundationBaselineCatalog.Load();
        foreach (var role in catalog.LocationRoles)
            await c.AddMissingAsync<LogisticsLocationRole>(x => x.Name == role.Name, new()
            {
                Name = role.Name, IsPostalAddress = NoYes.Yes, IsContactInfo = role.IsContactInfo ? NoYes.Yes : NoYes.No
            }, ct);
        var country = catalog.Country;
        await c.AddMissingAsync<LogisticsAddressCountryRegion>(x => x.CountryRegionId == country.CountryRegionId,
            new() { CountryRegionId = country.CountryRegionId, IsoCode = country.IsoCode, AddrFormat = country.AddrFormat }, ct);

        foreach (var hq in catalog.Headquarters)
            await HeadquartersAsync(c, country.CountryRegionId, hq, ct);
    }

    private static async Task HeadquartersAsync(SeedContext c, string countryId, FoundationBaselineCatalog.HeadquartersRow hq, CancellationToken ct)
    {
        await c.AddMissingAsync<LogisticsAddressState>(x => x.CountryRegionId == countryId && x.StateId == hq.Region,
            new() { CountryRegionId = countryId, StateId = hq.Region, Name = hq.RegionName }, ct);
        await c.AddMissingAsync<LogisticsAddressCounty>(x => x.CountryRegionId == countryId && x.StateId == hq.Region && x.CountyId == hq.County,
            new() { CountryRegionId = countryId, StateId = hq.Region, CountyId = hq.County, Name = hq.City }, ct);
        var city = await c.AddMissingAsync<LogisticsAddressCity>(x => x.CityKey == hq.CityKey, new()
        {
            CityKey = hq.CityKey, Name = hq.City, Description = hq.City + " City", CountryRegionId = countryId, StateId = hq.Region, CountyId = hq.County
        }, ct);
        var district = await c.AddMissingAsync<LogisticsAddressDistrict>(x => x.Name == hq.District,
            new() { Name = hq.District, Description = hq.District + " district", City = city.RecId }, ct);
        var zip = await c.AddMissingAsync<LogisticsAddressZipCode>(x => x.ZipCode == hq.Zip, new()
        {
            ZipCode = hq.Zip, CountryRegionId = countryId, State = hq.Region, County = hq.County, City = hq.City, CityAlias = hq.County,
            CityRecId = city.RecId, District = district.RecId, DistrictName = district.Name
        }, ct);

        var location = await c.AddMissingAsync<LogisticsLocation>(x => x.LocationId == hq.Company + "-LOC-1",
            new() { LocationId = hq.Company + "-LOC-1", Description = hq.Company + " Headquarters", IsPostalAddress = NoYes.Yes }, ct);
        await c.AddMissingAsync<LogisticsPostalAddress>(x => x.Location == location.RecId, new()
        {
            Location = location.RecId, ValidFrom = new DateTime(2026, 1, 1), ValidTo = new DateTime(2154, 12, 31),
            CountryRegionId = countryId, State = hq.Region, County = hq.County, City = city.Name, CityRecId = city.RecId,
            District = district.RecId, DistrictName = district.Name, ZipCode = zip.ZipCode, ZipCodeRecId = zip.RecId,
            Street = hq.Street, StreetNumber = hq.Building,
            // The entity has no unit or additional-number fields, so both are kept in the building complement.
            BuildingCompliment = "Unit 1; Additional number " + hq.Additional,
            Address = $"{hq.Building} {hq.Street}, Unit 1\n{district.Name}\n{city.Name} {zip.ZipCode} - {hq.Additional}\n{hq.RegionName} Region, Saudi Arabia"
        }, ct);

        await c.AddMissingAsync<LogisticsElectronicAddress>(x => x.ElectronicAddressId == hq.Company + "-EML-1", new()
        {
            ElectronicAddressId = hq.Company + "-EML-1", Location = location.RecId, Description = "Primary Email",
            Type = ElectronicAddressType.Email, Locator = hq.Email, IsPrimary = NoYes.Yes
        }, ct);
        await c.AddMissingAsync<LogisticsElectronicAddress>(x => x.ElectronicAddressId == hq.Company + "-PHN-1", new()
        {
            ElectronicAddressId = hq.Company + "-PHN-1", Location = location.RecId, Description = "Primary Phone",
            Type = ElectronicAddressType.Phone, Locator = hq.Phone, IsPrimary = NoYes.Yes
        }, ct);
    }
}
