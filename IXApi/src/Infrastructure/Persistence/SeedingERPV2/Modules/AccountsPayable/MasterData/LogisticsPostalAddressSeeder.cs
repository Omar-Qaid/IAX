using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public sealed class LogisticsPostalAddressSeeder : IDataSeeder
{
    public string Id => "ERPV2.LogisticsPostalAddressSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DirPartyLocationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        await c.EnsureAsync<LogisticsAddressCountryRegion>(x => x.CountryRegionId == "SA", new() { CountryRegionId = "SA", IsoCode = "SA", AddrFormat = "SAU" }, ct, nameof(LogisticsAddressCountryRegion.IsoCode));
        await c.EnsureAsync<LogisticsAddressState>(x => x.StateId == "RIY" && x.CountryRegionId == "SA", new() { CountryRegionId = "SA", StateId = "RIY", Name = "Riyadh" }, ct);
        await c.EnsureAsync<LogisticsAddressCounty>(x => x.CountyId == "RUH" && x.StateId == "RIY" && x.CountryRegionId == "SA", new() { CountyId = "RUH", StateId = "RIY", CountryRegionId = "SA", Name = "Riyadh" }, ct);
        var city = await c.EnsureAsync<LogisticsAddressCity>(x => x.CityKey == "APV2-RUH", new() { CityKey = "APV2-RUH", Name = "APV2 Riyadh", Description = "Synthetic example locality", CountryRegionId = "SA", StateId = "RIY", CountyId = "RUH" }, ct);
        var district = await c.EnsureAsync<LogisticsAddressDistrict>(x => x.Name == "APV2 District", new() { Name = "APV2 District", Description = "Synthetic", City = city.RecId }, ct);
        var zip = await c.EnsureAsync<LogisticsAddressZipCode>(x => x.ZipCode == "APV2", new() { ZipCode = "APV2", CountryRegionId = "SA", State = "RIY", County = "RUH", City = city.Name, CityRecId = city.RecId, District = district.RecId, DistrictName = district.Name }, ct);
        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var code = $"APV2-{c.Company}-{suffix}";
            var location = await c.RequireAsync<LogisticsLocation>(x => x.LocationId == code, ct);
            await c.EnsureAsync<LogisticsPostalAddress>(x => x.Location == location.RecId,
                new() { Location = location.RecId, Address = "Synthetic AP example address", Street = "Example street", StreetNumber = "1",
                    ValidFrom = new(2000, 1, 1), ValidTo = new(2099, 12, 31), CountryRegionId = "SA", State = "RIY", County = "RUH",
                    City = city.Name, CityRecId = city.RecId, District = district.RecId, DistrictName = district.Name, ZipCode = zip.ZipCode, ZipCodeRecId = zip.RecId }, ct);
        }
    }
}
