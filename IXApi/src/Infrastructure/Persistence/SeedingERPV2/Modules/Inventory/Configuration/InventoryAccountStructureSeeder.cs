using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// The inventory journal account structure and its ledger dimensions. A third structure is used for
/// the same reason the sales one exists: an activated structure cannot be redefined, so the profit
/// and loss codes cannot be added to the purchase or sales ranges already assigned to this ledger.
/// </summary>
public sealed class InventoryAccountStructureSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventoryAccountStructureSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.MainAccountSeeder", "ERPV2.LedgerSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var builder = new AccountStructureBuilder(c.Db);
        var structure = await c.Rows<DimensionHierarchy>().SingleOrDefaultAsync(x => x.Name == "IV2", ct);
        if (structure == null)
        {
            var (defined, error) = await builder.DefineAsync(c.Company, new AccountStructureCreateDto("IV2", "Inventory example main accounts",
                [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment,
                    string.Join(';', GeneralLedgerSeedExample.InventoryJournalAccounts))]), ct);
            if (defined == null || error != null) throw new InvalidOperationException(error);
            var (activated, activationError) = await builder.ActivateAsync(c.Company, "IV2", ct);
            if (activated == null || activationError != null) throw new InvalidOperationException(activationError);
            structure = activated;
        }
        if (structure.IsDeleted || structure.IsDraft == NoYes.Yes) throw new InvalidOperationException("IV2 structure is unavailable.");
        await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var (_, assignmentError) = await builder.AssignToLedgerAsync(c.Company, structure.Name, ct);
        if (assignmentError != null) throw new InvalidOperationException(assignmentError);
        var chart = await c.RequireAsync<LedgerChartOfAccounts>(x => x.Name == "APV2", ct);
        var codes = GeneralLedgerSeedExample.InventoryJournalAccounts;
        foreach (var account in await c.Rows<MainAccount>().Where(x => x.LedgerChartOfAccounts == chart.RecId && codes.Contains(x.MainAccountId) && !x.IsDeleted).ToListAsync(ct))
        {
            var combination = await new LedgerDimensionResolver(c.Db).ResolveAsync(c.Company, account, [], structure.RecId, ct);
            if (combination.MainAccount != account.RecId || combination.AccountStructure != structure.RecId || combination.IsDeleted || !combination.IsActive)
                throw new InvalidOperationException("Inventory example combination conflicts.");
        }
    }
}
