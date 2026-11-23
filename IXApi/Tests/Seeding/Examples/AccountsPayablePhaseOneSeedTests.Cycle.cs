using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Services;
using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

namespace IAX.IXApi.Tests;

public sealed partial class AccountsPayablePhaseOneSeedTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Cycle_examples_prepare_configuration_and_masters_only_and_return_persisted_references(bool existingCompany)
    {
        await using var database = await SqliteTestDatabase.CreateAsync(createSchema: false);
        using var provider = new ServiceCollection().AddScoped<ApplicationDbContext>(_ => database.CreateContext())
            .AddAccountsPayableConfigurationAndMasterDataExamples().AddAccountsPayableConfigurationAndMasterDataExamples()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        if (existingCompany) await InitializeCompanyAsync(db);
        Assert.Empty(scope.ServiceProvider.GetServices<ISeedScenario>());
        Assert.Equal(3, scope.ServiceProvider.GetServices<ISeedValidator>().Count());
        var examples = scope.ServiceProvider.GetRequiredService<AccountsPayableCycleSeeder>();
        var result = await examples.PrepareAsync(new(2026, "master-example-test"));
        Assert.NotNull(result.MasterData.AdditionalMasters);
        Assert.Equal((await db.Set<CompanyInfo>().SingleAsync()).RecId, result.MasterData.AdditionalMasters.CompanyRecId);
        Assert.Equal(existingCompany ? "Riyadh Business Services (Demo)" : "Office Supplies Demo Company", (await db.Set<CompanyInfo>().SingleAsync()).Name);
        Assert.Equal((await db.Set<PurchParameters>().SingleAsync()).RecId, result.MasterData.AdditionalMasters.PurchaseParametersRecId);
        Assert.True(result.MasterData.AdditionalMasters.StorageDimensionGroupRecId > 0);
        Assert.True(result.MasterData.AdditionalMasters.TrackingDimensionGroupRecId > 0);
        Assert.Equal((await db.Set<MarkupTable>().SingleAsync()).RecId, result.MasterData.AdditionalMasters.ChargeCodeRecId);
        Assert.Equal(ExampleIdentifiers.Scope, result.Scope);
        Assert.False(result.TransactionsExecuted);
        Assert.Equal("dat", result.Company);
        Assert.Equal(2026, result.FiscalYear);
        Assert.Equal(4, result.PreparationSteps.Count);
        Assert.Equal(Enumerable.Range(1, result.CompletedSeeders.Count), result.CompletedSeeders.Select(x => x.Order));
        Assert.Equal(scope.ServiceProvider.GetServices<IDataSeeder>().Select(x => x.Id).OrderBy(x => x),
            result.CompletedSeeders.Select(x => x.SeederId).OrderBy(x => x));
        Assert.All(result.MasterData.Vendors, x => Assert.True(x.RecId > 0 && x.PartyRecId > 0 && x.BankRecId > 0 && x.AddressRecId > 0));
        Assert.Equal(2, result.MasterData.Vendors.Count);
        Assert.Equal(2, result.MasterData.Products.Count);
        Assert.Equal(3, result.MasterData.PurchasePrices.Count);
        var local = result.MasterData.Vendors.Single(x => x.Currency == "SAR");
        Assert.Equal("APV2-dat-LOCAL", local.AccountNum);
        Assert.Equal(VendorSeedExample.Name("LOCAL"), local.Name);
        Assert.Equal(25m, result.MasterData.PurchasePrices.Single(x => x.VendorAccount == local.AccountNum && x.ItemId == ExampleIdentifiers.StockItem).Amount);
        Assert.Equal(350m, result.MasterData.PurchasePrices.Single(x => x.VendorAccount == local.AccountNum && x.ItemId == ExampleIdentifiers.ServiceItem).Amount);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result));
        Assert.False(json.RootElement.GetProperty(nameof(result.TransactionsExecuted)).GetBoolean());
        var counts = await CountsAsync(db);
        var tables = counts.Split(';').Select(x => x.Split(':')).ToDictionary(x => x[0], x => long.Parse(x[1]));
        foreach (var table in new[] { "PurchTable", "PurchLine", "VendPurchOrderJour", "VendPurchOrderTrans", "VendPackingSlipJour", "VendPackingSlipTrans",
            "SourceDocumentHeader", "SourceDocumentLine", "AccountingEvent", "SubledgerJournalAccountEntryDistribution", "AccountingDistribution", "SubledgerJournalEntry", "SubledgerJournalAccountEntry", "VendInvoiceInfoSubLine", "InventTransOrigin", "InventTrans", "VendInvoiceInfoTable", "VendInvoiceInfoLine", "VendInvoiceJour", "VendInvoiceTrans", "VendTrans", "VendTransOpen",
            "VendSettlement", "LedgerJournalTable", "LedgerJournalTrans", "BankAccountTrans", "TaxTrans", "MarkupTrans", "GeneralJournalEntry", "GeneralJournalAccountEntry", "SubledgerVoucherGeneralJournalEntry" })
            Assert.Equal(0, tables[table]);
        db.ChangeTracker.Clear();
        var repeated = await examples.PrepareAsync(new(2026, "master-example-test"));
        Assert.Equal(counts, await CountsAsync(db));
        Assert.Equal(result.MasterData.AdditionalMasters, repeated.MasterData.AdditionalMasters);
        Assert.Equal(result.MasterData.Vendors.Select(x => x.RecId), repeated.MasterData.Vendors.Select(x => x.RecId));
        Assert.Equal(result.MasterData.Products.Select(x => x.RecId), repeated.MasterData.Products.Select(x => x.RecId));
        Assert.Equal(result.MasterData.PurchasePrices.Select(x => x.RecId), repeated.MasterData.PurchasePrices.Select(x => x.RecId));
    }

    [Fact]
    public async Task Transaction_storage_round_trips_and_rejects_orphan_references()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        await new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>())
            .RunAsync(2026, "transaction-storage-test");
        await AssertTransactionStorageAsync(db);
    }

    [Fact]
    public async Task Phase_two_stock_service_cycle_runs_through_business_services_and_is_repeatable()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = TransactionServices(db);
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<AccountsPayableTransactionExamples>();
        var result = await examples.RunAsync(new(2026, "cycle-test"));
        Assert.Equal(21, result.Scenarios.Count);
        Assert.All(result.Scenarios, x => Assert.True(x.PostedVouchers > 0));
        var complete = result.Scenarios.Single(x => x.Id == "01");
        Assert.Equal(100m, complete.PhysicalQuantity);
        Assert.Equal(-3680m, complete.BankMovement);
        Assert.Equal(480m, complete.TaxAmount);
        Assert.Equal(0m, complete.OpenCredit);
        Assert.Equal(0m, result.Scenarios.Single(x => x.Id == "03").PhysicalQuantity);
        Assert.True(complete.SourceDistributions > 0);
        Assert.Equal(-2932.50m, result.Scenarios.Single(x => x.Id == "07").BankMovement);
        Assert.Equal(382.50m, result.Scenarios.Single(x => x.Id == "07").TaxAmount);
        Assert.Equal(-2817.50m, result.Scenarios.Single(x => x.Id == "12").BankMovement);
        Assert.Equal(-805m, result.Scenarios.Single(x => x.Id == "13").BankMovement);
        Assert.Equal(393.75m, result.Scenarios.Single(x => x.Id == "13").TaxAmount);
        Assert.Equal(90m, result.Scenarios.Single(x => x.Id == "14").PhysicalQuantity);
        Assert.Equal(-287.50m, result.Scenarios.Single(x => x.Id == "15").BankMovement);
        var before = await CountsAsync(db);
        await examples.RunAsync(new(2026, "cycle-test"));
        Assert.Equal(before, await CountsAsync(db));
    }

    [SqlServerFact]
    public async Task SQL_Server_transaction_examples_run_twice_without_duplicate_rows()
    {
        var database = "IXApi_APV2_Cycle_" + Guid.NewGuid().ToString("N");
        var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(TestEnvironment.SqlServerConnectionString(database));
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await db.Database.EnsureCreatedAsync();
            using var provider = TransactionServices(db);
            using var scope = provider.CreateScope();
            var examples = scope.ServiceProvider.GetRequiredService<AccountsPayableTransactionExamples>();
            var first = await examples.RunAsync(new(2026, "sql-cycle-test"));
            Assert.Equal(21, first.Scenarios.Count);
            var counts = await CountsAsync(db);
            await examples.RunAsync(new(2026, "sql-cycle-test"));
            Assert.Equal(counts, await CountsAsync(db));
        }
        finally
        {
            Assert.StartsWith("IXApi_APV2_Cycle_", database);
            // UnitOfWork owns/disposes its DI context; cleanup must use a separate instance.
            await using var cleanup = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public void Reconciliation_refuses_a_main_account_the_demo_chart_does_not_define()
    {
        var posted = new AccountsPayableReconciliationSnapshot
        {
            ScenarioId = "01", ExpectedPhysicalQuantity = 0m, ExpectedOpenDebit = 0m, ExpectedOpenCredit = 0m,
            PurchaseOrders = [], VendorTransactions = [], Open = [], SettlementRows = [], Inventory = [],
            Accruals = [], Tax = [], Bank = [], Entries = [], PostedVouchers = [], SourceDistributions = 0,
            DimensionsByAccount = new Dictionary<string, long[]>
            {
                [GeneralLedgerSeedExample.Bank] = [7L],
                // Seeded in the chart but with no dimension combination in this company.
                [GeneralLedgerSeedExample.VendorControl] = []
            }
        };
        Assert.Equal(0m, posted.LedgerAmount(GeneralLedgerSeedExample.Bank));
        // A renumbered chart must fail the checks, not satisfy the ones that expect a cleared account.
        var unknown = Assert.Throws<InvalidOperationException>(() => posted.LedgerAmount("999999"));
        Assert.Contains("not in the seeded demo chart", unknown.Message);
        var unmapped = Assert.Throws<InvalidOperationException>(() => posted.LedgerAmount(GeneralLedgerSeedExample.VendorControl));
        Assert.Contains("has no ledger dimension", unmapped.Message);
    }

    [Fact]
    public void Each_extracted_cycle_check_refuses_the_defect_it_owns()
    {
        // Balanced, consistent baseline: one SAR 100 invoice, half settled, with a balanced voucher.
        var invoice = new VendTrans { RecId = 41L, Invoice = "APV2-I1", AmountCur = -100m, AmountMst = -100m, SettleAmountCur = -40m };
        AccountsPayableReconciliationSnapshot Posted(decimal expectedCredit, decimal openAmount, decimal reportingAmount) =>
            new()
            {
                ScenarioId = "01", ExpectedPhysicalQuantity = 0m, ExpectedOpenDebit = 0m, ExpectedOpenCredit = expectedCredit,
                PurchaseOrders = [], VendorTransactions = [invoice],
                Open = [new VendTransOpen { RefRecId = invoice.RecId, AmountCur = openAmount }],
                SettlementRows = [], Inventory = [], Accruals = [], Tax = [], Bank = [],
                Entries =
                [
                    new GeneralJournalAccountEntry { GeneralJournalEntry = 1L, AccountingCurrencyAmount = 60m, ReportingCurrencyAmount = 60m },
                    new GeneralJournalAccountEntry { GeneralJournalEntry = 1L, AccountingCurrencyAmount = -60m, ReportingCurrencyAmount = reportingAmount }
                ],
                PostedVouchers = [1L], SourceDistributions = 0,
                DimensionsByAccount = GeneralLedgerSeedExample.MainAccounts.ToDictionary(x => x.Code, _ => new[] { 7L })
            };

        // The invoice is SAR 60 open, which is what the baseline states everywhere.
        new OpenTransactionsValidator().Verify(Posted(60m, -60m, -60m));
        new VendorBalanceValidator().Verify(Posted(60m, -60m, -60m));
        new LedgerBalanceValidator().Verify(Posted(60m, -60m, -60m));

        Assert.Contains("open balances differ",
            Assert.Throws<InvalidOperationException>(() => new OpenTransactionsValidator().Verify(Posted(75m, -60m, -60m))).Message);
        Assert.Contains("unsettled remainder is -60",
            Assert.Throws<InvalidOperationException>(() => new VendorBalanceValidator().Verify(Posted(55m, -55m, -60m))).Message);
        Assert.Contains("voucher does not balance",
            Assert.Throws<InvalidOperationException>(() => new LedgerBalanceValidator().Verify(Posted(60m, -60m, -59m))).Message);
    }

    [Fact]
    public void Every_seeded_main_account_is_reachable_through_its_named_constant()
    {
        Assert.Equal(GeneralLedgerSeedExample.MainAccounts.Count,
            GeneralLedgerSeedExample.MainAccounts.Select(x => x.Code).Distinct().Count());
        Assert.Equal(10, AccountsPayableReconciliationSnapshot.Checks.Count);
        Assert.Equal(AccountsPayableReconciliationSnapshot.Checks.Count,
            AccountsPayableReconciliationSnapshot.Checks.Select(x => x.GetType()).Distinct().Count());
    }

    /// <summary>
    /// Two whole cycles posting the same documents at the same moment. The single-threaded rerun
    /// test shows business keys make a repeat a no-op; this shows the same holds when the repeat is
    /// concurrent rather than sequential, which is the realistic failure - two users, or a retried
    /// request arriving twice, posting the same invoice at once. Each runner gets its own context
    /// and connection, so they contend for real.
    /// <para>
    /// Measured outcome: <b>both runners complete</b> and every table count is identical to the
    /// single run, so the seeder lock and the business keys together make a concurrent repeat a
    /// no-op rather than a second set of documents. The assertion compares a count of every mapped
    /// table, so one duplicated row anywhere fails it.
    /// </para>
    /// </summary>
    [SqlServerFact]
    public async Task Two_concurrent_runs_of_the_same_cycle_do_not_duplicate_any_document()
    {
        var database = "IXApi_APV2_Concurrent_" + Guid.NewGuid().ToString("N");
        var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(TestEnvironment.SqlServerConnectionString(database));
        // Each stage gets its own context: TransactionServices registers the one it is given as a
        // singleton, and the provider disposes it, so a shared instance would be dead afterwards.
        ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await using (var schema = Context()) await schema.Database.EnsureCreatedAsync();

            // Establish the full cycle once, so the concurrent pair below are both repeats.
            async Task RunAsync()
            {
                using var provider = TransactionServices(Context());
                using var scope = provider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<AccountsPayableTransactionExamples>()
                    .RunAsync(new(2026, "concurrent-test"));
            }
            await RunAsync();
            string settled;
            await using (var baseline = Context()) settled = await CountsAsync(baseline);

            var runners = new[] { RunAsync(), RunAsync() };
            var outcomes = await Task.WhenAll(runners.Select(async r =>
            {
                try { await r; return null; }
                catch (Exception ex) { return (ex.InnerException ?? ex).Message.ReplaceLineEndings(" "); }
            }));

            // Both are expected to complete: the lock serializes them and the keys make the
            // second a no-op. The database must be what the single run left behind.
            await using (var after = Context()) Assert.Equal(settled, await CountsAsync(after));
            Assert.All(outcomes, x => Assert.Null(x));
        }
        finally
        {
            Assert.StartsWith("IXApi_APV2_Concurrent_", database);
            SqlConnection.ClearAllPools();
            await using var cleanup = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connection.ConnectionString).Options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
