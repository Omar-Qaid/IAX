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
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

namespace IXApi.Tests;

[CollectionDefinition("AccountsPayable phase one", DisableParallelization = true)]
public sealed class AccountsPayablePhaseOneSeedCollection { }

[Collection("AccountsPayable phase one")]
public sealed class AccountsPayablePhaseOneSeedTests
{
    [Fact]
    public void Registration_is_complete_idempotent_and_has_no_transaction_seeders()
    {
        var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().AddAccountsPayablePhaseOneSeeding();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var ordered = new SeedDependencyResolver().Resolve(scope.ServiceProvider.GetServices<IDataSeeder>());
        Assert.Equal(58, ordered.Count);
        Assert.Equal(2, scope.ServiceProvider.GetServices<ISeedValidator>().Count());
        Assert.DoesNotContain(ordered, x => x.Id.Contains("Cycle") || x.Id.Contains("PurchTable"));
        foreach (var seeder in ordered)
            foreach (var dependency in seeder.Dependencies)
                Assert.True(ordered.ToList().FindIndex(x => x.Id == dependency) < ordered.ToList().IndexOf(seeder));
    }

    [Fact]
    public void Dependency_errors_are_reported_before_execution()
    {
        Assert.Throws<InvalidOperationException>(() => new SeedDependencyResolver().Resolve([new Stub("A", ["B"])]));
        Assert.Throws<InvalidOperationException>(() => new SeedDependencyResolver().Resolve([new Stub("A", ["B"]), new Stub("B", ["A"])]));
    }

    [Fact]
    public async Task Phase_one_runs_twice_without_duplicates_and_preserves_sequence_progress()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        // Preserve production constraints while normalizing SQL Server Unicode literals for SQLite.
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        var companyParty = new DirPartyTable { PartyNumber = "APV2-TEST-LEGAL", Name = "Test legal entity", IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes };
        db.Add(companyParty);
        await db.SaveChangesAsync();
        db.Set<CompanyInfo>().Add(new() { DataArea = "dat", Name = "AP phase-one test", DataAreaId = "dat", Party = companyParty.RecId });
        db.Set<LogisticsLocation>().Add(new() { DataAreaId = "oth", LocationId = "OTHER-LOCATION", Description = "Other company - preserve" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        using var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = services.CreateScope();
        var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>());
        var first = await runner.RunAsync(2026, "phase-one-test");
        Assert.Equal(58, first.CompletedSeeders.Count);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SeedContext(db, 2026, "phase-one-test")
            .RequireAsync<LogisticsLocation>(x => x.LocationId == "OTHER-LOCATION", default));
        Assert.Equal(2, await db.Set<VendTable>().CountAsync());
        Assert.Equal(2, await db.Set<VendBankAccount>().CountAsync());
        var vendor = await db.Set<VendTable>().SingleAsync(x => x.Currency == "SAR");
        var party = await db.Set<DirPartyTable>().SingleAsync(x => x.RecId == vendor.Party);
        Assert.NotNull(party.PrimaryAddressLocation);
        Assert.NotNull(party.PrimaryContactEmail);
        var item = await db.Set<InventTable>().SingleAsync(x => x.ItemId == "APV2-STOCK");
        Assert.True(item.Product > 0);
        Assert.Equal(3, await db.Set<InventTableModule>().CountAsync(x => x.ItemId == item.ItemId));
        Assert.Single(await db.Set<InventModelGroupItem>().Where(x => x.ItemId == item.ItemId).ToListAsync());
        await AssertBusinessExampleAsync(db);
        var counts = await CountsAsync(db);
        var tableCounts = counts.Split(';').Select(x => x.Split(':')).ToDictionary(x => x[0], x => long.Parse(x[1]));
        var requiredTables = "VendParameters VendGroup VendLedgerAccounts PaymTerm VendPaymModeTable CashDisc LedgerJournalName TaxTable TaxGroupHeading TaxGroupData TaxItemGroupHeading TaxOnItem Ledger Currency ExchangeRateType ExchangeRateCurrencyPair ExchangeRate MainAccount DimensionAttributeValueCombination InventPosting InventModelGroup InventParameters VendTable DirPartyTable DirPartyLocation LogisticsLocation LogisticsPostalAddress LogisticsElectronicAddress VendBankAccount BankAccountTable EcoResProduct InventTable InventTableModule InventSite InventLocation InventDim EcoResCategory PriceDiscTable CompanyInfo EcoResStorageDimensionGroup EcoResStorageDimensionGroupFldSetup EcoResStorageDimensionGroupItem EcoResTrackingDimensionGroup EcoResTrackingDimensionGroupFldSetup EcoResTrackingDimensionGroupItem InventItemPurchSetup InventItemInventSetup PurchParameters MarkupTable".Split(' ');
        foreach (var table in requiredTables)
            Assert.True(tableCounts.GetValueOrDefault(table) > 0, $"Phase-one table {table} was not populated.");
        foreach (var table in new[] { "PurchTable", "VendTrans", "InventTrans", "GeneralJournalEntry", "BankAccountTrans" })
            Assert.Equal(0, tableCounts[table]);
        var sequence = await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().FirstAsync();
        var formattedOne = IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequenceService.FormatCode(sequence, 1);
        var formattedTwo = IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequenceService.FormatCode(sequence, 2);
        Assert.NotEqual(formattedOne, formattedTwo);
        Assert.DoesNotContain("#", formattedOne);
        sequence.NextRec = 12;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await runner.RunAsync(2026, "phase-one-test");
        Assert.Equal(counts, await CountsAsync(db));
        Assert.Equal(12, (await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().SingleAsync(x => x.RecId == sequence.RecId)).NextRec);

        var group = await db.Set<VendGroup>().SingleAsync(x => x.VendGroupCode == "APV2");
        group.Name = "Customized by operator";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        // The next year creates calendar/rate rows before the vendor conflict; all must roll back.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2027, "phase-one-test"));
        Assert.Contains("Conflicting VendGroup.Name", exception.Message);
        Assert.Equal("Customized by operator", (await db.Set<VendGroup>().SingleAsync()).Name);
        Assert.Equal(counts, await CountsAsync(db));
    }

    [Theory]
    [InlineData("price")]
    [InlineData("company")]
    [InlineData("storage")]
    [InlineData("charge")]
    public async Task Commit_validator_rejects_semantically_invalid_references_and_rolls_back(string corruption)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        await InitializeCompanyAsync(db);
        var before = await CountsAsync(db);
        using var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = services.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var runner = new SeedRunner(db, seeders.Append(new CorruptExampleSeeder(seeders.Select(x => x.Id).ToArray(), corruption)),
            scope.ServiceProvider.GetServices<ISeedValidator>());
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2026, "integrity-test"));
        Assert.Contains(corruption switch { "price" => "vendor price currency/unit/amount", "storage" => "storage field policy", "charge" => "charge posting references", _ => "DimensionAttributeValueCombination reference in dat" }, exception.Message);
        Assert.Equal(before, await CountsAsync(db));
    }

    [Fact]
    public async Task Mapped_string_lengths_are_checked_before_insert_even_on_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        var max = db.Model.FindEntityType(typeof(LogisticsLocation))!.FindProperty(nameof(LogisticsLocation.LocationId))!.GetMaxLength()!.Value;
        var context = new SeedContext(db, 2026, "integrity-test");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.EnsureAsync<LogisticsLocation>(x => x.LocationId == "TOO-LONG",
            new() { LocationId = new string('X', max + 1) }, default));
        Assert.Contains("LogisticsLocation.LocationId exceeds", exception.Message);
        Assert.Empty(await db.Set<LogisticsLocation>().ToListAsync());
    }

    [SqlServerPhaseOneFact]
    public async Task SQL_Server_real_schema_first_run_rerun_and_example_are_consistent()
    {
        // A generated database name is mandatory: this test never targets an application database.
        var database = "IXApi_APV2_Integrity_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!,
            InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 30
        };
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await db.Database.EnsureCreatedAsync();
            await InitializeCompanyAsync(db);
            using var services = new ServiceCollection().AddAccountsPayableConfigurationAndMasterDataExamples().BuildServiceProvider();
            using var scope = services.CreateScope();
            var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>());
            var examples = new AccountsPayableCycleSeeder(runner, scope.ServiceProvider.GetServices<ISeedValidator>());
            var prepared = await examples.PrepareAsync(new(2026, "sql-integrity-test"));
            Assert.False(prepared.TransactionsExecuted);
            Assert.Equal(ExampleIdentifiers.Scope, prepared.Scope);
            Assert.Equal(58, prepared.CompletedSeeders.Count);
            Assert.Equal(2, prepared.MasterData.Vendors.Count);
            Assert.Equal(2, prepared.MasterData.Products.Count);
            Assert.Equal(3, prepared.MasterData.PurchasePrices.Count);
            await AssertBusinessExampleAsync(db);
            var vendorIds = await db.Set<VendTable>().OrderBy(x => x.AccountNum).Select(x => x.RecId).ToArrayAsync();
            var counts = await CountsAsync(db);
            db.ChangeTracker.Clear();
            var repeated = await examples.PrepareAsync(new(2026, "sql-integrity-test"));
            Assert.Equal(counts, await CountsAsync(db));
            Assert.Equal(vendorIds, await db.Set<VendTable>().OrderBy(x => x.AccountNum).Select(x => x.RecId).ToArrayAsync());
            Assert.Equal(prepared.MasterData.Vendors.Select(x => x.RecId), repeated.MasterData.Vendors.Select(x => x.RecId));
            await AssertTransactionStorageAsync(db);
        }
        finally
        {
            Assert.StartsWith("IXApi_APV2_Integrity_", db.Database.GetDbConnection().Database);
            await db.Database.EnsureDeletedAsync();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Cycle_examples_prepare_configuration_and_masters_only_and_return_persisted_references(bool existingCompany)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        using var provider = new ServiceCollection().AddScoped<ApplicationDbContext>(_ => new Fixture(options))
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
        Assert.Equal(Enumerable.Range(1, 58), result.CompletedSeeders.Select(x => x.Order));
        Assert.Equal(58, result.CompletedSeeders.Select(x => x.SeederId).Distinct().Count());
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
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        await new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>())
            .RunAsync(2026, "transaction-storage-test");
        await AssertTransactionStorageAsync(db);
    }

    private static async Task AssertTransactionStorageAsync(ApplicationDbContext db)
    {
        var company = await db.Set<CompanyInfo>().SingleAsync();
        var ledger = await db.Set<Ledger>().SingleAsync();
        var period = await db.Set<FiscalCalendarPeriod>().FirstAsync();
        var dimension = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.MainAccountValue == "681001");
        var header = new VendInvoiceInfoTable { ParmId = "STORAGE-TEST", Num = "STORAGE-ONLY", CurrencyCode = "SAR" };
        var line = new VendInvoiceInfoLine { ParmId = header.ParmId, OrigPurchId = "STORAGE-PO", InventTransId = "STORAGE-LINE", ReceiveNow = 1m };
        db.AddRange(header, line);
        await db.SaveChangesAsync();
        var receiptLink = new VendInvoiceInfoSubLine { DocumentId = "STORAGE-RECEIPT", ParmId = header.ParmId, LineRefRecId = line.RecId,
            ReceiveNow = 1.25m, InventNow = 1.25m }; // Polymorphic receipt reference intentionally not resolved by storage.
        var sourceHeader = new SourceDocumentHeader(); // Metadata values are intentionally not invented by storage tests.
        db.Add(sourceHeader);
        await db.SaveChangesAsync();
        var sourceLine = new SourceDocumentLine { SourceDocumentHeader = sourceHeader.RecId, TermQuantity = 1.25m };
        var accountingEvent = new AccountingEvent { SourceDocumentHeader = sourceHeader.RecId, AccountingDate = new DateTime(2026, 4, 15) };
        db.AddRange(sourceLine, accountingEvent);
        await db.SaveChangesAsync();
        var distribution = new AccountingDistribution { SourceDocumentHeader = sourceHeader.RecId, SourceDocumentLine = sourceLine.RecId,
            AccountingEvent = accountingEvent.RecId, AccountingLegalEntity = company.RecId, LedgerDimension = dimension.RecId,
            AccountingDate = new DateTime(2026, 4, 15), Number = 1, AllocationFactor = 1m,
            TransactionCurrency = "SAR", TransactionCurrencyAmount = 125.123456m };
        var journal = new SubledgerJournalEntry { AccountingEvent = accountingEvent.RecId, Ledger = ledger.RecId, FiscalCalendarPeriod = period.RecId,
            JournalNumber = "STORAGE-JOURNAL", Voucher = "STORAGE-VOUCHER", VoucherDataAreaId = "dat" };
        db.AddRange(receiptLink, distribution, journal);
        await db.SaveChangesAsync();
        var entry = new SubledgerJournalAccountEntry { SubledgerJournalEntry = journal.RecId, LedgerDimension = dimension.RecId,
            TransactionCurrency = "SAR", PostingType = IAX.IXApi.Modules.Finance.Common.LedgerPostingType.PurchConsump,
            TransactionCurrencyAmount = 125.123456m, AccountingCurrencyAmount = 125.123456m, ExchangeRate1 = 1m };
        db.Add(entry);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(1.25m, (await db.Set<VendInvoiceInfoSubLine>().SingleAsync()).ReceiveNow);
        Assert.Equal(line.RecId, (await db.Set<VendInvoiceInfoSubLine>().SingleAsync()).LineRefRecId);
        Assert.Equal(125.123456m, (await db.Set<AccountingDistribution>().SingleAsync()).TransactionCurrencyAmount);
        var persisted = await db.Set<SubledgerJournalAccountEntry>().SingleAsync();
        Assert.Equal(journal.RecId, persisted.SubledgerJournalEntry);
        Assert.Equal(125.123456m, persisted.AccountingCurrencyAmount);
        Assert.Null(persisted.GeneralJournalAccountEntry);
        var persistedDistribution = await db.Set<AccountingDistribution>().SingleAsync();
        Assert.Equal(sourceLine.RecId, persistedDistribution.SourceDocumentLine);
        Assert.Equal(sourceHeader.RecId, persistedDistribution.SourceDocumentHeader);
        Assert.Equal(accountingEvent.RecId, persistedDistribution.AccountingEvent);
        Assert.Equal(accountingEvent.RecId, (await db.Set<SubledgerJournalEntry>().SingleAsync()).AccountingEvent);
        var distributionLink = new SubledgerJournalAccountEntryDistribution { AccountingDistribution = distribution.RecId,
            ParentDistribution = distribution.RecId, SubledgerJournalAccountEntry = entry.RecId,
            AccountingCurrencyAmount = 125.123456m };
        db.Add(distributionLink);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var persistedLink = await db.Set<SubledgerJournalAccountEntryDistribution>().SingleAsync();
        Assert.Equal(distribution.RecId, persistedLink.AccountingDistribution);
        Assert.Equal(entry.RecId, persistedLink.SubledgerJournalAccountEntry);
        Assert.Equal(125.123456m, persistedLink.AccountingCurrencyAmount);
        Assert.Equal(sourceHeader.RecId, (await db.Set<SourceDocumentLine>().SingleAsync()).SourceDocumentHeader);
        Assert.Equal(sourceHeader.RecId, (await db.Set<AccountingEvent>().SingleAsync()).SourceDocumentHeader);
        db.ChangeTracker.Clear();
        db.Add(new VendInvoiceInfoSubLine { DocumentId = "ORPHAN", LineRefRecId = long.MaxValue });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        db.Add(new SubledgerJournalAccountEntry { LedgerDimension = dimension.RecId, SubledgerJournalEntry = long.MaxValue });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        foreach (var orphan in new object[] {
            new SourceDocumentLine { SourceDocumentHeader = long.MaxValue },
            new AccountingEvent { AccountingDate = new DateTime(2026, 4, 15), SourceDocumentHeader = long.MaxValue },
            new AccountingDistribution { SourceDocumentLine = long.MaxValue },
            new AccountingDistribution { AccountingEvent = long.MaxValue },
            new AccountingDistribution { FinalizeAccountingEvent = long.MaxValue },
            new SubledgerJournalEntry { AccountingEvent = long.MaxValue },
            new SubledgerJournalAccountEntryDistribution { AccountingDistribution = long.MaxValue },
            new SubledgerJournalAccountEntryDistribution { SubledgerJournalAccountEntry = long.MaxValue } })
        {
            db.Add(orphan);
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            db.ChangeTracker.Clear();
        }
    }

    [Fact]
    public async Task Phase_two_stock_service_cycle_runs_through_business_services_and_is_repeatable()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = TransactionServices(db);
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<AccountsPayableTransactionExamples>();
        var result = await examples.RunAsync(new(2026, "cycle-test"));
        Assert.Equal(18, result.Scenarios.Count);
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
    [SqlServerPhaseOneFact]
    public async Task SQL_Server_transaction_examples_run_twice_without_duplicate_rows()
    {
        var database = "IXApi_APV2_Cycle_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder { DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!,
            InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 30 };
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await db.Database.EnsureCreatedAsync();
            using var provider = TransactionServices(db);
            using var scope = provider.CreateScope();
            var examples = scope.ServiceProvider.GetRequiredService<AccountsPayableTransactionExamples>();
            var first = await examples.RunAsync(new(2026, "sql-cycle-test"));
            Assert.Equal(18, first.Scenarios.Count);
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
    [SqlServerPhaseOneFact]
    public async Task Two_concurrent_runs_of_the_same_cycle_do_not_duplicate_any_document()
    {
        var database = "IXApi_APV2_Concurrent_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder { DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!,
            InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 30 };
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

    /// <summary>
    /// Accounts payable service by service: every <see cref="BaseService{T}"/> in the module gets its
    /// read/create/update/delete contract checked, so the setup and master-data services that the
    /// cycle stages never touch are verified too. Each service is one recorded stage.
    /// </summary>
    [Fact]
    public async Task AP_services_one_by_one_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));

        var runId = Guid.NewGuid().ToString("N");
        var failures = new List<string>();
        void Record(string stage, string name, string status, string? detail = null) =>
            DetailedApRecord(runId, "SQLite", stage, name, status, detail);
        Record("run", "فحص خدمات الحسابات الدائنة خدمة بخدمة", "STARTED");

        async Task Stage(string stage, string name, Func<Task> action)
        {
            Record(stage, name, "RUNNING");
            try { await action(); Record(stage, name, "PASSED"); }
            catch (Exception ex)
            {
                var error = ex is TargetInvocationException { InnerException: not null } t ? t.InnerException! : ex;
                failures.Add(stage + ": " + error.Message); Record(stage, name, "FAILED", error.ToString());
            }
            finally { db.ChangeTracker.Clear(); }
        }

        var crudServices = typeof(IAX.IXApi.Modules.Finance.FinanceModule).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.Namespace?.StartsWith("IAX.IXApi.Modules.Finance.AccountsPayable") == true
                && t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(BaseService<>))
            .OrderBy(t => t.Name, StringComparer.Ordinal).ToArray();
        Assert.NotEmpty(crudServices);

        var actor = new CycleIdentity();
        var services = new ServiceCollection().AddLogging().AddSingleton<ApplicationDbContext>(db).AddSingleton(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddAccountsPayableTransactionExamples();
        foreach (var type in crudServices)
        {
            services.AddScoped(type);
            services.AddScoped(typeof(IBaseService<>).MakeGenericType(type.BaseType!.GetGenericArguments()[0]),
                p => p.GetRequiredService(type));
        }
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        // The cycle seeder supplies the vendors, groups, payment methods, parameters and posting
        // profiles the read paths need; running one scenario adds the vendor transactions.
        await sp.GetRequiredService<AccountsPayableCycleSeeder>().PrepareAsync(new(2026, "ap-service-test"));
        await sp.GetRequiredService<AccountsPayableTransactionExamples>().RunAsync(new(2026, "ap-service-test", ["01"]));
        db.ChangeTracker.Clear();

        // Posting a pending invoice consumes its VendInvoiceInfoTable row, so the cycle leaves none
        // behind. Hold one through the pending-invoice service itself and leave it unposted, so the
        // read paths of that service have a row to work with.
        await Stage("fixture", "فاتورة معلقة غير مرحلة لفحص خدمة الفواتير المعلقة", async () =>
        {
            var date = new DateTime(2026, 4, 15);
            var account = $"APV2-{db.GetDataAreaId()}-LOCAL";
            var created = await sp.GetRequiredService<IPurchOrderService>().CreateAsync(
                new(account, null, "SAR", date, "APV2-SITE", "APV2-WH", "AP-SERVICE-PENDING", date.AddDays(-1)), default);
            Assert.True(created.StatusCode == 200, created.Message);
            var order = (PurchTable)created.Data!;
            Assert.True((await sp.GetRequiredService<IPurchOrderService>().AddLineAsync(order.RecId,
                new("APV2-STOCK", 10m, "EA", 25m, 1m, "APV2-SITE", "APV2-WH", date, null), default)).StatusCode == 200);
            var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId);
            Assert.True((await sp.GetRequiredService<IPurchConfirmationService>().ConfirmAsync(order.RecId, default, date)).StatusCode == 200);
            Assert.True((await sp.GetRequiredService<IProductReceiptService>().PostAsync(order.RecId,
                new("AP-SERVICE-R1", date.AddDays(1), [new(line.RecId, 10m)]), default)).StatusCode == 200);
            Assert.True((await sp.GetRequiredService<IVendInvoiceInfoService>().CreateFromOrderAsync(order.RecId,
                new("AP-SERVICE-I1", date.AddDays(2), date.AddDays(2), Lines: [new(line.RecId, 10m)]), default)).StatusCode == 200);
            Assert.NotEmpty(await db.Set<VendInvoiceInfoTable>().Where(x => x.DataAreaId == "dat").ToListAsync());
        });

        foreach (var type in crudServices)
        {
            var entity = type.BaseType!.GetGenericArguments()[0];
            Record(type.Name, entity.Name + ": read/create/update/delete", "RUNNING");
            try
            {
                var check = typeof(FoundationServiceStageTests).GetMethod(nameof(FoundationServiceStageTests.CheckCrudAsync),
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!.MakeGenericMethod(entity);
                await (Task)check.Invoke(null, [db, sp.GetRequiredService(typeof(IBaseService<>).MakeGenericType(entity)), sp])!;
                Record(type.Name, entity.Name + ": read/create/update/delete", "PASSED");
            }
            catch (Exception ex)
            {
                var error = ex is TargetInvocationException { InnerException: not null } t ? t.InnerException! : ex;
                failures.Add(type.Name + ": " + error.Message);
                Record(type.Name, entity.Name + ": read/create/update/delete", "FAILED", error.ToString());
            }
            finally { actor.ResetArea(); db.ChangeTracker.Clear(); }
        }
        Record("run", "فحص خدمات الحسابات الدائنة خدمة بخدمة", failures.Count == 0 ? "PASSED" : "FAILED");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public async Task Detailed_AP_stages_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await RunDetailedApStagesAsync(db, "SQLite", async () =>
            await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'")));
    }

    [SqlServerPhaseOneFact]
    public async Task Detailed_AP_stages_SQLServer()
    {
        var connection = new SqlConnectionStringBuilder { DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!,
            InitialCatalog = "IXApi_APStage_" + Guid.NewGuid().ToString("N"), IntegratedSecurity = true, TrustServerCertificate = true };
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
        var created = false;
        var completed = false;
        var runId = Guid.NewGuid().ToString("N");
        try
        {
            await RunDetailedApStagesAsync(db, "SQL Server", async () => { created = await db.Database.EnsureCreatedAsync(); Assert.True(created); }, runId);
            completed = true;
        }
        finally
        {
            if (created)
            {
                DetailedApRecord(runId,"SQL Server","cleanup","تنظيف قاعدة الاختبار المؤقتة","RUNNING",connection.InitialCatalog);
                try
                {
                    // UnitOfWork owns/disposes its context; cleanup uses a fresh context.
                    await using var cleanup = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
                    Assert.True(await cleanup.Database.EnsureDeletedAsync());
                    DetailedApRecord(runId,"SQL Server","cleanup","تنظيف قاعدة الاختبار المؤقتة","PASSED",connection.InitialCatalog);
                    if (completed) DetailedApRecord(runId,"SQL Server","run","اختبار دورة الحسابات الدائنة","PASSED");
                }
                catch (Exception error)
                {
                    DetailedApRecord(runId,"SQL Server","cleanup","تنظيف قاعدة الاختبار المؤقتة","FAILED",error.ToString());
                    DetailedApRecord(runId,"SQL Server","run","اختبار دورة الحسابات الدائنة","FAILED",error.ToString());
                    throw;
                }
            }
        }
    }

    private static void DetailedApRecord(string runId, string providerName, string stage, string name, string status, string? detail = null)
    {
        var path = Environment.GetEnvironmentVariable("IXAPI_AP_STAGE_REPORT") ?? Path.Combine(AppContext.BaseDirectory, "ap-stage-results.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.AppendAllText(path, JsonSerializer.Serialize(new { runId, provider = providerName, stage, name, status, detail, atUtc = DateTime.UtcNow }) + Environment.NewLine);
    }
    private static async Task RunDetailedApStagesAsync(ApplicationDbContext db, string providerName, Func<Task> initialize, string? reportRunId = null)
    {
        var runId = reportRunId ?? Guid.NewGuid().ToString("N");
        void Record(string stage, string name, string status, string? detail = null) => DetailedApRecord(runId,providerName,stage,name,status,detail);
        async Task Stage(string number, string name, Func<Task> action)
        {
            Record(number, name, "RUNNING");
            try { await action(); db.ChangeTracker.Clear(); Record(number, name, "PASSED"); }
            catch (Exception error) { Record(number, name, "FAILED", error.ToString()); Record("run","اختبار دورة الحسابات الدائنة","FAILED",error.Message); throw; }
        }
        static void Success(PurchResult result) => Assert.True(result.StatusCode == 200, $"HTTP {result.StatusCode}: {result.Message}");
        Record("run", "اختبار دورة الحسابات الدائنة", "STARTED");
        await Stage("0", "قاعدة الاختبار والنموذج والمفاتيح الخارجية", initialize);
        using var provider = TransactionServices(db);
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        await Stage("1", "إعداد المورد والأصناف والضرائب وحسابات الترحيل", async () =>
        {
            await services.GetRequiredService<AccountsPayableCycleSeeder>().PrepareAsync(new(2026,"cycle-test"));
            Assert.Equal(2, await db.Set<VendTable>().CountAsync());
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        });
        var orders = services.GetRequiredService<IPurchOrderService>();
        PurchTable order = null!;
        PurchLine line = null!;
        VendInvoiceInfoTable held = null!;
        var date = new DateTime(2026,4,15);
        const string account = "APV2-dat-LOCAL";
        await Stage("2", "إنشاء أمر الشراء وبنوده", async () =>
        {
            var result = await orders.CreateAsync(new(account,null,"SAR",date,"APV2-SITE","APV2-WH","AP-STAGE-MAIN",date.AddDays(-1)),default);
            Success(result); order = (PurchTable)result.Data!;
            Success(await orders.AddLineAsync(order.RecId,new("APV2-STOCK",100m,"EA",25m,1m,"APV2-SITE","APV2-WH",date,null),default));
            line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId);
            Assert.Equal(100m,line.PurchQty); Assert.Equal(2500m,line.LineAmount);
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        });
        await Stage("3", "تأكيد أمر الشراء وحفظ نسخة التأكيد", async () =>
        {
            Success(await services.GetRequiredService<IPurchConfirmationService>().ConfirmAsync(order.RecId,default,date));
            var confirmation = await db.Set<VendPurchOrderJour>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId);
            Assert.Equal(100m,confirmation.Qty); Assert.Equal(2500m,confirmation.Amount);
            Assert.Single(await db.Set<VendPurchOrderTrans>().Where(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId).ToListAsync());
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        });
        await Stage("4", "استلام البضاعة والتحديث المادي للمخزون", async () =>
        {
            Success(await services.GetRequiredService<IProductReceiptService>().PostAsync(order.RecId,new("AP-STAGE-R1",date.AddDays(1),[new(line.RecId,100m)]),default));
            Assert.Equal(100m,(await db.Set<VendPackingSlipJour>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId)).Qty);
            var received = await db.Set<PurchLine>().SingleAsync(x => x.RecId == line.RecId);
            Assert.Equal(0m,received.RemainPurchPhysical); Assert.Equal(100m,received.RemainPurchFinancial);
            Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
            var stock = await db.Set<InventSum>().Where(x => x.DataAreaId == "dat" && x.ItemId == "APV2-STOCK").ToListAsync();
            Assert.Equal(100m,stock.Sum(x => x.PhysicalInvent));
            Assert.Equal(100m,stock.Sum(x => x.Received));
            Assert.Equal(2500m,stock.Sum(x => x.PhysicalValue));
            Assert.Equal(0m,stock.Sum(x => x.PostedValue));
            var accrual = Assert.Single(await db.Set<PurchReceiptAccrual>().ToListAsync());
            Assert.Equal(100m,accrual.InventQuantity); Assert.Equal(2500m,accrual.AmountMst);
            Assert.Equal(0m,accrual.ReversedInventQuantity); Assert.Equal(0m,accrual.ReversedAmountMst);
            var receiptEntries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
            Assert.Equal(2500m,receiptEntries.Where(x => x.LedgerDimension == accrual.InventoryLedgerDimension).Sum(x => x.AccountingCurrencyAmount));
            Assert.Equal(-2500m,receiptEntries.Where(x => x.LedgerDimension == accrual.AccrualLedgerDimension).Sum(x => x.AccountingCurrencyAmount));
            Assert.Empty(await db.Set<VendTrans>().ToListAsync());
            Assert.Empty(await db.Set<TaxTrans>().ToListAsync());
            Assert.Empty(await db.Set<BankAccountTrans>().ToListAsync());
        });
        var pending = services.GetRequiredService<IVendInvoiceInfoService>();
        await Stage("5", "إنشاء الفاتورة المعلقة وربطها بالاستلام", async () =>
        {
            var entryCount = await db.Set<GeneralJournalAccountEntry>().CountAsync();
            Success(await pending.CreateFromOrderAsync(order.RecId,new("AP-STAGE-I1",date.AddDays(2),date.AddDays(2),Lines: [new(line.RecId,100m)]),default));
            held = await db.Set<VendInvoiceInfoTable>().SingleAsync(x => x.DataAreaId == "dat" && x.Num == "AP-STAGE-I1");
            var ids = await db.Set<VendInvoiceInfoLine>().Where(x => x.DataAreaId == "dat" && x.ParmId == held.ParmId).Select(x => x.RecId).ToArrayAsync();
            Assert.True(await db.Set<VendInvoiceInfoSubLine>().AnyAsync(x => x.DataAreaId == "dat" && x.LineRefRecId != null && ids.Contains(x.LineRefRecId.Value)));
            Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
            Assert.Equal(entryCount,await db.Set<GeneralJournalAccountEntry>().CountAsync());
            Assert.Empty(await db.Set<VendTrans>().ToListAsync());
            Assert.Empty(await db.Set<TaxTrans>().ToListAsync());
            Assert.Empty(await db.Set<BankAccountTrans>().ToListAsync());
        });
        await Stage("6", "ترحيل فاتورة المورد والضريبة", async () =>
        {
            Success(await pending.PostAsync(held.ParmId,default));
            var invoice = await db.Set<VendInvoiceJour>().SingleAsync(x => x.DataAreaId == "dat" && x.InvoiceId == "AP-STAGE-I1");
            Assert.Equal(2875m,invoice.InvoiceAmount); Assert.Equal(375m,invoice.SumTax);
            Assert.Equal(0m,(await db.Set<PurchLine>().SingleAsync(x => x.RecId == line.RecId)).RemainPurchFinancial);
            var stock = await db.Set<InventSum>().Where(x => x.DataAreaId == "dat" && x.ItemId == "APV2-STOCK").ToListAsync();
            Assert.Equal(0m,stock.Sum(x => x.Received));
            Assert.Equal(0m,stock.Sum(x => x.PhysicalValue));
            Assert.Equal(100m,stock.Sum(x => x.PostedQty));
            Assert.Equal(2500m,stock.Sum(x => x.PostedValue));
            var accrual = Assert.Single(await db.Set<PurchReceiptAccrual>().ToListAsync());
            Assert.Equal(100m,accrual.ReversedInventQuantity); Assert.Equal(2500m,accrual.ReversedAmountMst);
        });
        await Stage("7", "حركة المورد والرصيد المفتوح", async () =>
        {
            var open = await db.Set<VendTransOpen>().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == account);
            Assert.Equal(-2875m,open.AmountCur);
            Assert.True(await db.Set<VendTrans>().AnyAsync(x => x.DataAreaId == "dat" && x.RecId == open.RefRecId && x.Invoice == "AP-STAGE-I1"));
        });
        await Stage("8", "دفع المورد وحركة البنك دون تسوية تلقائية", async () =>
        {
            Success(await services.GetRequiredService<IVendPaymentService>().PostAsync(new(account,"APV2-BANK",2875m,"AP-STAGE-PAY",date.AddDays(35),SettleAutomatically:false),default));
            Assert.Equal(2,await db.Set<VendTransOpen>().CountAsync(x => x.DataAreaId == "dat" && x.AccountNum == account));
            Assert.Equal(-2875m,(await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == "dat").ToListAsync()).Sum(x => x.AmountCur));
        });
        await Stage("9", "تسوية الفاتورة مع الدفعة وإقفال الرصيد", async () =>
        {
            var ids = await db.Set<VendTransOpen>().Where(x => x.DataAreaId == "dat" && x.AccountNum == account).Select(x => x.RefRecId).ToArrayAsync();
            Success(await services.GetRequiredService<IVendSettleOpenTransactionsService>().SettleAsync(new(account,date.AddDays(36),ids),default));
            Assert.False(await db.Set<VendTransOpen>().AnyAsync(x => x.DataAreaId == "dat" && x.AccountNum == account));
            Assert.True(await db.Set<VendSettlement>().AnyAsync(x => x.DataAreaId == "dat" && x.AccountNum == account));
        });
        await Stage("10", "مطابقة قيود الأستاذ والمخزون والقيمة", async () =>
        {
            var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == "dat").ToListAsync();
            Assert.NotEmpty(entries);
            Assert.All(entries.GroupBy(x => x.GeneralJournalEntry), voucher => Assert.Equal(0m,voucher.Sum(x => x.AccountingCurrencyAmount)));
            var origins = await db.Set<InventTransOrigin>().Where(x => x.DataAreaId == "dat" && x.ReferenceId == order.PurchId).Select(x => x.RecId).ToArrayAsync();
            var stock = await db.Set<InventTrans>().Where(x => x.DataAreaId == "dat" && origins.Contains(x.InventTransOrigin)).ToListAsync();
            Assert.Equal(100m,stock.Sum(x => x.Qty)); Assert.Equal(2500m,stock.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment));
            var summaries = await db.Set<InventSum>().Where(x => x.DataAreaId == "dat" && x.ItemId == "APV2-STOCK").ToListAsync();
            Assert.Equal(100m,summaries.Sum(x => x.PhysicalInvent));
            Assert.Equal(100m,summaries.Sum(x => x.AvailPhysical));
            Assert.Equal(2500m,summaries.Sum(x => x.PostedValue));
            var company = await db.Set<CompanyInfo>().SingleAsync(x => x.DataArea == "dat");
            Assert.True(await db.Set<AccountingDistribution>().AnyAsync(x => x.AccountingLegalEntity == company.RecId));
            var postedEntryIds = entries.Select(x => x.RecId).ToArray();
            Assert.True(await db.Set<SubledgerJournalAccountEntry>().AnyAsync(x => x.GeneralJournalAccountEntry != null && postedEntryIds.Contains(x.GeneralJournalAccountEntry.Value)));
        });
        var names = new Dictionary<string,string>
        {
            ["01"]="شراء مخزني وخدمي حتى الدفع", ["02"]="استلام وفوترة جزئية", ["03"]="شراء خدمة دون مخزون",
            ["04"]="فاتورة مصروف مباشرة", ["05"]="فاتورة مقابل عدة استلامات", ["06"]="مطابقة السعر والكمية ورفض المخالفة",
            ["07"]="مصاريف الشراء", ["08"]="دفعة جزئية", ["09"]="دفعة على الحساب ثم تسوية",
            ["11"]="دفعة مقدمة من يومية الدفع وتطبيقها", ["12"]="الخصم النقدي", ["13"]="عملة أجنبية وفروق صرف محققة",
            ["14"]="مرتجع شراء وإشعار دائن", ["15"]="إشعار دائن مالي دون مخزون", ["16"]="إلغاء الكمية المتبقية",
            ["18"]="عكس التسوية", ["19"]="عكس الدفعة", ["20"]="رفض العمليات غير الصحيحة والتراجع عن آثارها"
        };
        var examples = services.GetRequiredService<AccountsPayableTransactionExamples>();
        foreach (var id in AccountsPayableTransactionFlow.Scenarios.Keys.OrderBy(x => x,StringComparer.Ordinal))
            await Stage("scenario-"+id,names[id],async () =>
            {
                var result = await examples.RunAsync(new(2026,"cycle-test",[id]));
                var scenario = Assert.Single(result.Scenarios);
                Assert.Equal(id,scenario.Id); Assert.True(scenario.PostedVouchers > 0);
                await AssertIndependentScenarioAmountsAsync(db, id);
            });
        await Stage("11", "إعادة الدورة دون تكرار مستندات أو حركات", async () =>
        {
            var before = await CountsAsync(db);
            await examples.RunAsync(new(2026,"cycle-test"));
            Assert.Equal(before,await CountsAsync(db));
        });
        Record("scenario-10","فاتورة دفعة مقدمة مرتبطة بأمر الشراء","NOT_IMPLEMENTED","Not implemented by the current AP example/business-service path.");
        Record("scenario-17","تصحيح استلام تاريخي","NOT_IMPLEMENTED","Not implemented by the current AP example/business-service path.");
        Record("run","اختبار دورة الحسابات الدائنة","PASSED");
    }

    // Independent arithmetic oracle: fixed fixture inputs, never totals read back from the service.
    // BankCur is SAR except scenario 13 (USD); all GL/tax/value amounts are SAR.
    private static async Task AssertIndependentScenarioAmountsAsync(ApplicationDbContext db, string id)
    {
        (decimal Qty, decimal Value, decimal Tax, decimal BankCur, decimal BankMst, decimal Expense,
            decimal Delivery, decimal Discount, decimal FxLoss, decimal Control, decimal Debit, decimal Credit) expected = id switch
        {
            "01" => (100m,2500m,480m,-3680m,-3680m,700m,0m,0m,0m,0m,0m,0m),
            "03" => (0m,0m,105m,-805m,-805m,700m,0m,0m,0m,0m,0m,0m),
            "04" => (0m,0m,52.50m,-402.50m,-402.50m,350m,0m,0m,0m,0m,0m,0m),
            "07" => (100m,2500m,382.50m,-2932.50m,-2932.50m,0m,50m,0m,0m,0m,0m,0m),
            "08" => (100m,2500m,375m,-1000m,-1000m,0m,0m,0m,0m,-1875m,0m,1875m),
            "12" => (100m,2500m,375m,-2817.50m,-2817.50m,0m,0m,-57.50m,0m,0m,0m,0m),
            // 700 USD net + 105 USD VAT at 3.75; payment 805 USD at 3.80.
            "13" => (100m,2625m,393.75m,-805m,-3059m,0m,0m,0m,40.25m,0m,0m,0m),
            "14" => (90m,2250m,337.50m,-2587.50m,-2587.50m,0m,0m,0m,0m,0m,0m,0m),
            "15" => (0m,0m,37.50m,-287.50m,-287.50m,250m,0m,0m,0m,0m,0m,0m),
            "16" or "20" => (60m,1500m,225m,-1725m,-1725m,0m,0m,0m,0m,0m,0m,0m),
            "18" => (100m,2500m,375m,-2875m,-2875m,0m,0m,0m,0m,0m,2875m,2875m),
            "19" => (100m,2500m,375m,0m,0m,0m,0m,0m,0m,-2875m,0m,2875m),
            "02" or "05" or "06" or "09" or "11" => (100m,2500m,375m,-2875m,-2875m,0m,0m,0m,0m,0m,0m,0m),
            _ => throw new InvalidOperationException($"Missing independent arithmetic expectations for AP scenario {id}.")
        };
        var actual = await AccountsPayableReconciliationSnapshot.LoadAsync(db, $"APV2-2026-{id}", id,
            expected.Qty, expected.Debit, expected.Credit, default);
        Assert.Equal(expected.Qty, actual.PhysicalQuantity);
        Assert.Equal(expected.Value, actual.Inventory.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment));
        Assert.Equal(expected.Tax, actual.Tax.Sum(x => x.TaxAmount));
        Assert.Equal(expected.BankCur, actual.Bank.Sum(x => x.AmountCur));
        Assert.Equal(expected.BankMst, actual.Bank.Sum(x => x.AmountMst));
        Assert.Equal(expected.Debit, actual.OpenDebit);
        Assert.Equal(expected.Credit, actual.OpenCredit);
        var accounts = new Dictionary<string, decimal>
        {
            [GeneralLedgerSeedExample.Bank] = expected.BankMst,
            [GeneralLedgerSeedExample.Inventory] = expected.Value,
            [GeneralLedgerSeedExample.RecoverablePurchaseTax] = expected.Tax,
            [GeneralLedgerSeedExample.VendorControl] = expected.Control,
            [GeneralLedgerSeedExample.MaintenanceExpense] = expected.Expense,
            [GeneralLedgerSeedExample.DeliveryExpense] = expected.Delivery,
            [GeneralLedgerSeedExample.PurchaseCashDiscount] = expected.Discount,
            [GeneralLedgerSeedExample.RealizedExchangeLoss] = expected.FxLoss,
            [GeneralLedgerSeedExample.RealizedExchangeGain] = 0m,
            [GeneralLedgerSeedExample.VendorAdvance] = 0m,
            [GeneralLedgerSeedExample.ReceivedNotInvoicedInterim] = 0m,
            [GeneralLedgerSeedExample.ReceivedNotInvoiced] = 0m
        };
        foreach (var (account, amount) in accounts)
            Assert.True(amount == actual.LedgerAmount(account),
                $"Scenario {id}, account {account}: expected {amount}, actual {actual.LedgerAmount(account)} SAR.");
        Assert.All(actual.Entries.GroupBy(x => x.GeneralJournalEntry),
            voucher => Assert.Equal(0m, voucher.Sum(x => x.AccountingCurrencyAmount)));
    }

    private static ServiceProvider TransactionServices(ApplicationDbContext db)
    {
        var actor = new CycleIdentity();
        return new ServiceCollection().AddSingleton(db).AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor).AddAccountsPayableTransactionExamples().BuildServiceProvider();
    }
    private sealed class CycleIdentity : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "cycle-test";
        public string GetOwnerAccountId() => "cycle-test";
    }

    private static async Task InitializeCompanyAsync(ApplicationDbContext db)
    {
        var party = new DirPartyTable { PartyNumber = "APV2-TEST-LEGAL", Name = "Riyadh Business Services (Demo)", IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes };
        db.Add(party);
        await db.SaveChangesAsync();
        db.Add(new CompanyInfo { DataArea = "dat", Name = party.Name, Party = party.RecId });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static async Task AssertBusinessExampleAsync(ApplicationDbContext db)
    {
        var local = await db.Set<VendTable>().SingleAsync(x => x.AccountNum == "APV2-dat-LOCAL");
        var party = await db.Set<DirPartyTable>().SingleAsync(x => x.RecId == local.Party);
        Assert.Equal(VendorSeedExample.Name("LOCAL"), party.Name);
        var prices = await db.Set<PriceDiscTable>().Where(x => x.AccountRelation == local.AccountNum).ToListAsync();
        var paper = prices.Single(x => x.ItemRelation == InventorySeedExample.StockItem);
        var maintenance = prices.Single(x => x.ItemRelation == InventorySeedExample.ServiceItem);
        var net = 100m * paper.Amount / paper.PriceUnit + 2m * maintenance.Amount / maintenance.PriceUnit;
        var rate = await db.Set<TaxData>().SingleAsync(x => x.TaxCode == "APV2-15");
        var tax = decimal.Round(net * rate.TaxValue / 100m, 2);
        Assert.Equal(3200m, net);
        Assert.Equal(480m, tax);
        Assert.Equal(3680m, net + tax);
        Assert.Equal("SAR", paper.Currency);
        Assert.Equal("SAR", maintenance.Currency);
        var terms = await db.Set<PaymTerm>().SingleAsync(x => x.PaymTermId == local.PaymTermId);
        Assert.Equal(new DateTime(2026, 5, 15), new DateTime(2026, 4, 15).AddDays(terms.NumOfDays));
    }

    private sealed class CorruptExampleSeeder(string[] dependencies, string corruption) : IDataSeeder
    {
        public string Id => "ZZ.CorruptExample";
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            if (corruption == "price")
            {
                var price = await c.Rows<PriceDiscTable>().SingleAsync(x => x.AccountRelation == "APV2-dat-LOCAL" && x.ItemRelation == "APV2-STOCK", ct);
                price.Currency = "USD"; // An existing currency; a foreign key alone cannot catch this mismatch.
            }
            else if (corruption == "storage")
            {
                var row = await c.Rows<IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResStorageDimensionGroupFldSetup>()
                    .SingleAsync(x => x.DimensionFieldId == IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.InventDimensionFields.Site, ct);
                row.IsFinancialInventoryEnabled = false;
            }
            else if (corruption == "charge")
            {
                var row = await c.Rows<MarkupTable>().SingleAsync(ct);
                row.CustomerLedgerDimension = (await c.Rows<DimensionAttributeValueCombination>().SingleAsync(x => x.MainAccountValue == "181001", ct)).RecId;
            }
            else
            {
                var account = await c.Rows<MainAccount>().FirstAsync(ct);
                var combination = new DimensionAttributeValueCombination { DataAreaId = "oth", MainAccount = account.RecId, MainAccountValue = account.MainAccountId, DisplayValue = "OTHER-COMPANY" };
                c.Db.Add(combination);
                await c.Db.SaveChangesAsync(ct);
                var bank = await c.Rows<BankAccountTable>().SingleAsync(x => x.AccountId == "APV2-BANK", ct);
                bank.LedgerDimension = combination.RecId;
            }
            await c.Db.SaveChangesAsync(ct);
        }
    }

    private static async Task<string> CountsAsync(ApplicationDbContext db)
    {
        var counts = new List<string>();
        var opened = db.Database.GetDbConnection().State != System.Data.ConnectionState.Open;
        if (opened) await db.Database.OpenConnectionAsync();
        try
        {
        foreach (var table in db.Model.GetEntityTypes().Select(x => x.GetTableName()).OfType<string>().Distinct().Order())
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM \"" + table.Replace("\"", "\"\"") + "\"";
            counts.Add(table + ":" + await command.ExecuteScalarAsync());
        }
        return string.Join(";", counts);
        }
        finally
        {
            if (opened) await db.Database.CloseConnectionAsync();
        }
    }

    private sealed class Stub(string id, string[] dependencies) : IDataSeeder
    {
        public string Id => id;
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public Task SeedAsync(SeedContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class Fixture(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Execute the production relationships in SQLite; normalize SQL Server storage types only.
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

public sealed class SqlServerPhaseOneFactAttribute : FactAttribute
{
    public SqlServerPhaseOneFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")))
            Skip = "Set IXAPI_AP_PHASE_ONE_SQLSERVER to opt into a disposable local SQL Server database.";
    }
}
