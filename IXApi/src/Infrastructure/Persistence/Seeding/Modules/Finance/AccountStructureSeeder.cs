using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>
/// The two account structures the core general ledger cycle defines (section 4, step 4): a balance
/// sheet structure over accounts 100000..399999 with Department optional, and a profit and loss
/// structure over 400000..899999 with Department mandatory and limited to the three lab values.
/// Their main account ranges do not overlap, which is what lets the main account value select the
/// structure.
///
/// Both are defined and activated. LedgerExampleSeeder assigns them only when creating its own
/// example ledger; example revenue/expense combinations include Department ADMIN. Existing ledger
/// assignments and user-maintained structures are preserved.
/// </summary>
public sealed class AccountStructureSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.Foundation.AccountStructures";
    public override int Version => 1;
    public override SeederCategory Category => SeederCategory.Reference;
    public override string Scope => "dat";
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.FinancialDimensions" };

    private const string Area = "dat";

    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedDataAsync(context.Db, ct);

    public async Task SeedDataAsync(DbContext db, CancellationToken ct = default)
    {
        var builder = new AccountStructureBuilder(db);
        await DefineAndActivateAsync(db, builder, new AccountStructureCreateDto(
            "BS", "Balance sheet accounts",
            [
                new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "100000..399999"),
                // All values and blank, so a balance sheet account needs no department.
                new AccountStructureSegmentDto("Department", "*;\"\"")
            ]), ct);

        await DefineAndActivateAsync(db, builder, new AccountStructureCreateDto(
            "PL", "Profit and loss accounts",
            [
                new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "400000..899999"),
                // Mandatory and limited to the three values: no "" criterion, so blank is refused.
                new AccountStructureSegmentDto("Department", "ADMIN;SALES;SERVICE")
            ]), ct);
    }

    private static async Task DefineAndActivateAsync(DbContext db, AccountStructureBuilder builder,
        AccountStructureCreateDto input, CancellationToken ct)
    {
        var existing = await db.Set<DimensionHierarchy>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DataAreaId == Area && x.Name == input.Name, ct);
        if (existing != null)
        {
            if (existing.IsDeleted || existing.IsDraft == IAX.IXApi.Modules.Finance.Common.NoYes.Yes)
                throw new InvalidOperationException($"Existing account structure '{input.Name}' is deleted or a draft; review it before seeding.");
            return;
        }
        if (existing == null)
        {
            var (structure, error) = await builder.DefineAsync(Area, input, ct);
            if (structure == null)
                throw new InvalidOperationException($"Account structure '{input.Name}' could not be seeded: {error}");
        }
        var (activated, activationError) = await builder.ActivateAsync(Area, input.Name, ct);
        if (activationError != null || activated == null)
            throw new InvalidOperationException(
                $"Account structure '{input.Name}' could not be activated: {activationError}");
    }
}
