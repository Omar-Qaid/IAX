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
public sealed class CustElectronicAddressSeeder : IDataSeeder
{
    public string Id => "ERPV2.CustElectronicAddressSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CustPartyLocationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var code = $"ARV2-{c.Company}-{suffix}";
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == code, ct);
            var location = await c.RequireAsync<LogisticsLocation>(x => x.LocationId == code, ct);
            var email = await c.EnsureAsync<LogisticsElectronicAddress>(x => x.ElectronicAddressId == code,
                new() { ElectronicAddressId = code, Location = location.RecId, Type = ElectronicAddressType.Email,
                    Locator = CustomerSeedExample.Email(suffix), Description = "Customer accounts receivable contact (demo)", IsPrimary = NoYes.Yes }, ct);
            if (party.PrimaryContactEmail != null && party.PrimaryContactEmail != email.RecId) throw new InvalidOperationException("Customer primary email conflicts.");
            party.PrimaryContactEmail = email.RecId;
            await c.Db.SaveChangesAsync(ct);
        }
    }
}
