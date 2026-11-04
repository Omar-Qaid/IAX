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

    private static ServiceProvider TransactionServices(ApplicationDbContext db)
    {
        var actor = new CycleIdentity();
        return new ServiceCollection().AddSingleton(db).AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor).AddAccountsPayableTransactionExamples().BuildServiceProvider();
    }
    private sealed class CycleIdentity : ICompanyExecutionContext, ICurrentUserService
    {
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
