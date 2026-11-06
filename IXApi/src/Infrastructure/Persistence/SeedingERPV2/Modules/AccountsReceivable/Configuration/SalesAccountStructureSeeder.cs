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
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// The sales account structure and its ledger dimensions. A second structure is used rather than a
/// wider purchase one because an activated structure cannot be redefined, and its main-account
/// ranges must not overlap the purchase structure already assigned to this ledger.
/// </summary>
public sealed class SalesAccountStructureSeeder : IDataSeeder
{
    public string Id => "ERPV2.SalesAccountStructureSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.MainAccountSeeder", "ERPV2.LedgerSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var builder = new AccountStructureBuilder(c.Db);
        var structure = await c.Rows<DimensionHierarchy>().SingleOrDefaultAsync(x => x.Name == "ARV2", ct);
        if (structure == null)
        {
            var (defined, error) = await builder.DefineAsync(c.Company, new AccountStructureCreateDto("ARV2", "AR example main accounts",
                [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "181006..181007;281003..281005;481003;681004..681005")]), ct);
            if (defined == null || error != null) throw new InvalidOperationException(error);
            var (activated, activationError) = await builder.ActivateAsync(c.Company, "ARV2", ct);
            if (activated == null || activationError != null) throw new InvalidOperationException(activationError);
            structure = activated;
        }
        if (structure.IsDeleted || structure.IsDraft == NoYes.Yes) throw new InvalidOperationException("ARV2 structure is unavailable.");
        await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var (_, assignmentError) = await builder.AssignToLedgerAsync(c.Company, structure.Name, ct);
        if (assignmentError != null) throw new InvalidOperationException(assignmentError);
        var chart = await c.RequireAsync<LedgerChartOfAccounts>(x => x.Name == "APV2", ct);
        var codes = GeneralLedgerSeedExample.AccountsReceivableAccounts;
        foreach (var account in await c.Rows<MainAccount>().Where(x => x.LedgerChartOfAccounts == chart.RecId && codes.Contains(x.MainAccountId) && !x.IsDeleted).ToListAsync(ct))
        {
            var combination = await new LedgerDimensionResolver(c.Db).ResolveAsync(c.Company, account, [], structure.RecId, ct);
            if (combination.MainAccount != account.RecId || combination.AccountStructure != structure.RecId || combination.IsDeleted || !combination.IsActive)
                throw new InvalidOperationException("AR example combination conflicts.");
        }
    }
}
