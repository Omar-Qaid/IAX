using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>The rules individual baseline seeders and the startup runner keep: what a rerun may and may not change.</summary>
public sealed class BaselineSeederBehaviorTests
{
    [Fact]
    public async Task Ledger_has_an_accounting_currency_and_every_period_open()
    {
        await using var f = await Db.CreateAsync();
        await f.SeedAsync(new BaselineCompanySeeder(), new BaselineCurrencySeeder(), new BaselineFiscalCalendarSeeder(), new BaselineLedgerSeeder());
        var ledger = await f.Context.Set<Ledger>().SingleAsync();
        Assert.Equal("SAR", ledger.AccountingCurrency);
        Assert.NotEqual(0, ledger.FiscalCalendar);
        Assert.NotEqual(0, ledger.PrimaryForLegalEntity);
        var statuses = await f.Context.Set<LedgerFiscalCalendarPeriod>().ToListAsync();
        Assert.Equal(await f.Context.Set<FiscalCalendarPeriod>().CountAsync(), statuses.Count);
        Assert.All(statuses, x => Assert.Equal(FiscalPeriodStatus.Open, x.Status));
    }

    [Fact]
    public async Task Ledger_rerun_keeps_a_closed_period_closed_and_fills_only_a_missing_currency()
    {
        await using var f = await Db.CreateAsync();
        await f.SeedAsync(new BaselineCompanySeeder(), new BaselineCurrencySeeder(), new BaselineFiscalCalendarSeeder());
        f.Context.Add(new Ledger { Name = "Existing", Description = "Operator ledger", DataAreaId = "dat" });
        await f.Context.SaveChangesAsync();
        await f.SeedAsync(new BaselineLedgerSeeder());
        var closed = await f.Context.Set<LedgerFiscalCalendarPeriod>().FirstAsync();
        closed.Status = FiscalPeriodStatus.Closed;
        await f.Context.SaveChangesAsync();

        await f.SeedAsync(new BaselineLedgerSeeder());
        var ledger = await f.Context.Set<Ledger>().SingleAsync();
        Assert.Equal("Existing", ledger.Name);
        Assert.Equal("SAR", ledger.AccountingCurrency);
        Assert.Equal(FiscalPeriodStatus.Closed, (await f.Context.Set<LedgerFiscalCalendarPeriod>().SingleAsync(x => x.RecId == closed.RecId)).Status);
    }

    [Fact]
    public async Task Calendar_rerun_keeps_customized_period_dates_and_restores_only_a_missing_period()
    {
        await using var f = await Db.CreateAsync();
        f.Context.Add(new FiscalCalendar { CalendarId = "HBMC", Description = "Another company's calendar", DataAreaId = "oth" });
        await f.Context.SaveChangesAsync();
        await f.SeedAsync(new BaselineCompanySeeder(), new BaselineFiscalCalendarSeeder());
        var period = await f.Context.Set<FiscalCalendarPeriod>().SingleAsync(x => x.Name == "Period 1");
        period.EndDate = period.EndDate.AddDays(-1);
        var customizedEnd = period.EndDate;
        f.Context.Remove(await f.Context.Set<FiscalCalendarPeriod>().SingleAsync(x => x.Name == "Period 2"));
        await f.Context.SaveChangesAsync();

        await f.SeedAsync(new BaselineFiscalCalendarSeeder());
        await f.SeedAsync(new BaselineFiscalCalendarSeeder());
        Assert.Equal(14, await f.Context.Set<FiscalCalendarPeriod>().CountAsync());
        Assert.Equal(2, await f.Context.Set<FiscalCalendar>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(customizedEnd, (await f.Context.Set<FiscalCalendarPeriod>().SingleAsync(x => x.Name == "Period 1")).EndDate);
    }

    [Fact]
    public async Task An_existing_postal_code_for_other_geography_is_kept_and_never_linked()
    {
        await using var f = await Db.CreateAsync();
        await f.SeedAsync(new BaselineCompanySeeder());
        f.Context.Add(new LogisticsAddressZipCode { ZipCode = "23431", CountryRegionId = "US", State = "NY", City = "Albany", DataAreaId = "dat" });
        await f.Context.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => f.SeedAsync(new BaselineLogisticsSeeder()));
        Assert.Contains("existing postal codes were preserved", error.Message);
        Assert.Equal("Albany", (await f.Context.Set<LogisticsAddressZipCode>().SingleAsync(x => x.ZipCode == "23431")).City);
        Assert.Empty(await f.Context.Set<LogisticsPostalAddress>().ToListAsync());
    }

    [Fact]
    public async Task Organization_structure_is_repeatable_and_assigns_only_named_unassigned_workers()
    {
        await using var f = await Db.CreateAsync();
        await f.SeedAsync(new BaselineCompanySeeder());
        f.Context.Add(new IAX.IXApi.Modules.Finance.Foundation.Genders.Gender { RecId = 1, Code = "GEN1", Name = "Male", DataAreaId = "dat" });
        f.Context.Add(new IAX.IXApi.Modules.Finance.Foundation.Nationalities.HcmNationality { RecId = 1, Code = "NAT1", Name = "Saudi", DataAreaId = "dat" });
        await f.Context.SaveChangesAsync();
        foreach (var (id, number) in new[] { (1L, "DEMO-DIRECTOR"), (2L, "DEMO-SALES-MGR") })
        {
            var party = new DirPartyTable { PartyNumber = number, Name = number, DataAreaId = "dat" };
            f.Context.Add(party);
            await f.Context.SaveChangesAsync();
            f.Context.Add(new HcmWorker { RecId = id, PersonnelNumber = number, Person = party.RecId, GenderId = 1, NationalityId = 1, DataAreaId = "dat" });
        }
        await f.Context.SaveChangesAsync();

        await f.SeedAsync(new BaselineOrganizationStructureSeeder());
        await f.SeedAsync(new BaselineOrganizationStructureSeeder());
        Assert.Equal(13, await f.Context.Set<OrganizationUnit>().CountAsync());
        Assert.Equal(12, await f.Context.Set<OrganizationRole>().CountAsync());
        Assert.Equal(19, await f.Context.Set<OrganizationHierarchyNode>().CountAsync());
        Assert.Equal(14, await f.Context.Set<HcmPosition>().CountAsync());
        Assert.Equal(7, await f.Context.Set<HcmPositionReportingLine>().CountAsync());
        Assert.Equal(2, await f.Context.Set<HcmWorkerOrganizationAssignment>().CountAsync());
        Assert.DoesNotContain(await f.Context.Set<OrganizationUnit>().ToListAsync(), x => string.IsNullOrWhiteSpace(x.NameAlias));
    }

    [Fact]
    public async Task Organization_structure_rerun_preserves_customized_closed_and_deleted_records()
    {
        await using var f = await Db.CreateAsync();
        await f.SeedAsync(new BaselineCompanySeeder(), new BaselineOrganizationStructureSeeder());
        var position = await f.Context.Set<HcmPosition>().SingleAsync(x => x.Code == "SH-A-SELLER-01");
        position.Name = "Customized seat";
        var role = await f.Context.Set<OrganizationRole>().SingleAsync(x => x.Code == "SELLER");
        role.IsDeleted = true;
        var shop = await f.Context.Set<OrganizationUnit>().SingleAsync(x => x.Code == "SH-A");
        var node = await f.Context.Set<OrganizationHierarchyNode>().FirstAsync(x => x.OrganizationUnitId == shop.RecId);
        node.ValidTo = new DateOnly(2026, 9, 16);
        await f.Context.SaveChangesAsync();

        await f.SeedAsync(new BaselineOrganizationStructureSeeder());
        Assert.Equal(14, await f.Context.Set<HcmPosition>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(19, await f.Context.Set<OrganizationHierarchyNode>().IgnoreQueryFilters().CountAsync());
        Assert.Equal("Customized seat", (await f.Context.Set<HcmPosition>().IgnoreQueryFilters().SingleAsync(x => x.RecId == position.RecId)).Name);
        Assert.True((await f.Context.Set<OrganizationRole>().IgnoreQueryFilters().SingleAsync(x => x.RecId == role.RecId)).IsDeleted);
        Assert.Equal(new DateOnly(2026, 9, 16), (await f.Context.Set<OrganizationHierarchyNode>().IgnoreQueryFilters().SingleAsync(x => x.RecId == node.RecId)).ValidTo);
    }

    [Fact]
    public async Task Runner_skips_an_import_already_recorded_by_the_previous_seeding()
    {
        await using var f = await Db.CreateAsync();
        // Rows written before SeederId was recorded carry only the key in TableName.
        f.Context.Add(new SysDataSeedLog { TableName = "Test.Import", Outcome = "Success", Policy = "Legacy" });
        await f.Context.SaveChangesAsync();
        var import = new Probe("Test.Import", BaselineSeederKind.LegacyImport);
        var result = await f.RunAsync(new SeedingOptions { EnableLegacyImports = true }, import, new Probe("Test.Repeatable"));
        Assert.Equal(["Test.Repeatable"], result.CompletedSeeders);
        Assert.Equal(0, import.Runs);
    }

    [Fact]
    public async Task Runner_rolls_back_a_failed_seeder_records_the_failure_and_stops()
    {
        await using var f = await Db.CreateAsync();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            f.RunAsync(new SeedingOptions(), new Probe("Test.Fails", fail: true), new Probe("Test.After", dependencies: ["Test.Fails"])));
        Assert.Contains("Test.Fails", error.Message);
        Assert.Empty(await f.Context.Set<UnitOfMeasure>().Where(x => x.Symbol == "PROBE").ToListAsync());
        var log = await f.Context.Set<SysDataSeedLog>().SingleAsync();
        Assert.Equal(("Test.Fails", "Failed"), (log.SeederId, log.Outcome));
    }

    [Fact]
    public async Task Runner_refuses_production_and_imports_are_off_unless_enabled()
    {
        await using var f = await Db.CreateAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new BaselineSeedRunner(f.Context, [new Probe("Test.Any")], Options.Create(new SeedingOptions()),
                NullLogger<BaselineSeedRunner>.Instance, new Production()).RunAsync());
        var import = new Probe("Test.Import", BaselineSeederKind.LegacyImport);
        Assert.Empty((await f.RunAsync(new SeedingOptions(), import)).CompletedSeeders);
        Assert.Equal(0, import.Runs);
    }

    [Fact]
    public void Baseline_graph_resolves_keeps_the_previous_seeder_ids_and_rejects_missing_or_cyclic_dependencies()
    {
        var configuration = new ConfigurationBuilder().Build();
        using var provider = new ServiceCollection().AddLogging().AddSingleton<IConfiguration>(configuration)
            .AddBaselineSeeding(configuration).BuildServiceProvider();
        var seeders = provider.CreateScope().ServiceProvider.GetServices<IBaselineSeeder>().ToArray();
        Assert.Equal(seeders.Length, seeders.Select(x => x.Id).Distinct().Count());
        Assert.Equal(seeders.Length, new SeedDependencyResolver().Resolve(seeders).Count);
        // Every seeder the previous startup seeding recorded history for still exists under the same id.
        Assert.Subset(seeders.Select(x => x.Id).ToHashSet(), new HashSet<string>
        {
            "Administration.NumberSequences", "Administration.Settings", "Finance.AccountsReceivable.CustGroups", "Finance.AccountsReceivable.CustLedger",
            "Finance.AccountsReceivable.DeliverySetup", "Finance.AccountsReceivable.Markup", "Finance.AccountsReceivable.PaymModes",
            "Finance.AccountsReceivable.PriceDisc", "Finance.AccountsReceivable.SalesPools", "Finance.Foundation.AccountStructures",
            "Finance.Foundation.CompanyInfo", "Finance.Foundation.FinancialDimensions", "Finance.Foundation.LegacyEmployees", "Finance.Foundation.Logistics",
            "Finance.Foundation.Structure", "Finance.GeneralLedger.Currencies", "Finance.GeneralLedger.FiscalCalendars", "Finance.GeneralLedger.Ledger",
            "Finance.GeneralLedger.MainAccounts", "Finance.GeneralLedger.Tax", "Finance.Inventory.ItemGroups", "Finance.Inventory.Setup",
            "Finance.Inventory.SitesAndWarehouses", "Finance.Inventory.UnitOfMeasure", "Identity.MasterData", "Organization.DocumentManagement",
            "Workflow.LegacyMaster", "Workflow.ProcessMaster"
        });
        Assert.Throws<InvalidOperationException>(() => new SeedDependencyResolver().Resolve([new Probe("A", dependencies: ["Missing"])]));
        Assert.Throws<InvalidOperationException>(() => new SeedDependencyResolver().Resolve([new Probe("A", dependencies: ["B"]), new Probe("B", dependencies: ["A"])]));
    }

    private sealed class Probe(string id, BaselineSeederKind kind = BaselineSeederKind.Repeatable, bool fail = false, string[]? dependencies = null) : IBaselineSeeder
    {
        public int Runs { get; private set; }
        public string Id => id;
        public IReadOnlyCollection<string> Dependencies => dependencies ?? [];
        public BaselineSeederKind Kind => kind;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            Runs++;
            await c.AddMissingAsync<UnitOfMeasure>(x => x.Symbol == "PROBE", new() { Symbol = "PROBE" }, ct);
            if (fail) throw new InvalidOperationException("probe failure");
        }
    }

    private sealed class Production : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "IXApi";
        public string ContentRootPath { get; set; } = "";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class Db : IAsyncDisposable
    {
        private SqliteTestDatabase database = null!;
        public ApplicationDbContext Context => database.Context;

        public static async Task<Db> CreateAsync() => new() { database = await SqliteTestDatabase.CreateAsync() };

        public async Task SeedAsync(params IDataSeeder[] seeders)
        {
            var context = new SeedContext(Context, 2026, "sys", "dat");
            foreach (var seeder in seeders) await seeder.SeedAsync(context, default);
            Context.ChangeTracker.Clear();
        }

        public Task<SeedResult> RunAsync(SeedingOptions options, params IBaselineSeeder[] seeders) =>
            new BaselineSeedRunner(Context, seeders, Options.Create(options), NullLogger<BaselineSeedRunner>.Instance).RunAsync();

        public ValueTask DisposeAsync() => database.DisposeAsync();
    }

}
