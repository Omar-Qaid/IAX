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

        // The journal selections belong on this row rather than being filled in later by
        // InventJournalNameSeeder: EnsureAsync validates every field it owns on each rerun, so a
        // selection written by another seeder would be read back here as conflicting data.
        // InventParametersService.GetReadiness requires all four to be selected.
        await c.EnsureAsync<InventParameters>(x => x.DataAreaId == c.Company, new()
        {
            DefaultUnitId = "EA",
            MultiSiteDefaultInventSiteId = "APV2-SITE",
            MultiSiteDefaultInventLocationId = "APV2-WH",
            MovementJournalNameId = InventJournalSeedExample.Name(InventJournalType.Movement),
            LossProfitJournalNameId = InventJournalSeedExample.Name(InventJournalType.Adjustment),
            TransferJournalNameId = InventJournalSeedExample.Name(InventJournalType.Transfer),
            CountJournalNameId = InventJournalSeedExample.Name(InventJournalType.Counting),
            MandatoryItemTaxGroup = NoYes.Yes
        }, ct);
    }
}
