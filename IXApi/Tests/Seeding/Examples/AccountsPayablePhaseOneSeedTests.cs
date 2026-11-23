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


[Collection(SeedingCollection.Name)]
public sealed partial class AccountsPayablePhaseOneSeedTests
{
    [Fact]
    public void Registration_is_complete_idempotent_and_has_no_transaction_seeders()
    {
        var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().AddAccountsPayablePhaseOneSeeding();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var ordered = new SeedDependencyResolver().Resolve(scope.ServiceProvider.GetServices<IDataSeeder>());
        Assert.Equal(64, ordered.Count); // includes VendPrepaymentSetupSeeder
        Assert.Single(ordered, x => x.Id == "ERPV2.TaxVatCategorySeeder");
        Assert.Single(ordered, x => x.Id == "ERPV2.InventoryNumberSequenceSeeder");
        // The inventory journal setup: its own account structure for the profit and loss codes, and
        // the journal names the journal services cannot create or post without.
        Assert.Single(ordered, x => x.Id == "ERPV2.InventoryAccountStructureSeeder");
        Assert.Single(ordered, x => x.Id == "ERPV2.InventJournalNameSeeder");
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
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
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
        Assert.Equal(scope.ServiceProvider.GetServices<IDataSeeder>().Select(x => x.Id).OrderBy(x => x),
            first.CompletedSeeders.OrderBy(x => x));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SeedContext(db, 2026, "phase-one-test")
            .RequireAsync<LogisticsLocation>(x => x.LocationId == "OTHER-LOCATION", default));
        Assert.Equal(3, await db.Set<VendTable>().CountAsync()); // LOCAL, USD and the tax-free prepayment vendor
        Assert.Equal(2, await db.Set<VendBankAccount>().CountAsync());
        var vendor = await db.Set<VendTable>().SingleAsync(x => x.AccountNum == "APV2-dat-LOCAL");
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
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
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
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var max = db.Model.FindEntityType(typeof(LogisticsLocation))!.FindProperty(nameof(LogisticsLocation.LocationId))!.GetMaxLength()!.Value;
        var context = new SeedContext(db, 2026, "integrity-test");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.EnsureAsync<LogisticsLocation>(x => x.LocationId == "TOO-LONG",
            new() { LocationId = new string('X', max + 1) }, default));
        Assert.Contains("LogisticsLocation.LocationId exceeds", exception.Message);
        Assert.Empty(await db.Set<LogisticsLocation>().ToListAsync());
    }

    [SqlServerFact]
    public async Task SQL_Server_real_schema_first_run_rerun_and_example_are_consistent()
    {
        // A generated database name is mandatory: this test never targets an application database.
        var database = "IXApi_APV2_Integrity_" + Guid.NewGuid().ToString("N");
        var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(TestEnvironment.SqlServerConnectionString(database));
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
            Assert.Equal(scope.ServiceProvider.GetServices<IDataSeeder>().Select(x => x.Id).OrderBy(x => x),
                prepared.CompletedSeeders.Select(x => x.SeederId).OrderBy(x => x));
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
            Assert.Equal(3, await db.Set<VendTable>().CountAsync()); // LOCAL, USD and the tax-free prepayment vendor
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
            var receipts = services.GetRequiredService<IProductReceiptService>();
            var duplicate = await receipts.PostAsync(order.RecId,
                new("AP-STAGE-R1", date.AddDays(1), [new(line.RecId, 50m), new(line.RecId, 50m)]), default);
            Assert.Equal(422, duplicate.StatusCode);
            Assert.Empty(await db.Set<VendPackingSlipJour>().ToListAsync());
            Success(await receipts.PostAsync(order.RecId,new("AP-STAGE-R1",date.AddDays(1),[new(line.RecId,100m)]),default));
            var receipt = await db.Set<VendPackingSlipJour>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId);
            Assert.Equal(100m, receipt.Qty);
            var version = await db.Set<VendPackingSlipVersion>().AsNoTracking()
                .SingleAsync(x => x.VendPackingSlipJour == receipt.RecId);
            Assert.Equal(receipt.InternalPackingSlipId, version.InternalPackingSlipId);
            Assert.Equal(receipt.LedgerVoucher, version.LedgerVoucher);
            Assert.Equal(date.AddDays(1), version.AccountingDate);
            Assert.Equal(100m, version.Qty);
            var receiptLine = await db.Set<VendPackingSlipTrans>().AsNoTracking()
                .SingleAsync(x => x.DataAreaId == "dat" && x.VendPackingSlipJour == receipt.RecId);
            var history = await db.Set<VendPackingSlipTransHistory>().AsNoTracking()
                .SingleAsync(x => x.DataAreaId == "dat" && x.VendPackingSlipTrans == receiptLine.RecId);
            Assert.Equal(version.VersionDateTime, history.ValidFrom);
            Assert.Equal(100m, history.InventQty);
            Assert.Equal(0m, history.RemainInvent);
            Assert.Equal(2500m, history.ValueMst);
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
            ["10"]="دفعة مقدمة على أمر الشراء وتطبيقها", ["11"]="دفعة مقدمة من يومية الدفع وتطبيقها", ["12"]="الخصم النقدي", ["13"]="عملة أجنبية وفروق صرف محققة",
            ["14"]="مرتجع شراء وإشعار دائن", ["15"]="إشعار دائن مالي دون مخزون", ["16"]="إلغاء الكمية المتبقية", ["17"]="تصحيح إيصال الاستلام",
            ["18"]="عكس التسوية", ["19"]="عكس الدفعة", ["20"]="رفض العمليات غير الصحيحة والتراجع عن آثارها",
            ["21"]="استرداد إشعار دائن من المورد"
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
        await Stage("scenario-10", "فاتورة دفعة مقدمة مرتبطة بأمر الشراء", async () =>
        {
            var vendor = await db.Set<VendTable>().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == account);
            vendor.TaxGroup = string.Empty;
            var category = new EcoResCategory { Name = "AP stage prepayment", IsActive = true };
            db.Add(category);
            var parameters = await db.Set<VendParameters>().SingleAsync(x => x.DataAreaId == "dat");
            parameters.AdvancePostingProfile = "APSTADV";
            var dimensions = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == "dat"
                && (x.MainAccountValue == GeneralLedgerSeedExample.VendorAdvance || x.MainAccountValue == GeneralLedgerSeedExample.ReceivedNotInvoiced)).ToListAsync();
            var asset = Assert.Single(dimensions, x => x.MainAccountValue == GeneralLedgerSeedExample.VendorAdvance);
            var liability = Assert.Single(dimensions, x => x.MainAccountValue == GeneralLedgerSeedExample.ReceivedNotInvoiced);
            db.Add(new VendLedgerAccounts { DataAreaId = "dat", PostingProfile = "APSTADV",
                AccountCode = IAX.IXApi.Modules.Finance.Common.AccountCode.All, SummaryLedgerDimension = liability.RecId });
            await db.SaveChangesAsync();
            db.Add(new InventPosting { DataAreaId = "dat", InventAccountType = IAX.IXApi.Modules.Finance.Common.InventAccountType.PurchAdvance,
                ItemCode = IAX.IXApi.Modules.Finance.Common.TableGroupAll.All, CustVendCode = IAX.IXApi.Modules.Finance.Common.TableGroupAll.All,
                LedgerDimension = asset.RecId, CategoryRelation = category.RecId });
            await db.SaveChangesAsync();
            var created = await orders.CreateAsync(new(account, null, "SAR", date, "APV2-SITE", "APV2-WH", "AP-STAGE-ADV", date.AddDays(-1)), default);
            Success(created);
            var purchase = (PurchTable)created.Data!;
            Success(await orders.AddLineAsync(purchase.RecId, new("APV2-STOCK", 10m, "EA", 100m, 1m, "APV2-SITE", "APV2-WH", date, null), default));
            var advanceLine = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == purchase.PurchId);
            Success(await orders.DefinePrepaymentAsync(purchase.RecId, new(300m, category.RecId, "Order deposit"), default));
            var invoices = services.GetRequiredService<IVendorInvoiceService>();
            var advanceResult = await invoices.PostPrepaymentAsync(purchase.RecId, "AP-STAGE-ADV-I", date, 300m, default);
            Success(advanceResult);
            var advance = (VendInvoiceJour)advanceResult.Data!;
            Success(await services.GetRequiredService<IProductReceiptService>().PostAsync(purchase.RecId,
                new("AP-STAGE-ADV-R", date.AddDays(1), [new(advanceLine.RecId, 10m)]), default));
            Success(await invoices.PostAsync(purchase.RecId, new("AP-STAGE-ADV-F", date.AddDays(2), [new(advanceLine.RecId, 10m)]), default));
            var final = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync(x => x.DataAreaId == "dat" && x.InvoiceId == "AP-STAGE-ADV-F");
            Assert.Equal(1000m, final.InvoiceAmount);
            Assert.Equal(422, (await invoices.ApplyPrepaymentAsync(purchase.RecId, advance.RecId, final.RecId, date.AddDays(3), 200m, default)).StatusCode);
            Success(await services.GetRequiredService<IVendPaymentService>().PostAsync(
                new(account, "APV2-BANK", 300m, "AP-STAGE-ADV-P", date.AddDays(3), SettleInvoiceId: advance.InvoiceId), default));
            var applied = await invoices.ApplyPrepaymentAsync(purchase.RecId, advance.RecId, final.RecId, date.AddDays(4), 200m, default);
            Success(applied);
            Success(await invoices.ApplyPrepaymentAsync(purchase.RecId, advance.RecId, final.RecId, date.AddDays(4), 200m, default));
            var definition = await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync(x => x.DataAreaId == "dat" && x.PurchTable == purchase.PurchId);
            Assert.Equal(100m, definition.PrepayApplicationRemaining);
            var finalTrans = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.DataAreaId == "dat" && x.Voucher == final.LedgerVoucher && x.Invoice == final.InvoiceId);
            var balance = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.DataAreaId == "dat" && x.RefRecId == finalTrans.RecId);
            Assert.Equal(-800m, balance.AmountCur);
            var match = (VendSettlement)applied.Data!;
            var entryId = await db.Set<GeneralJournalEntry>().Where(x => x.DataAreaId == "dat" && x.SubledgerVoucher == match.Voucher)
                .Select(x => x.RecId).SingleAsync();
            var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == "dat" && x.GeneralJournalEntry == entryId).ToListAsync();
            Assert.Equal(-200m, Assert.Single(entries, x => x.LedgerDimension == asset.RecId).AccountingCurrencyAmount);
            Assert.Equal(200m, Assert.Single(entries, x => x.LedgerDimension != asset.RecId).AccountingCurrencyAmount);

            var reversed = await invoices.ReversePrepaymentApplicationAsync(purchase.RecId, match.RecId, date.AddDays(5), default);
            Success(reversed);
            Success(await invoices.ReversePrepaymentApplicationAsync(purchase.RecId, match.RecId, date.AddDays(5), default));
            var reversedMatch = (VendSettlement)reversed.Data!;
            Assert.Equal(-200m, reversedMatch.AmountCur);
            Assert.Equal(300m, (await db.Set<PurchPrepayTable>().AsNoTracking()
                .SingleAsync(x => x.DataAreaId == "dat" && x.PurchTable == purchase.PurchId)).PrepayApplicationRemaining);
            Assert.Equal(-1000m, (await db.Set<VendTransOpen>().AsNoTracking()
                .SingleAsync(x => x.DataAreaId == "dat" && x.RefRecId == finalTrans.RecId)).AmountCur);
            var reversedEntryId = await db.Set<GeneralJournalEntry>().Where(x => x.DataAreaId == "dat" && x.SubledgerVoucher == reversedMatch.Voucher)
                .Select(x => x.RecId).SingleAsync();
            var reversalLines = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == "dat" && x.GeneralJournalEntry == reversedEntryId).ToListAsync();
            foreach (var original in entries)
                Assert.Equal(-original.AccountingCurrencyAmount, Assert.Single(reversalLines, x => x.LedgerDimension == original.LedgerDimension).AccountingCurrencyAmount);
        });
        await Stage("scenario-17", "تصحيح استلام تاريخي", async () =>
        {
            const string item = "APV2-STOCK";
            var assignment = await db.Set<InventModelGroupItem>().SingleAsync(x => x.ItemDataAreaId == "dat" && x.ItemId == item);
            var model = await db.Set<InventModelGroup>().SingleAsync(x => x.DataAreaId == "dat" && x.ModelGroupId == assignment.ModelGroupId);
            model.PostPhysicalPurchase = IAX.IXApi.Modules.Finance.Common.NoYes.Yes;
            model.PostOnHandPhysical = IAX.IXApi.Modules.Finance.Common.NoYes.Yes;
            await db.SaveChangesAsync();

            var created = await orders.CreateAsync(new(account, null, "SAR", date, "APV2-SITE", "APV2-WH", "AP-STAGE-CORRECT", date.AddDays(-1)), default);
            Success(created);
            var correctionOrder = (PurchTable)created.Data!;
            Success(await orders.AddLineAsync(correctionOrder.RecId, new(item, 100m, "EA", 100m, 1m, "APV2-SITE", "APV2-WH", date, null), default));
            var correctionLine = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == correctionOrder.PurchId);
            var summary = db.Set<InventSum>().AsNoTracking().Where(x => x.DataAreaId == "dat" && x.ItemId == item && x.InventDimId == correctionLine.InventDimId);
            var stockBefore = await summary.SingleAsync();
            var vendorRowsBefore = await db.Set<VendTrans>().CountAsync(x => x.DataAreaId == "dat");
            var receipts = services.GetRequiredService<IProductReceiptService>();
            var posted = await receipts.PostAsync(correctionOrder.RecId, new("AP-STAGE-CORRECT-R", date.AddDays(1), [new(correctionLine.RecId, 60m)]), default);
            Success(posted);
            var receipt = (VendPackingSlipJour)posted.Data!;
            var receiptLine = await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync(x => x.DataAreaId == "dat" && x.VendPackingSlipJour == receipt.RecId);
            var initial = await db.Set<PurchReceiptAccrual>().AsNoTracking().SingleAsync(x => x.DataAreaId == "dat" && x.PackingSlipTransRecId == receiptLine.RecId);
            Assert.Equal(6000m, initial.AmountMst);
            var invalid = await receipts.CorrectAsync(receipt.RecId, new(receiptLine.RecId, 70m, date.AddDays(2)), default);
            Assert.Equal(422, invalid.StatusCode);
            Assert.Equal(stockBefore.PhysicalInvent + 60m, (await summary.SingleAsync()).PhysicalInvent);
            var request = new ProductReceiptCorrectionDto(receiptLine.RecId, 50m, date.AddDays(2));
            Success(await receipts.CorrectAsync(receipt.RecId, request, default));
            Success(await receipts.CorrectAsync(receipt.RecId, request, default));
            var stockAfter = await summary.SingleAsync();
            Assert.Equal(stockBefore.PhysicalInvent + 50m, stockAfter.PhysicalInvent);
            Assert.Equal(stockBefore.PhysicalValue + 5000m, stockAfter.PhysicalValue);
            Assert.Equal(stockBefore.Ordered - 50m, stockAfter.Ordered);
            var versions = await db.Set<VendPackingSlipVersion>().AsNoTracking().Where(x => x.VendPackingSlipJour == receipt.RecId).OrderBy(x => x.VersionDateTime).ToListAsync();
            Assert.Equal(new[] { 60m, 50m }, versions.Select(x => x.Qty));
            var histories = await db.Set<VendPackingSlipTransHistory>().AsNoTracking().Where(x => x.DataAreaId == "dat" && x.VendPackingSlipTrans == receiptLine.RecId).OrderBy(x => x.ValidFrom).ToListAsync();
            Assert.Equal(new[] { 6000m, 5000m }, histories.Select(x => x.ValueMst));
            Assert.Equal(histories[0].ValidTo, histories[1].ValidFrom);
            var vouchers = versions.Select(x => x.LedgerVoucher).ToArray();
            var entryIds = await db.Set<GeneralJournalEntry>().Where(x => vouchers.Contains(x.SubledgerVoucher)).Select(x => x.RecId).ToArrayAsync();
            Assert.Equal(2, entryIds.Length);
            var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == "dat" && entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();
            Assert.Equal(5000m, entries.Where(x => x.LedgerDimension == initial.InventoryLedgerDimension).Sum(x => x.AccountingCurrencyAmount));
            Assert.Equal(-5000m, entries.Where(x => x.LedgerDimension == initial.AccrualLedgerDimension).Sum(x => x.AccountingCurrencyAmount));
            Assert.Equal(vendorRowsBefore, await db.Set<VendTrans>().CountAsync(x => x.DataAreaId == "dat"));

            Success(await receipts.CancelAsync(receipt.RecId, date.AddDays(3), default));
            Success(await receipts.CancelAsync(receipt.RecId, date.AddDays(3), default));
            var canceledStock = await summary.SingleAsync();
            Assert.Equal(stockBefore.PhysicalInvent, canceledStock.PhysicalInvent);
            Assert.Equal(stockBefore.PhysicalValue, canceledStock.PhysicalValue);
            Assert.Equal(stockBefore.Ordered, canceledStock.Ordered);
            Assert.Equal(100m, (await db.Set<PurchLine>().AsNoTracking().SingleAsync(x => x.RecId == correctionLine.RecId)).RemainPurchPhysical);
            var canceledVersions = await db.Set<VendPackingSlipVersion>().AsNoTracking()
                .Where(x => x.VendPackingSlipJour == receipt.RecId).OrderBy(x => x.VersionDateTime).ToListAsync();
            Assert.Equal(new[] { 60m, 50m, 0m }, canceledVersions.Select(x => x.Qty));
            var finalHistory = await db.Set<VendPackingSlipTransHistory>().AsNoTracking().Where(x => x.DataAreaId == "dat" && x.VendPackingSlipTrans == receiptLine.RecId)
                .OrderByDescending(x => x.ValidFrom).FirstAsync();
            Assert.Equal(0m, finalHistory.Qty);
            Assert.Equal(0m, finalHistory.ValueMst);
            var cancellationVouchers = canceledVersions.Select(x => x.LedgerVoucher).ToArray();
            var allEntryIds = await db.Set<GeneralJournalEntry>().Where(x => cancellationVouchers.Contains(x.SubledgerVoucher)).Select(x => x.RecId).ToArrayAsync();
            Assert.Equal(3, allEntryIds.Length);
            var allEntries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == "dat" && allEntryIds.Contains(x.GeneralJournalEntry)).ToListAsync();
            Assert.All(allEntries.GroupBy(x => x.LedgerDimension), group => Assert.Equal(0m, group.Sum(x => x.AccountingCurrencyAmount)));
            Assert.Equal(vendorRowsBefore, await db.Set<VendTrans>().CountAsync(x => x.DataAreaId == "dat"));
        });
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
            // Prepayment 1,000 then the 1,500 remainder; tax-free vendor; advance 181005 nets to zero.
            "10" => (100m,2500m,0m,-2500m,-2500m,0m,0m,0m,0m,0m,0m,0m),
            // Received 100, corrected to 50, never invoiced: only the physical quantity remains.
            "17" => (50m,0m,0m,0m,0m,0m,0m,0m,0m,0m,0m,0m),
            "18" => (100m,2500m,375m,-2875m,-2875m,0m,0m,0m,0m,0m,2875m,2875m),
            "19" => (100m,2500m,375m,0m,0m,0m,0m,0m,0m,-2875m,0m,2875m),
            "02" or "05" or "06" or "09" or "11" => (100m,2500m,375m,-2875m,-2875m,0m,0m,0m,0m,0m,0m,0m),
            // Credit note 115 inclusive of 15% VAT, recovered into the bank. The expense and the
            // input tax reverse, the bank moves *in*, and the vendor control account nets to zero
            // because the credit debited it and the refund credits it back.
            "21" => (0m,0m,-15m,115m,115m,-100m,0m,0m,0m,0m,0m,0m),
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
            // Only 17 stops at the receipt: its corrected 50 x 25 stays accrued until invoiced.
            [GeneralLedgerSeedExample.ReceivedNotInvoicedInterim] = id == "17" ? 50 * 25m : 0m,
            [GeneralLedgerSeedExample.ReceivedNotInvoiced] = id == "17" ? -50 * 25m : 0m
        };
        foreach (var (account, amount) in accounts)
            Assert.True(amount == actual.LedgerAmount(account),
                $"Scenario {id}, account {account}: expected {amount}, actual {actual.LedgerAmount(account)} SAR.");
        Assert.All(actual.Entries.GroupBy(x => x.GeneralJournalEntry),
            voucher => Assert.Equal(0m, voucher.Sum(x => x.AccountingCurrencyAmount)));
    }

    private static ServiceProvider TransactionServices(ApplicationDbContext db)
    {
        var actor = new TestIdentity("cycle-test");
        return new ServiceCollection().AddSingleton(db).AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor).AddAccountsPayableTransactionExamples().BuildServiceProvider();
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

}

