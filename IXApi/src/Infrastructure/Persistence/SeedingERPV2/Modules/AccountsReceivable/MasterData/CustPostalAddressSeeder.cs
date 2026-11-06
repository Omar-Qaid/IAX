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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// Customer premises in the geography the purchase example already seeded. The country, state,
/// county, city, district and ZIP rows are reused rather than created again.
/// </summary>
public sealed class CustPostalAddressSeeder : IDataSeeder
{
    public string Id => "ERPV2.CustPostalAddressSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CustPartyLocationSeeder", "ERPV2.LogisticsPostalAddressSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var city = await c.RequireAsync<LogisticsAddressCity>(x => x.CityKey == "SA-RIY-RUH", ct);
        var district = await c.RequireAsync<LogisticsAddressDistrict>(x => x.Name == "Al Olaya", ct);
        var zip = await c.RequireAsync<LogisticsAddressZipCode>(x => x.ZipCode == "12211", ct);
        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var code = $"ARV2-{c.Company}-{suffix}";
            var location = await c.RequireAsync<LogisticsLocation>(x => x.LocationId == code, ct);
            await c.EnsureAsync<LogisticsPostalAddress>(x => x.Location == location.RecId,
                new() { Location = location.RecId, Address = $"{CustomerSeedExample.Building(suffix)} {CustomerSeedExample.Street(suffix)}, Al Olaya, Riyadh 12211, Saudi Arabia (demo premises)",
                    Street = CustomerSeedExample.Street(suffix), StreetNumber = CustomerSeedExample.Building(suffix),
                    ValidFrom = new(2000, 1, 1), ValidTo = new(2099, 12, 31), CountryRegionId = "SA", State = "RIY", County = "RUH",
                    City = city.Name, CityRecId = city.RecId, District = district.RecId, DistrictName = district.Name,
                    ZipCode = zip.ZipCode, ZipCodeRecId = zip.RecId }, ct);
            // PrimaryAddressLocation also references the postal address's Location alternate key.
            // Both parent rows must exist before updating this shortcut.
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == code, ct);
            if (party.PrimaryAddressLocation != null && party.PrimaryAddressLocation != location.RecId)
                throw new InvalidOperationException("Customer primary address conflicts.");
            party.PrimaryAddressLocation = location.RecId;
            await c.Db.SaveChangesAsync(ct);
        }
    }
}
