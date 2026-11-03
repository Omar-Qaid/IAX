using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed partial class FinanceOperationalExamplesSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.OperationalExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.GeneralLedger.Examples", "Finance.Inventory.DemoProducts", "Finance.Inventory.Setup", "Administration.NumberSequences" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        await SeedInventoryVoucherSequenceAsync(db, ct);
        var dimensions = await SeedLedgerDimensionsAsync(db, ct);
        await SeedBankAndSubledgerSetupAsync(db, dimensions, ct);
        await SeedReturnSetupAsync(db, ct);
        await SeedReleasedProductSetupAsync(db, ct);
    }
}
