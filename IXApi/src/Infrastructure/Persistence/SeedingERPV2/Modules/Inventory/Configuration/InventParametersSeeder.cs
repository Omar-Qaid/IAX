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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class InventParametersSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventParametersSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.UnitOfMeasureSeeder", "ERPV2.InventLocationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        await c.EnsureAsync<InventParameters>(x => x.DataAreaId == c.Company, new() { DefaultUnitId = "EA", MultiSiteDefaultInventSiteId = "APV2-SITE", MultiSiteDefaultInventLocationId = "APV2-WH", MandatoryItemTaxGroup = NoYes.Yes }, ct);
    }
}
