using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace IAX.IXApi.Tests;
[Collection(SeedingCollection.Name)]
public sealed class TaxSeedTests
{
    [Fact]
    public void Registration_and_manifest_are_consistent()
    {
        using var provider = new ServiceCollection().AddTaxConfigurationAndMasterDataExamples().AddTaxConfigurationAndMasterDataExamples().BuildServiceProvider();
        var seeders = provider.GetServices<IDataSeeder>().ToArray();
        Assert.Equal(seeders.Length, new SeedDependencyResolver().Resolve(seeders).Count);
        Assert.Equal(seeders.Length, seeders.Select(x => x.Id).Distinct().Count());
        var catalog = new TaxExampleCatalog();
        catalog.Validate();
        catalog.Tables.Transactions[0] = "UnknownTax";
        Assert.Throws<InvalidOperationException>(catalog.Validate);
    }

    [Fact]
    public async Task Preparation_reports_all_tax_tables_preserves_sequences_and_rolls_back_late_rate_conflicts()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db).AddTaxConfigurationAndMasterDataExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<TaxCycleSeeder>();
        var first = await examples.PrepareAsync(new TaxExampleOptions(2026, "tax-test"));
        Assert.False(first.TransactionsExecuted);
        Assert.Equal(11, first.MasterData!.Tables.Count);
        Assert.Equal(10, first.MasterData.References.Count);
        Assert.Equal(first.CompletedSeeders, first.SeederSteps.Select(x => x.SeederId));
        Assert.Empty(await db.Set<TaxTrans>().ToListAsync());
        Assert.Empty(await db.Set<TaxJournalTrans>().ToListAsync());
        Assert.Empty(await db.Set<TaxTransGeneralJournalAccountEntry>().ToListAsync());
        var mapped = db.Model.GetEntityTypes().Where(x => x.GetTableName() is string name && name.StartsWith("Tax", StringComparison.Ordinal)).Select(x => x.ClrType.Name).ToHashSet();
        Assert.True(mapped.SetEquals(TaxTableCatalog.MasterTypes.Concat(TaxTableCatalog.TransactionTypes)));
        var sequence = await db.Set<SysNumberSequence>().OrderBy(x => x.RecId).FirstAsync();
        sequence.NextRec += 3;
        var sequenceId = sequence.RecId;
        var next = sequence.NextRec;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var second = await examples.PrepareAsync(new TaxExampleOptions(2026, "tax-test"));
        Assert.Equal(first.MasterData.References, second.MasterData!.References);
        Assert.Equal(first.MasterData.Tables.SelectMany(x => x.RecordIds), second.MasterData.Tables.SelectMany(x => x.RecordIds));
        Assert.Equal(next, (await db.Set<SysNumberSequence>().SingleAsync(x => x.RecId == sequenceId)).NextRec);
        db.ChangeTracker.Clear();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var runner = new SeedRunner(db, seeders.Append(new OverlappingRate(seeders.Select(x => x.Id).ToArray())), scope.ServiceProvider.GetServices<ISeedValidator>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2027, "tax-test"));
        Assert.False(await db.Set<TaxData>().AnyAsync(x => x.TaxFromDate.Year == 2027));
        Assert.False(await db.Set<TaxReportPeriod>().AnyAsync(x => x.FromDate.Year == 2027));
    }

    private sealed class OverlappingRate(string[] dependencies) : IDataSeeder
    {
        public string Id => "ERPV2.Tax.TestOverlap";
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            c.Db.Add(new TaxData { DataAreaId = c.Company, TaxCode = "APV2-15", TaxValue = 15m, TaxFromDate = new(c.FiscalYear, 6, 1), TaxToDate = new(c.FiscalYear, 6, 30) });
            await c.Db.SaveChangesAsync(ct);
        }
    }

    [Fact]
    public async Task Input_and_output_examples_post_and_rerun_without_duplicate_tax()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var actor = new TestIdentity("foundation-test");
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<IAX.IXApi.Shared.Application.Identity.ICompanyExecutionContext>(actor)
            .AddSingleton<IAX.IXApi.Infrastructure.Identity.ICurrentUserService>(actor)
            .AddTaxTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<TaxTransactionExamples>();
        var first = await examples.RunAsync(new(2026, "tax-test"));
        Assert.Equal(11, first.Preparation.MasterData!.Tables.Count);
        Assert.Single(first.InputTax.TaxJournalDetails);
        Assert.Single(first.InputTax.TaxAccountingAssociationIds);
        var output = Assert.Single(first.OutputTax.Scenarios);
        Assert.Equal(-52.50m, output.TaxAmount); // Output tax is a credit, as in AR scenario 04.
        Assert.True(output.PostedVouchers > 0);
        var ids = await db.Set<TaxTrans>().OrderBy(x => x.RecId).Select(x => x.RecId).ToArrayAsync();
        db.ChangeTracker.Clear();
        var second = await examples.RunAsync(new(2026, "tax-test"));
        Assert.Equal(ids, await db.Set<TaxTrans>().OrderBy(x => x.RecId).Select(x => x.RecId).ToArrayAsync());
        Assert.Equal(first.InputTax.TaxAccountingAssociationIds, second.InputTax.TaxAccountingAssociationIds);
    }
    [Fact]
    public void Catalog_exemption_codes_fit_the_mapped_exempt_code_length()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        var max = db.Model.FindEntityType(typeof(TaxExemptCodeTable))!.FindProperty(nameof(TaxExemptCodeTable.ExemptCode))!.GetMaxLength()!.Value;
        Assert.All(new[] { TaxSeedCatalog.ExemptReason, TaxSeedCatalog.NoExemption }, code => Assert.InRange(code.Length, 1, max));
        int Max<T>(string field) => db.Model.FindEntityType(typeof(T))!.FindProperty(field)!.GetMaxLength() ?? int.MaxValue;
        Assert.All(new[] { TaxSeedCatalog.StandardItemGroup, TaxSeedCatalog.ZeroItemGroup, TaxSeedCatalog.ExemptItemGroup,
                TaxSeedCatalog.OutOfScopeItemGroup, TaxSeedCatalog.InventoryZeroItemGroup },
            code => Assert.InRange(code.Length, 1, Max<TaxItemGroupHeading>(nameof(TaxItemGroupHeading.TaxItemGroup))));
        Assert.All(new[] { TaxSeedCatalog.StandardPurchase, TaxSeedCatalog.StandardSales, TaxSeedCatalog.ZeroRated, TaxSeedCatalog.Exempt },
            code => Assert.InRange(code.Length, 1, Max<TaxTable>(nameof(TaxTable.TaxCode))));
    }

}
