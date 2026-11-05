using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace IXApi.Tests;
[Collection("AccountsPayable phase one")]
public sealed class FoundationSeedTests
{
    [Fact]
    public void Registration_is_idempotent_and_dependencies_resolve()
    {
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var ordered = new SeedDependencyResolver().Resolve(seeders);
        Assert.Equal(seeders.Length, ordered.Count);
        Assert.Equal(seeders.Length, seeders.Select(x => x.Id).Distinct().Count());
        Assert.Contains(ordered, x => x.Id == "ERPV2.Foundation.HcmWorkerSeeder");
        Assert.Contains(scope.ServiceProvider.GetServices<ISeedValidator>(), x => x is FoundationIntegrityValidator);
    }
    [Fact]
    public async Task All_master_tables_are_populated_and_reruns_preserve_keys_and_conflicts_roll_back()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>());
        await runner.RunAsync(2026, "foundation-test");
        var mapped = db.Model.GetEntityTypes().Where(x => FoundationTableCatalog.MasterTypes.Contains(x.ClrType.Name)).ToArray();
        Assert.Equal(FoundationTableCatalog.MasterTypes.Count, mapped.Length);
        var counts = new Dictionary<string,long>();
        foreach (var type in mapped)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM \"" + type.GetTableName() + "\"";
            var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            Assert.True(count > 0, type.ClrType.Name);
            counts[type.ClrType.Name] = count;
        }
        db.ChangeTracker.Clear();
        await runner.RunAsync(2026, "foundation-test");
        foreach (var type in mapped)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM \"" + type.GetTableName() + "\"";
            Assert.Equal(counts[type.ClrType.Name], Convert.ToInt64(await cmd.ExecuteScalarAsync()));
        }
        var delivery = await db.Set<DlvMode>().SingleAsync(x => x.Code == "FNV2");
        delivery.Txt = "Operator change";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2027, "foundation-test"));
        Assert.False(await db.Set<TaxReportPeriod>().AnyAsync(x => x.FromDate.Year == 2027));
    }
    [Fact]
    public async Task Late_cross_company_reference_is_rejected_and_rolled_back()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var validators = scope.ServiceProvider.GetServices<ISeedValidator>().ToArray();
        var runner = new SeedRunner(db, seeders, validators);
        await runner.RunAsync(2026, "foundation-test");
        var original = await db.Set<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER");
        var originalUnit = original.OrganizationUnitId;
        db.ChangeTracker.Clear();
        var corruptingRunner = new SeedRunner(db, seeders.Append(new CrossCompanyReference(seeders.Select(x => x.Id).ToArray())), validators);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => corruptingRunner.RunAsync(2026, "foundation-test"));
        Assert.Contains("crosses company", error.Message);
        Assert.Equal(originalUnit, (await db.Set<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER")).OrganizationUnitId);
        Assert.False(await db.Set<IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit>().IgnoreQueryFilters().AnyAsync(x => x.Code == "FNV2-OTHER"));
    }
    private sealed class CrossCompanyReference(string[] dependencies) : IDataSeeder
    {
        public string Id => "ERPV2.Foundation.TestCorruption";
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            var foreign = new IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit { DataAreaId = "oth", Code = "FNV2-OTHER", Name = "Other company" };
            c.Db.Add(foreign);
            await c.Db.SaveChangesAsync(ct);
            var position = await c.Rows<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER", ct);
            position.OrganizationUnitId = foreign.RecId;
            await c.Db.SaveChangesAsync(ct);
        }
    }
    [Fact]
    public async Task Tax_and_charge_examples_use_business_services_and_are_repeatable()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        var actor = new ExampleIdentity();
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<IAX.IXApi.Shared.Application.Identity.ICompanyExecutionContext>(actor)
            .AddSingleton<IAX.IXApi.Infrastructure.Identity.ICurrentUserService>(actor)
            .AddFoundationTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.FoundationTransactionExamples>();
        var result = await examples.RunAsync(new(2026, "foundation-test"));
        Assert.Equal("FoundationTransactions", result.Scope);
        Assert.Single(result.TaxJournalDetails);
        Assert.Equal(2, result.Scenarios.Count);
        Assert.All(result.Scenarios, x => Assert.True(x.PostedVouchers > 0));
        var taxes = await db.Set<TaxTrans>().CountAsync();
        var charges = await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans>().CountAsync();
        Assert.True(taxes > 0);
        Assert.True(charges > 0);
        var journalTaxes = await db.Set<TaxJournalTrans>().ToListAsync();
        var detail = Assert.Single(journalTaxes);
        Assert.Equal("dat", detail.DataAreaId);
        Assert.Equal(IAX.IXApi.Modules.Finance.Common.FinanceTableNumbers.LedgerJournalTrans, detail.TransTableId);
        var source = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == detail.TransRecId && x.DataAreaId == detail.DataAreaId);
        var posted = await db.Set<TaxTrans>().SingleAsync(x => x.SourceRecId == source.RecId && x.SourceTableId == detail.TransTableId && x.TaxCode == detail.TaxCode);
        Assert.Equal(source.TaxGroup, detail.TaxGroup);
        Assert.Equal(source.TaxItemGroup, detail.TaxItemGroup);
        Assert.Equal(posted.SourceBaseAmountCur, detail.SourceBaseAmountCur);
        Assert.Equal(posted.SourceTaxAmountCur, detail.SourceTaxAmountCur);
        Assert.Equal(posted.TaxBaseAmount, detail.TaxBaseAmount);
        Assert.Equal(posted.TaxAmount, detail.TaxAmount);
        Assert.Equal(52.50m, detail.TaxAmount);
        Assert.True(await db.Set<DimensionAttributeValueCombination>().AnyAsync(x => x.DataAreaId == detail.DataAreaId && x.RecId == detail.OperationLedgerDimension));
        db.ChangeTracker.Clear();
        await examples.RunAsync(new(2026, "foundation-test"));
        Assert.Equal(journalTaxes.Count, await db.Set<TaxJournalTrans>().CountAsync());
        Assert.Equal(detail.RecId, await db.Set<TaxJournalTrans>().Select(x => x.RecId).SingleAsync());
        Assert.Equal(taxes, await db.Set<TaxTrans>().CountAsync());
        Assert.Equal(charges, await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans>().CountAsync());
    }
    private sealed class ExampleIdentity : IAX.IXApi.Shared.Application.Identity.ICompanyExecutionContext, IAX.IXApi.Infrastructure.Identity.ICurrentUserService
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "foundation-test";
        public string GetOwnerAccountId() => "foundation-test";
    }
    [Fact]
    public void Foundation_manifest_rejects_coverage_drift()
    {
        var catalog = new IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data.FoundationExampleCatalog();
        catalog.Validate();
        catalog.Tables.Masters[0] = "UnknownTable";
        Assert.Throws<InvalidOperationException>(() => catalog.Validate());
        var referenceCatalog = new IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data.FoundationExampleCatalog();
        referenceCatalog.References[0] = referenceCatalog.References[0] with { Value = "WrongFixture" };
        Assert.Throws<InvalidOperationException>(() => referenceCatalog.Validate());
    }
    [Fact]
    public async Task Foundation_preparation_reports_persisted_references_and_preserves_them_on_rerun()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db).AddFoundationConfigurationAndMasterDataExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<FoundationCycleSeeder>();
        var first = await examples.PrepareAsync(new(2026,"foundation-test"));
        Assert.False(first.TransactionsExecuted);
        Assert.NotNull(first.MasterData);
        Assert.All(await db.Set<TaxReportPeriod>().ToListAsync(), interval => Assert.Equal(0, interval.VersionNum));
        Assert.Equal(4, first.PreparationSteps.Count);
        Assert.Equal(68, first.MasterData.Tables.Count);
        Assert.Equal(13, first.MasterData.References.Count);
        Assert.All(first.MasterData.Tables, table => Assert.Equal(table.RowCount, table.RecordIds.Count));
        var currency = first.MasterData.References.Single(x => x.Entity == "Currency");
        Assert.Equal((await db.Set<Currency>().SingleAsync(x => x.CurrencyCode == "SAR")).RecId.ToString(), currency.RecordId);
        Assert.Empty(await db.Set<TaxJournalTrans>().ToListAsync());
        db.ChangeTracker.Clear();
        var second = await examples.PrepareAsync(new(2026,"foundation-test"));
        Assert.Equal(first.MasterData.References, second.MasterData!.References);
    }
    private sealed class Fixture(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                property.SetColumnType(type == typeof(byte[]) ? "BLOB" : type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) ? "TEXT" : "INTEGER");
                property.SetDefaultValueSql(null);
                if (property.Name == "RowVersion") property.SetDefaultValueSql("X'00'");
            }
        }
    }
}
