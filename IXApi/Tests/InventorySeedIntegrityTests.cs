using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Modules.Finance;
using Mapster;
using Microsoft.Data.SqlClient;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

[Collection("AccountsReceivable cycle stages")]
public sealed class InventorySeedIntegrityTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task Inventory_preparation_rolls_back_masters_after_late_validation_failure()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddInventoryConfigurationAndMasterDataExamples().BuildServiceProvider();
        var runner = new SeedRunner(db, provider.GetServices<IDataSeeder>(),
            provider.GetServices<ISeedValidator>().Append(new RejectAfterPreparation()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2026, "inventory-test"));
        Assert.Empty(await db.Set<InventTable>().ToListAsync());
        Assert.Empty(await db.Set<InventTableModule>().ToListAsync());
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
        Assert.Empty(await db.Set<SysNumberSequence>().ToListAsync());
    }

    private sealed class RejectAfterPreparation : ISeedValidator
    {
        public Task ValidateAsync(SeedContext context, CancellationToken ct) => throw new InvalidOperationException("Injected late validation failure.");
    }

    [Fact]
    public async Task Inventory_reference_package_posts_and_reruns_without_duplicate_movements()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        var actor = new Identity();
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddLogging().AddInventoryTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<InventoryTransactionExamples>();
        var report = await runner.RunAsync(new(2026, "inventory-test", ["01"]));
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(report));
        var scenario = Assert.Single(report.Scenarios);
        Assert.Equal("01", scenario.Id);
        Assert.Equal(90, scenario.PhysicalQuantity);
        Assert.Equal(1080, scenario.InventoryValue);
        Assert.Equal(720, scenario.CostOfGoodsSold);
        Assert.Equal(-120, scenario.CloseAdjustment);
        // The aggregate figures alone would pass even if an intermediate step posted the wrong
        // quantity and a later one compensated, so every stage pair is compared as well.
        Assert.NotEmpty(scenario.Stages);
        Assert.All(scenario.Stages, stage =>
        {
            Assert.Equal(stage.ExpectedQuantity, stage.ActualQuantity);
            Assert.Equal(stage.ExpectedValue, stage.ActualValue);
        });
        var movements = await db.Set<InventTrans>().CountAsync();
        var vouchers = await db.Set<GeneralJournalEntry>().CountAsync();
        db.ChangeTracker.Clear();
        var replay = await runner.RunAsync(new(2026, "inventory-test", ["01"]));
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(replay));
        Assert.Equal(scenario.InventoryValue, Assert.Single(replay.Scenarios).InventoryValue);
        Assert.Equal(movements, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(vouchers, await db.Set<GeneralJournalEntry>().CountAsync());
    }

    [Theory]
    [InlineData("02", 8, 80)]
    [InlineData("03", 88, 1056)]
    [InlineData("04", 93, 1116)]
    [InlineData("05", 90, 1080)]
    [InlineData("06", 90, 1080)]
    [InlineData("07", 90, 1080)]
    [InlineData("08", 8, 80)]
    [InlineData("09", 16, 240)]
    public async Task Inventory_journal_package_reconciles_profit_loss_and_preserves_posted_journals_on_replay(
        string scenarioId, decimal expectedQuantity, decimal expectedValue)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        var actor = new Identity();
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddLogging().AddInventoryTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<InventoryTransactionExamples>();
        var report = await runner.RunAsync(new(2026, "inventory-test", [scenarioId]));
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(report));
        var scenario = Assert.Single(report.Scenarios);
        Assert.Equal(expectedQuantity, scenario.PhysicalQuantity);
        Assert.Equal(expectedValue, scenario.InventoryValue);
        Assert.Equal(0m, scenario.CostOfGoodsSold);
        Assert.Equal(scenarioId == "09" ? 5 : scenarioId is "05" or "06" or "07" ? 1 : 2, scenario.PostedVouchers);
        Assert.All(scenario.Stages, stage => Assert.Equal(stage.ExpectedValue, stage.ActualValue));
        var movements = await db.Set<InventTrans>().CountAsync();
        var vouchers = await db.Set<GeneralJournalEntry>().CountAsync();
        var counter = await db.Set<SysNumberSequence>().Where(x => x.NumberSequence == InventJournalSeedExample.VoucherSequence)
            .Select(x => x.NextRec).SingleAsync();
        db.ChangeTracker.Clear();
        var replay = await runner.RunAsync(new(2026, "inventory-test", [scenarioId]));
        Assert.Equal(expectedValue, Assert.Single(replay.Scenarios).InventoryValue);
        Assert.Equal(movements, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(vouchers, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(counter, await db.Set<SysNumberSequence>().Where(x => x.NumberSequence == InventJournalSeedExample.VoucherSequence)
            .Select(x => x.NextRec).SingleAsync());
        if (scenarioId == "06") Assert.Equal(3, await db.Set<InventTransferJour>().CountAsync());

        // Reusing a document key must validate the persisted business facts, even when it was already posted.
        var openingQuantity = scenarioId is "02" or "08" or "09" ? 10m : 90m;
        var line = await db.Set<InventJournalTrans>().SingleAsync(x => x.Qty == openingQuantity && (scenarioId != "09" || x.CostPrice == 10m));
        line.Qty += 1;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(new(2026, "inventory-test", [scenarioId])));
        Assert.Contains("Existing adjustment line differs", error.Message);
        Assert.Equal(movements, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(vouchers, await db.Set<GeneralJournalEntry>().CountAsync());
    }

    [Fact]
    public async Task All_inventory_scenarios_coexist_and_replay_without_changing_balances_or_sequences()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        await VerifyInventoryReplayAsync(db, new Identity());
    }

    [SqlServerPhaseOneFact]
    public async Task All_inventory_scenarios_coexist_and_replay_on_SQLServer()
    {
        var actor = new Identity();
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = $"IXApi_InventoryReplay_{Guid.NewGuid():N}",
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection.ConnectionString, sql => sql.CommandTimeout(120)).Options;
        await using var db = new ApplicationDbContext(options, companyContext: actor);
        try
        {
            Assert.True(await db.Database.EnsureCreatedAsync());
            await VerifyInventoryReplayAsync(db, actor);
        }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private async Task VerifyInventoryReplayAsync(ApplicationDbContext db, Identity actor)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddLogging().AddInventoryTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<InventoryTransactionExamples>();
        var first = await runner.RunAsync(new(2026, "inventory-test"));
        Assert.Equal(9, first.Scenarios.Count);
        Assert.Equal(9, first.Scenarios.Select(x => x.ItemId).Distinct().Count());
        output.WriteLine(System.Text.Json.JsonSerializer.Serialize(first));
        var movements = await db.Set<InventTrans>().CountAsync();
        var vouchers = await db.Set<GeneralJournalEntry>().CountAsync();
        var settlements = await db.Set<InventSettlement>().CountAsync();
        var transferJournals = await db.Set<InventTransferJour>().CountAsync();
        var counters = await db.Set<SysNumberSequence>().AsNoTracking().OrderBy(x => x.NumberSequence)
            .Select(x => new { x.NumberSequence, x.NextRec }).ToListAsync();
        db.ChangeTracker.Clear();
        var replay = await runner.RunAsync(new(2026, "inventory-test"));
        Assert.Equal(first.Scenarios.Select(x => (x.Id, x.PhysicalQuantity, x.InventoryValue, x.CostOfGoodsSold, x.CloseAdjustment)),
            replay.Scenarios.Select(x => (x.Id, x.PhysicalQuantity, x.InventoryValue, x.CostOfGoodsSold, x.CloseAdjustment)));
        Assert.Equal(movements, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(vouchers, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(settlements, await db.Set<InventSettlement>().CountAsync());
        Assert.Equal(transferJournals, await db.Set<InventTransferJour>().CountAsync());
        Assert.Equal(counters, await db.Set<SysNumberSequence>().AsNoTracking().OrderBy(x => x.NumberSequence)
            .Select(x => new { x.NumberSequence, x.NextRec }).ToListAsync());
    }

    [SqlServerPhaseOneFact]
    public async Task RES02_competing_reservations_cannot_both_take_the_last_stock()
    {
        var actor = new Identity();
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = $"IXApi_InventoryReservation_{Guid.NewGuid():N}",
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection.ConnectionString, sql => sql.CommandTimeout(120)).Options;
        await using var db = new ApplicationDbContext(options, companyContext: actor);
        try
        {
            Assert.True(await db.Database.EnsureCreatedAsync());
            TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
            using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
                .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
                .AddLogging().AddInventoryTransactionExamples().BuildServiceProvider();
            using var scope = provider.CreateScope();
            var report = await scope.ServiceProvider.GetRequiredService<InventoryTransactionExamples>()
                .RunAsync(new(2026, "inventory-test", ["02"]));
            var item = Assert.Single(report.Scenarios).ItemId;
            var stock = await db.Set<InventSum>().SingleAsync(x => x.ItemId == item && x.PhysicalInvent > 0);
            Assert.Equal(8m, stock.PhysicalInvent);
            var origins = new[] { "RES02-A", "RES02-B" }.Select(id => new InventTransOrigin
            {
                DataAreaId = "dat", InventTransId = id, ItemId = item, ItemInventDimId = stock.InventDimId,
                ReferenceId = id, ReferenceCategory = InventRefType.SalesTable
            }).ToArray();
            db.AddRange(origins);
            await db.SaveChangesAsync();
            db.AddRange(origins.Select(origin => new InventTrans
            {
                DataAreaId = "dat", InventTransOrigin = origin.RecId, ItemId = item,
                InventDimId = stock.InventDimId, Qty = -8m, StatusIssue = StatusIssue.Ordered,
                ValueOpen = InventTransOpen.Yes
            }));
            stock.OnOrder = 16m;
            await db.SaveChangesAsync();
            var vouchers = await db.Set<GeneralJournalEntry>().CountAsync();

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var requests = origins.Select(origin => ReserveAsync(origin.InventTransId)).ToArray();
            start.SetResult();
            var results = await Task.WhenAll(requests);
            Assert.Equal(8m, Assert.Single(results, x => x.Error == null).Result!.Reserved);
            Assert.Equal("There is not enough available physical stock to reserve the requested quantity.",
                Assert.Single(results, x => x.Error != null).Error);

            db.ChangeTracker.Clear();
            var updated = await db.Set<InventSum>().SingleAsync(x => x.RecId == stock.RecId);
            Assert.Equal(8m, updated.PhysicalInvent);
            Assert.Equal(8m, updated.ReservPhysical);
            Assert.Equal(8m, updated.OnOrder);
            Assert.Equal(0m, updated.AvailPhysical);
            Assert.Equal(vouchers, await db.Set<GeneralJournalEntry>().CountAsync());
            var originIds = origins.Select(x => x.RecId).ToArray();
            var issues = await db.Set<InventTrans>().Where(x => originIds.Contains(x.InventTransOrigin)).ToListAsync();
            Assert.Equal(-8m, Assert.Single(issues, x => x.StatusIssue == StatusIssue.ReservPhysical).Qty);
            Assert.Equal(-8m, Assert.Single(issues, x => x.StatusIssue == StatusIssue.Ordered).Qty);

            async Task<(ReservationResultDto? Result, string? Error)> ReserveAsync(string demandId)
            {
                await using var session = new ApplicationDbContext(options, companyContext: actor);
                using var work = new UnitOfWork(session);
                await start.Task;
                return await new InventoryReservationService(work, actor)
                    .ReserveAsync(demandId, 8m, false, default);
            }
        }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private sealed class Identity : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "inventory-test";
        public string GetOwnerAccountId() => "inventory-test";
    }

    [Fact]
    public async Task Inventory_preparation_is_repeatable_preserves_counters_and_rejects_conflicts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddInventoryConfigurationAndMasterDataExamples().BuildServiceProvider();
        var runner = new SeedRunner(db, provider.GetServices<IDataSeeder>(), provider.GetServices<ISeedValidator>());
        var examples = new InventoryCycleSeeder(runner, provider.GetServices<ISeedValidator>());
        var first = await examples.PrepareAsync(new(2026, "inventory-test"));
        Assert.False(first.TransactionsExecuted);
        var stockItem = ExampleIdentifiers.StockItem(2026);
        Assert.NotEmpty(first.PreparationSteps);
        Assert.Equal(stockItem, Assert.Single(first.MasterData.Items, x => x.ItemId == stockItem && x.Stocked).ItemId);
        Assert.Equal(ExampleIdentifiers.ServiceItem(2026), Assert.Single(first.MasterData.Items, x => !x.Stocked).ItemId);
        Assert.Equal(InventoryCycleSeedExample.PostingProfiles.Count, first.MasterData.PostingProfiles.Count);
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
        var count = await db.Set<InventTable>().CountAsync();
        (await db.Set<SysNumberSequence>().SingleAsync(x => x.NumberSequence == "InventClosing")).NextRec = 29;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await examples.PrepareAsync(new(2026, "inventory-test"));
        Assert.Equal(count, await db.Set<InventTable>().CountAsync());
        Assert.Equal(29, (await db.Set<SysNumberSequence>().SingleAsync(x => x.NumberSequence == "InventClosing")).NextRec);
        var item = await db.Set<InventTable>().SingleAsync(x => x.ItemId == stockItem);
        item.Name = "User-owned description";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => examples.PrepareAsync(new(2026, "inventory-test")));
        Assert.Contains("Conflicting", error.Message);
        Assert.Equal("User-owned description", (await db.Set<InventTable>().SingleAsync(x => x.ItemId == stockItem)).Name);
    }

    [Fact]
    public async Task Inventory_preparation_seeds_the_journal_setup_the_readiness_check_requires()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddInventoryConfigurationAndMasterDataExamples().BuildServiceProvider();
        var runner = new SeedRunner(db, provider.GetServices<IDataSeeder>(), provider.GetServices<ISeedValidator>());
        await new InventoryCycleSeeder(runner, provider.GetServices<ISeedValidator>()).PrepareAsync(new(2026, "inventory-test"));

        // The journal id and voucher sequences must both exist and be allocatable: InventJournalService
        // draws the journal id, and InventJournalPostingService refuses to post without the voucher.
        foreach (var code in new[] { nameof(InventJournalTable), InventJournalSeedExample.VoucherSequence })
        {
            var sequence = await db.Set<SysNumberSequence>().SingleAsync(x => x.NumberSequence == code);
            Assert.True(sequence.InUse > 0 && sequence.Blocked != 1 && sequence.Manual != 1 && sequence.NoIncrement != 1,
                $"{code} is not allocatable.");
            Assert.Contains("{SEQ}", sequence.AnnotatedFormat);
        }

        // One journal name per type, each drawing the seeded voucher sequence, and each selected on
        // the parameter row. Readiness reports MISSING_<TYPE>_JOURNAL when any part of that is absent.
        var voucher = await db.Set<SysNumberSequence>().SingleAsync(x => x.NumberSequence == InventJournalSeedExample.VoucherSequence);
        var parameters = await db.Set<InventParameters>().SingleAsync();
        foreach (var (journalNameId, _, journalType) in InventJournalSeedExample.Names)
        {
            var name = await db.Set<InventJournalName>().SingleAsync(x => x.JournalNameId == journalNameId);
            Assert.Equal(journalType, name.JournalType);
            Assert.Equal(voucher.RecId, name.VoucherNumberSequenceTable);
        }
        Assert.Equal(InventJournalSeedExample.Name(InventJournalType.Movement), parameters.MovementJournalNameId);
        Assert.Equal(InventJournalSeedExample.Name(InventJournalType.Adjustment), parameters.LossProfitJournalNameId);
        Assert.Equal(InventJournalSeedExample.Name(InventJournalType.Transfer), parameters.TransferJournalNameId);
        Assert.Equal(InventJournalSeedExample.Name(InventJournalType.Counting), parameters.CountJournalNameId);

        // The four journal posting profiles, each resolved to a real ledger dimension.
        foreach (var type in new[] { InventAccountType.InventReceipt, InventAccountType.InventIssue,
            InventAccountType.InventProfit, InventAccountType.InventLoss })
        {
            var profile = await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == type
                && x.ItemRelation == InventoryCycleSeedExample.ItemGroup);
            Assert.NotEqual(0, profile.LedgerDimension);
        }
        Assert.Equal(2, await db.Set<MainAccount>().CountAsync(x => x.MainAccountId == GeneralLedgerSeedExample.InventoryProfit
            || x.MainAccountId == GeneralLedgerSeedExample.InventoryLoss));

        // Transfer orders need a Standard source, a Transit leg and a Standard destination.
        Assert.Equal(InventLocationType.Transit,
            (await db.Set<InventLocation>().SingleAsync(x => x.InventLocationId == "APV2-TRN")).InventLocationType);
        Assert.Equal(2, await db.Set<InventLocation>().CountAsync(x => x.InventLocationType == InventLocationType.Standard));
        Assert.NotNull(await db.Set<InventCountGroup>().SingleAsync(x => x.CountGroupId == InventJournalSeedExample.CountGroup));
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
