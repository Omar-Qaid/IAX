using System.Reflection;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using SharedProduct = IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProduct;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryArchitectureTests
{
    [Fact]
    public void Journal_name_rejects_unsupported_policies()
    {
        var validator = new InventJournalNameInputValidator();
        var valid = new InventJournalNameInputDto
        { JournalNameId = "COUNT", Description = "Count", JournalType = InventJournalType.Counting };
        Assert.True(validator.Validate(valid).IsValid);
        // Summary posting is supported: the journal gets one voucher and one totalled ledger entry.
        valid.DetailSummary = DetailSummary.Summary;
        Assert.True(validator.Validate(valid).IsValid);
        valid.DetailSummary = DetailSummary.Detail;
        valid.DeletePostedLines = true;
        Assert.False(validator.Validate(valid).IsValid);
        valid.DeletePostedLines = false;
        valid.Reservation = ItemReservation.Automatic;
        Assert.False(validator.Validate(valid).IsValid);
    }

    [Fact]
    public async Task Inventory_readiness_rejects_blocked_voucher_sequence_on_selected_journal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new SysNumberSequence { RecId = 1, DataAreaId = "dat", NumberSequence = "VOUCHER", Txt = "Voucher",
            Format = "V-{SEQ}", AnnotatedFormat = "V-{SEQ}", InUse = 1, IsActive = true, Blocked = 1,
            NextRec = 1, Highest = 99999 });
        db.Add(new InventJournalName { RecId = 2, DataAreaId = "dat", JournalNameId = "MOV",
            Description = "Movement", JournalType = InventJournalType.Movement, VoucherNumberSequenceTable = 1 });
        db.Add(new InventParameters { RecId = 3, DataAreaId = "dat", MovementJournalNameId = "MOV" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventParametersService(new TestUnitOfWork(db), identity);
        var readiness = (await service.GetReadinessAsync())!;
        Assert.Contains(readiness.Issues, x => x.Code == "MISSING_MOVEMENT_JOURNAL");
        var journalNames = new InventJournalNameService(new TestUnitOfWork(db), identity, new InventJournalNameInputValidator());
        Assert.Empty((await journalNames.LookupsAsync(default)).NumberSequences);
    }

    [Fact]
    public async Task Inventory_readiness_requires_accrual_offsets_only_when_physical_posting_is_enabled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var model = new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO",
            InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes, PostOnHandFinancial = NoYes.Yes };
        db.Add(model);
        await db.SaveChangesAsync();
        var service = new InventParametersService(new TestUnitOfWork(db), new TestIdentity());

        var initial = (await service.GetReadinessAsync())!;
        Assert.Contains(initial.Issues, x => x.Code == "MISSING_SALESCONSUMP_ACCOUNT");
        Assert.DoesNotContain(initial.Issues, x => x.Code == "MISSING_PURCHPACKINGSLIPOFFSETACCOUNT_ACCOUNT"
            || x.Code == "MISSING_SALESPACKINGSLIPOFFSETACCOUNT_ACCOUNT");

        model.PostPhysicalPurchase = NoYes.Yes;
        model.PostPhysicalRevenue = NoYes.Yes;
        await db.SaveChangesAsync();
        var enabled = (await service.GetReadinessAsync())!;
        Assert.Contains(enabled.Issues, x => x.Code == "MISSING_PURCHPACKINGSLIPOFFSETACCOUNT_ACCOUNT");
        Assert.Contains(enabled.Issues, x => x.Code == "MISSING_SALESPACKINGSLIPOFFSETACCOUNT_ACCOUNT");

        db.AddRange(new[] { InventAccountType.SalesConsump, InventAccountType.PurchPackingSlipOffsetAccount,
            InventAccountType.SalesPackingSlipOffsetAccount }.Select((type, i) => new InventPosting
            { RecId = i + 10, DataAreaId = "dat", InventAccountType = type, ItemCode = TableGroupAll.All,
                CustVendCode = TableGroupAll.All, LedgerDimension = 1 }));
        await db.SaveChangesAsync();
        var configured = (await service.GetReadinessAsync())!;
        Assert.DoesNotContain(configured.Issues, x => x.Code is "MISSING_SALESCONSUMP_ACCOUNT"
            or "MISSING_PURCHPACKINGSLIPOFFSETACCOUNT_ACCOUNT" or "MISSING_SALESPACKINGSLIPOFFSETACCOUNT_ACCOUNT");
    }

    [Fact]
    public async Task Inventory_readiness_does_not_accept_a_model_group_requiring_unimplemented_receiving()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var model = new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO",
            InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes, PostOnHandFinancial = NoYes.Yes,
            MandatoryReceive = NoYes.Yes };
        db.Add(model);
        await db.SaveChangesAsync();
        var service = new InventParametersService(new TestUnitOfWork(db), new TestIdentity());
        Assert.Contains((await service.GetReadinessAsync())!.Issues, x => x.Code == "MISSING_MODEL_GROUP");
        model.MandatoryReceive = NoYes.No;
        await db.SaveChangesAsync();
        Assert.DoesNotContain((await service.GetReadinessAsync())!.Issues, x => x.Code == "MISSING_MODEL_GROUP");
    }

    [Fact]
    public async Task Inventory_readiness_accepts_valid_setup_with_company_chosen_names()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new UnitOfMeasure { RecId = 1, Symbol = "PCS" },
            new EcoResStorageDimensionGroup { RecId = 2, Name = "STOCK" },
            new EcoResStorageDimensionGroupFldSetup { RecId = 3, StorageDimensionGroup = 2,
                DimensionFieldId = InventDimensionFields.Site, IsActive = true,
                IsPhysicalInventoryEnabled = true, IsFinancialInventoryEnabled = true },
            new EcoResStorageDimensionGroupFldSetup { RecId = 4, StorageDimensionGroup = 2,
                DimensionFieldId = InventDimensionFields.Warehouse, IsActive = true,
                IsPhysicalInventoryEnabled = true },
            new EcoResTrackingDimensionGroup { RecId = 5, Name = "UNTRACKED" },
            new InventItemGroup { RecId = 6, DataAreaId = "dat", ItemGroupId = "GOODS", Name = "Goods" },
            new InventModelGroup { RecId = 7, DataAreaId = "dat", ModelGroupId = "AVG",
                InventModel = InventModel.WeightedAvg, StockedProduct = NoYes.Yes,
                PostOnHandFinancial = NoYes.Yes });
        await db.SaveChangesAsync();
        var readiness = (await new InventParametersService(new TestUnitOfWork(db), new TestIdentity())
            .GetReadinessAsync())!;

        Assert.DoesNotContain(readiness.Issues, x => x.Code is "MISSING_INVENTORY_UNIT" or
            "MISSING_BASIC_STORAGE_GROUP" or "MISSING_BASIC_TRACKING_GROUP" or
            "MISSING_ITEM_GROUP" or "MISSING_MODEL_GROUP");
    }

    [Fact]
    public async Task Summary_journal_posting_uses_one_voucher_and_one_totalled_ledger_entry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = DateTime.Today;
        db.AddRange(
            new InventJournalTable
            {
                RecId = 1, DataAreaId = "dat", JournalId = "J-1", JournalNameId = "MOV",
                JournalType = InventJournalType.Movement, VoucherNumberSequenceTable = 4,
                DetailSummary = DetailSummary.Summary
            },
            // Two receipts on the same accounts: summary posting must total them into one entry.
            new InventJournalTrans
            {
                RecId = 2, DataAreaId = "dat", JournalId = "J-1", JournalType = InventJournalType.Movement,
                ItemId = "ITEM-1", InventDimId = "DIM-1", Unit = "pcs", Qty = 2, CostPrice = 10,
                TransDate = date, LedgerDimension = 18
            },
            new InventJournalTrans
            {
                RecId = 17, DataAreaId = "dat", JournalId = "J-1", JournalType = InventJournalType.Movement,
                ItemId = "ITEM-1", InventDimId = "DIM-1", Unit = "pcs", Qty = 3, CostPrice = 10,
                TransDate = date, LedgerDimension = 18
            },
            new InventJournalName { RecId = 3, DataAreaId = "dat", JournalNameId = "MOV", JournalType = InventJournalType.Movement, VoucherNumberSequenceTable = 4, DetailSummary = DetailSummary.Summary },
            new SysNumberSequence { RecId = 4, DataAreaId = "dat", NumberSequence = "INV-VOUCHER", Txt = "Inventory voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new ReleasedItem { RecId = 5, DataAreaId = "dat", ItemId = "ITEM-1", Product = 6 },
            new InventTableModule { RecId = 16, DataAreaId = "dat", ItemId = "ITEM-1", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs" },
            new InventModelGroup { RecId = 7, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes },
            new InventModelGroupItem { RecId = 8, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 9, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", TrackingDimensionGroup = 10 },
            new EcoResTrackingDimensionGroup { RecId = 10, Name = "No tracking" },
            new InventDim { RecId = 11, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new Ledger { RecId = 12, DataAreaId = "dat", FiscalCalendar = 13, AccountingCurrency = "SAR", ReportingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 14, DataAreaId = "dat", FiscalCalendar = 13, StartDate = date.AddDays(-1), EndDate = date.AddDays(1), Type = FiscalPeriodType.Operating },
            new LedgerFiscalCalendarPeriod { RecId = 15, DataAreaId = "dat", Ledger = 12, FiscalCalendarPeriod = 14, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 19, DataAreaId = "dat", MainAccountId = "130100" },
            new MainAccount { RecId = 20, DataAreaId = "dat", MainAccountId = "510100" },
            new DimensionAttributeValueCombination { RecId = 21, DataAreaId = "dat", MainAccount = 19, DisplayValue = "130100" },
            new DimensionAttributeValueCombination { RecId = 18, DataAreaId = "dat", MainAccount = 20, DisplayValue = "510100" },
            new InventPosting { RecId = 22, DataAreaId = "dat", InventAccountType = InventAccountType.InventReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 21 });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var unitOfWork = new TestUnitOfWork(db);
        var posting = new InventJournalPostingService(unitOfWork, identity, identity,
            new SysNumberSequenceService(unitOfWork, identity), new TestSalesInventoryNumbers());

        var result = await posting.PostAsync("J-1", default);

        Assert.Null(result.Error);
        Assert.Equal(2, result.Result!.InventoryTransactionsCreated);   // stock still moves per line
        Assert.Equal(1, result.Result.LedgerEntriesCreated);            // but the ledger is one entry
        var voucher = Assert.Single(result.Result.Vouchers);
        Assert.Equal(new[] { voucher, voucher },
            await db.Set<InventJournalTrans>().OrderBy(x => x.RecId).Select(x => x.Voucher).ToArrayAsync());

        Assert.Single(await db.Set<GeneralJournalEntry>().ToListAsync());
        var ledgerLines = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(2, ledgerLines.Count);                             // one per account, not per line
        Assert.Equal(0m, ledgerLines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == 50m);   // 2 + 3 units at 10
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == -50m);
        // The per-line record of the financial update survives summarization.
        Assert.Equal(2, await db.Set<IAX.IXApi.Modules.Finance.Entities.InventTransPosting>().CountAsync());
        var repeated = await posting.PostAsync("J-1", default);
        Assert.Null(repeated.Error);
        Assert.Equal(result.Result.Vouchers, repeated.Result!.Vouchers);
        Assert.Equal(2, await db.Set<InventTrans>().CountAsync());
        Assert.Single(await db.Set<GeneralJournalEntry>().ToListAsync());
        db.AddRange(
            new InventJournalTable { RecId = 30, DataAreaId = "dat", JournalId = "J-COUNT", JournalNameId = "COUNT",
                JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 4,
                DetailSummary = DetailSummary.Summary, CountingFrozen = true, CountingConfirmed = true },
            new InventJournalName { RecId = 31, DataAreaId = "dat", JournalNameId = "COUNT",
                JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 4,
                DetailSummary = DetailSummary.Summary },
            new InventJournalTrans { RecId = 32, DataAreaId = "dat", JournalId = "J-COUNT",
                JournalType = InventJournalType.Counting, ItemId = "ITEM-1", InventDimId = "DIM-1",
                Unit = "pcs", Qty = 0, UnitQty = 0, InventOnHand = 5, Counted = 5, TransDate = date });
        await db.SaveChangesAsync();
        var nextVoucher = (await db.Set<SysNumberSequence>().SingleAsync()).NextRec;
        var noVariance = await posting.PostAsync("J-COUNT", default);
        Assert.Null(noVariance.Error);
        Assert.Empty(noVariance.Result!.Vouchers);
        Assert.Equal(0, noVariance.Result.InventoryTransactionsCreated);
        Assert.Equal(0, noVariance.Result.LedgerEntriesCreated);
        Assert.Equal(nextVoucher, (await db.Set<SysNumberSequence>().SingleAsync()).NextRec);
    }

    [Fact]
    public async Task Journal_posting_rejects_missing_inventory_posting_accounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventJournalTable { RecId = 1, DataAreaId = "dat", JournalId = "J-1", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 4 },
            new InventJournalTrans { RecId = 2, DataAreaId = "dat", JournalId = "J-1", JournalType = InventJournalType.Adjustment, ItemId = "ITEM-1", InventDimId = "DIM-1", Unit = "pcs", Qty = 1, CostPrice = 10 },
            new InventJournalName { RecId = 3, DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 4 },
            new SysNumberSequence { RecId = 4, DataAreaId = "dat", NumberSequence = "INV-VOUCHER", Txt = "Inventory voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new ReleasedItem { RecId = 5, DataAreaId = "dat", ItemId = "ITEM-1", Product = 6 },
            new InventTableModule { RecId = 16, DataAreaId = "dat", ItemId = "ITEM-1", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs" },
            new InventModelGroup { RecId = 7, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes },
            new InventModelGroupItem { RecId = 8, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 9, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", TrackingDimensionGroup = 10 },
            new EcoResTrackingDimensionGroup { RecId = 10, Name = "No tracking" },
            new InventDim { RecId = 11, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new Ledger { RecId = 12, DataAreaId = "dat", FiscalCalendar = 13, AccountingCurrency = "SAR", ReportingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 14, DataAreaId = "dat", FiscalCalendar = 13, StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(1), Type = FiscalPeriodType.Operating },
            new LedgerFiscalCalendarPeriod { RecId = 15, DataAreaId = "dat", Ledger = 12, FiscalCalendarPeriod = 14, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var unitOfWork = new TestUnitOfWork(db);
        var posting = new InventJournalPostingService(unitOfWork, identity, identity,
            new SysNumberSequenceService(unitOfWork, identity), new TestSalesInventoryNumbers());

        var result = await posting.PostAsync("J-1", default);

        Assert.Contains("Inventory posting setup is missing", result.Error);
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
        Assert.Empty(await db.Set<InventTransOrigin>().ToListAsync());
        Assert.False((await db.Set<InventJournalTable>().SingleAsync()).Posted == NoYes.Yes);
    }

    [Fact]
    public async Task Inventory_journal_names_are_company_scoped_and_protect_used_setup()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SysNumberSequence { RecId = 10, DataAreaId = "dat", NumberSequence = "InventJournalVoucher", Txt = "Inventory voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true },
            new SysNumberSequence { RecId = 11, DataAreaId = "other", NumberSequence = "Hidden", Txt = "Hidden", Format = "H-#####", AnnotatedFormat = "H-{SEQ}", InUse = 1, IsActive = true },
            new InventJournalName { RecId = 20, DataAreaId = "other", JournalNameId = "HIDDEN", Description = "Hidden", JournalType = InventJournalType.Movement });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventJournalNameService(new TestUnitOfWork(db), identity, new InventJournalNameInputValidator());

        var created = await service.CreateAsync(new InventJournalNameInputDto
        {
            JournalNameId = " mov ", Description = " Movement ", JournalType = InventJournalType.Movement,
            VoucherNumberSequenceTable = 10, DetailSummary = DetailSummary.Detail
        }, default);
        Assert.Null(created.Error);
        Assert.Equal("MOV", created.JournalName!.JournalNameId);
        Assert.Equal("InventJournalVoucher", created.JournalName.VoucherNumberSequenceCode);
        Assert.Single(await service.ListAsync(default));
        Assert.Single((await service.LookupsAsync(default)).NumberSequences);
        Assert.NotNull((await service.CreateAsync(new InventJournalNameInputDto
        { JournalNameId = "MOV", Description = "Duplicate", JournalType = InventJournalType.Movement }, default)).Error);

        db.Add(new InventJournalTable { RecId = 30, DataAreaId = "dat", JournalId = "J0001", JournalNameId = "MOV", Description = "Used" });
        await db.SaveChangesAsync();
        Assert.Equal(InventJournalNameDeleteResult.InUse, await service.DeleteAsync(created.JournalName.RecId, default));
        var changedType = await service.UpdateAsync(created.JournalName.RecId, new InventJournalNameInputDto
        { JournalNameId = "MOV", Description = "Movement", JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 10 }, default);
        Assert.Contains("cannot be changed", changedType.Error);
    }

    [Fact]
    public async Task Inventory_journal_lifecycle_uses_standard_tables_and_enforces_company_dimensions_and_type_rules()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SysNumberSequence { RecId = 1, DataAreaId = "dat", NumberSequence = "InventJournalTable", Txt = "Journal", Format = "IJ-#####", AnnotatedFormat = "IJ-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new SysNumberSequence { RecId = 51, DataAreaId = "dat", NumberSequence = "InventTransferJour", Txt = "Transfer voucher", Format = "TV-#####", AnnotatedFormat = "TV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new InventJournalName { RecId = 2, DataAreaId = "dat", JournalNameId = "TRF", Description = "Transfer", JournalType = InventJournalType.Transfer, VoucherNumberSequenceTable = 9 },
            new InventJournalName { RecId = 13, DataAreaId = "dat", JournalNameId = "MOV", Description = "Movement", JournalType = InventJournalType.Movement, VoucherNumberSequenceTable = 9 },
            new InventJournalName { RecId = 16, DataAreaId = "dat", JournalNameId = "COUNT", Description = "Counting", JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 9 },
            new InventJournalName { RecId = 3, DataAreaId = "other", JournalNameId = "HIDDEN", Description = "Hidden", JournalType = InventJournalType.Transfer },
            new InventJournalName { RecId = 14, DataAreaId = "dat", JournalNameId = "ADJ", Description = "Adjustment", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 9 },
            new InventSite { RecId = 4, DataAreaId = "dat", SiteId = "JED", Name = "Jeddah" },
            new InventLocation { RecId = 5, DataAreaId = "dat", InventLocationId = "MAIN", InventSiteId = "JED", Name = "Main" },
            new InventLocation { RecId = 6, DataAreaId = "dat", InventLocationId = "SHOW", InventSiteId = "JED", Name = "Showroom" },
            new InventLocation { RecId = 44, DataAreaId = "dat", InventLocationId = "OPEN", InventSiteId = "JED", Name = "Opening stock" },
            new InventLocation { RecId = 48, DataAreaId = "dat", InventLocationId = "TRANSIT", InventSiteId = "JED", Name = "In transit", InventLocationType = InventLocationType.Transit },
            new ReleasedItem { RecId = 7, DataAreaId = "dat", ItemId = "ITEM-001", Name = "Office Chair" },
            new UnitOfMeasure { RecId = 8, DataAreaId = "dat", Symbol = "pcs", DecimalPrecision = 0 },
            new UnitOfMeasure { RecId = 45, DataAreaId = "dat", Symbol = "box", DecimalPrecision = 0 },
            new UnitOfMeasureConversion { RecId = 46, DataAreaId = "dat", FromUnitOfMeasure = 45, ToUnitOfMeasure = 8,
                Product = 0, Factor = 4, Numerator = 1, Denominator = 1, Rounding = UnitOfMeasureRounding.Nearest },
            new SysNumberSequence { RecId = 9, DataAreaId = "dat", NumberSequence = "InventJournalVoucher", Txt = "Voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new SysNumberSequence { RecId = 12, DataAreaId = "dat", NumberSequence = "InventTransferJour", Txt = "Transfer journal", Format = "TRFJ-#####", AnnotatedFormat = "TRFJ-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new InventModelGroup { RecId = 10, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes, PostOnHandFinancial = NoYes.Yes },
            new InventModelGroupItem { RecId = 11, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 40, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", TrackingDimensionGroup = 41 },
            new EcoResTrackingDimensionGroup { RecId = 41, Name = "No tracking" },
            new InventTableModule { RecId = 2, DataAreaId = "other", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "box" },
            new InventTableModule { RecId = 12, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs", Price = 20, PriceUnit = 1 },
            new InventItemGroupItem { RecId = 15, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", ItemGroupId = "FURN", ItemGroupDataAreaId = "dat" },
            new MainAccount { RecId = 20, DataAreaId = "dat", MainAccountId = "11050001", Name = "Inventory", CurrencyCode = "SAR" },
            new MainAccount { RecId = 21, DataAreaId = "dat", MainAccountId = "51010001", Name = "Inventory profit", CurrencyCode = "SAR" },
            new DimensionAttributeValueCombination { RecId = 22, DataAreaId = "dat", MainAccount = 20, MainAccountValue = "11050001", DisplayValue = "11050001" },
            new DimensionAttributeValueCombination { RecId = 23, DataAreaId = "dat", MainAccount = 21, MainAccountValue = "51010001", DisplayValue = "51010001" },
            new DimensionAttributeValueCombination { RecId = 50, DataAreaId = "other", MainAccountValue = "11050001", DisplayValue = "11050001" },
            new InventPosting { RecId = 24, DataAreaId = "dat", InventAccountType = InventAccountType.InventReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 22 },
            new InventPosting { RecId = 25, DataAreaId = "dat", InventAccountType = InventAccountType.InventProfit, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 23 },
            new InventPosting { RecId = 42, DataAreaId = "dat", InventAccountType = InventAccountType.InventIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 22 },
            new InventPosting { RecId = 43, DataAreaId = "dat", InventAccountType = InventAccountType.InventLoss, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 23 },
            new Ledger { RecId = 26, DataAreaId = "dat", Name = "SA01 ledger", Description = "Ledger", FiscalCalendar = 27, AccountingCurrency = "SAR", ReportingCurrency = "SAR" },
            new FiscalCalendarYear { RecId = 28, DataAreaId = "dat", FiscalCalendar = 27, Name = "Current", StartDate = DateTime.Today.AddMonths(-6), EndDate = DateTime.Today.AddMonths(6) },
            new FiscalCalendarPeriod { RecId = 29, DataAreaId = "dat", FiscalCalendar = 27, FiscalCalendarYear = 28, Name = "Current", ShortName = "CUR", Type = FiscalPeriodType.Operating, StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddDays(10) },
            new LedgerFiscalCalendarPeriod { RecId = 30, DataAreaId = "dat", Ledger = 26, FiscalCalendarPeriod = 29, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var uow = new TestUnitOfWork(db);
        var numbers = new SysNumberSequenceService(uow, identity);
        var dimensions = new InventDimensionResolver(uow, new TestSalesInventoryNumbers());
        var service = new InventJournalService(uow, identity, numbers, dimensions);

        var created = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "trf", Description = "Warehouse transfer", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        Assert.Null(created.Error);
        Assert.Equal("TRF", created.Journal!.JournalNameId);
        Assert.Equal(InventJournalType.Transfer, created.Journal.JournalType);
        Assert.DoesNotContain((await service.LookupsAsync(default)).JournalNames, x => x.Code == "HIDDEN");
        var accountLookup = await service.LedgerDimensionsAsync("11050001", default);
        Assert.Contains(accountLookup, x => x.Code == "22" && x.Description == "11050001");
        Assert.DoesNotContain(accountLookup, x => x.Code == "50");

        var sameDimension = await service.AddLineAsync(created.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", ToInventSiteId = "JED", ToInventLocationId = "MAIN", Unit = "pcs", Qty = 1 }, default);
        Assert.Contains("must be different", sameDimension.Error);
        var added = await service.AddLineAsync(created.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", ToInventSiteId = "JED", ToInventLocationId = "SHOW", Unit = "pcs", Qty = 2 }, default);
        Assert.Null(added.Error);
        var line = Assert.Single(added.Journal!.Lines);
        Assert.Equal("MAIN", line.InventLocationId);
        Assert.Equal("SHOW", line.ToInventLocationId);
        Assert.Equal(2m, line.UnitQty);
        Assert.Equal(1, added.Journal.NumOfLines);
        var posting = new InventJournalPostingService(uow, identity, identity, numbers, new TestSalesInventoryNumbers());
        var openingMain = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Initial on-hand", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var openingMainLine = await service.AddLineAsync(openingMain.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 5,
            CostPrice = 0, LedgerDimension = 22, TransDate = DateTime.Today }, default);
        Assert.Null(openingMainLine.Error);
        Assert.Equal(20m, Assert.Single(openingMainLine.Journal!.Lines).CostPrice);
        Assert.Equal(5m, Assert.Single(openingMainLine.Journal.Lines).UnitQty);
        Assert.Null((await posting.PostAsync(openingMain.Journal.JournalId, default)).Error);
        Assert.Equal(5, (await db.Set<InventSum>().SingleAsync(x => x.DataAreaId == "dat" && x.InventLocationId == "MAIN")).PhysicalInvent);

        var alternateUnitJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Unit conversion", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var alternateUnitLine = await service.AddLineAsync(alternateUnitJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "box", UnitQty = 1,
            LedgerDimension = 22 }, default);
        Assert.Null(alternateUnitLine.Error);
        Assert.Equal(4m, Assert.Single(alternateUnitLine.Journal!.Lines).Qty);
        Assert.Equal(1m, Assert.Single(alternateUnitLine.Journal.Lines).UnitQty);

        var posted = await posting.PostAsync(created.Journal.JournalId, default);
        Assert.Null(posted.Error);
        Assert.True(posted.Result!.Posted);
        Assert.Equal(2, posted.Result.InventoryTransactionsCreated);
        Assert.Equal(0, posted.Result.LedgerEntriesCreated);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        var transferTransactions = await db.Set<InventTrans>()
            .Where(x => x.ItemId == "ITEM-001" && x.Voucher == posted.Result.Vouchers.Single())
            .ToListAsync();
        transferTransactions = transferTransactions.OrderBy(x => x.Qty).ToList();
        Assert.Equal(new decimal[] { -2, 2 }, transferTransactions.Select(x => x.Qty));
        Assert.Equal(0, transferTransactions.Sum(x => x.Qty));
        Assert.Equal(0, transferTransactions.Sum(x => x.CostAmountPosted));
        Assert.Equal(3, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        Assert.Equal(2, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.ToInventDimId)).PhysicalInvent);
        var repeated = await posting.PostAsync(created.Journal.JournalId, default);
        Assert.Null(repeated.Error);
        Assert.Equal(posted.Result.Vouchers, repeated.Result!.Vouchers);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        Assert.False((await service.DeleteAsync(created.Journal.JournalId, default)).Deleted);

        var excessive = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "TRF", Description = "Too much stock", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        for (var i = 0; i < 2; i++)
        {
            var draft = await service.AddLineAsync(excessive.Journal!.JournalId, new InventJournalLineInputDto
            { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", ToInventSiteId = "JED",
                ToInventLocationId = "SHOW", Unit = "pcs", Qty = 2, TransDate = DateTime.Today }, default);
            Assert.Null(draft.Error);
        }
        Assert.Contains("exceed available stock", (await posting.PostAsync(excessive.Journal!.JournalId, default)).Error);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(3, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        var adjustment = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Opening adjustment", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var adjusted = await service.AddLineAsync(adjustment.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 3, CostPrice = 10, TransDate = DateTime.Today }, default);
        Assert.Null(adjusted.Error);
        Assert.Equal(line.InventDimId, Assert.Single(adjusted.Journal!.Lines).InventDimId);
        var adjustmentPost = await posting.PostAsync(adjustment.Journal.JournalId, default);
        Assert.Null(adjustmentPost.Error);
        Assert.Equal(1, adjustmentPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, adjustmentPost.Result.LedgerEntriesCreated);
        var ledgerEntry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.JournalNumber == adjustment.Journal!.JournalId);
        var accountEntries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == ledgerEntry.RecId).ToListAsync();
        Assert.Equal(2, accountEntries.Count);
        Assert.Equal(0, accountEntries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(30, accountEntries.Single(x => x.PostingType == LedgerPostingType.InventReceipt).AccountingCurrencyAmount);
        Assert.Equal(6, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        // D365 initializes basic opening stock through a movement journal; this dimension starts with zero on-hand.
        var opening = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Opening inventory", InventSiteId = "JED", InventLocationId = "OPEN" }, default);
        var openingLine = await service.AddLineAsync(opening.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "OPEN", Unit = "pcs", Qty = 4,
            CostPrice = 25, LedgerDimension = 22, TransDate = DateTime.Today }, default);
        Assert.Null(openingLine.Error);
        var openingPost = await posting.PostAsync(opening.Journal.JournalId, default);
        Assert.Null(openingPost.Error);
        Assert.Equal(1, openingPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, openingPost.Result.LedgerEntriesCreated);
        Assert.Equal(4, await db.Set<InventTrans>().Where(x => x.InventDimId == Assert.Single(openingLine.Journal!.Lines).InventDimId)
            .Select(x => x.Qty).SingleAsync());
        Assert.Equal(4, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001" && x.InventLocationId == "OPEN")).PhysicalInvent);

        var issue = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Issue at running average", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var issueLine = await service.AddLineAsync(issue.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = -1, CostPrice = 999, TransDate = DateTime.Today }, default);
        Assert.Null(issueLine.Error);
        var issuePost = await posting.PostAsync(issue.Journal.JournalId, default);
        Assert.Null(issuePost.Error);
        Assert.Equal(999, Assert.Single(issueLine.Journal!.Lines).CostPrice);
        // FIFO uses the same-site provisional average across all three warehouses until close.
        Assert.Equal(-(230m / 12m),
            await db.Set<InventTrans>().OrderByDescending(x => x.RecId).Select(x => x.CostAmountPosted).FirstAsync());
        Assert.Equal(5, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        var movement = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Movement receipt", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var movementLine = await service.AddLineAsync(movement.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 2, CostPrice = 20,
            LedgerDimension = 22, TransDate = DateTime.Today }, default);
        Assert.Null(movementLine.Error);
        var movementPost = await posting.PostAsync(movement.Journal.JournalId, default);
        Assert.Null(movementPost.Error);
        Assert.Equal(1, movementPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, movementPost.Result.LedgerEntriesCreated);
        Assert.Equal(7, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        var counting = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "COUNT", Description = "Count difference", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var countLine = await service.AddLineAsync(counting.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Counted = 6,
            TransDate = DateTime.Today }, default);
        Assert.Null(countLine.Error);
        var duplicateCount = await service.AddLineAsync(counting.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Counted = 0,
            TransDate = DateTime.Today }, default);
        Assert.Contains("already in the counting journal", duplicateCount.Error);
        var countPost = await posting.PostAsync(counting.Journal.JournalId, default);
        Assert.Contains("durable movement freeze", countPost.Error);
        Assert.Null(countPost.Result);
        Assert.Equal(7, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        Assert.Equal(await db.Set<InventTrans>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.Qty),
            await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.Equal(0, await db.Set<GeneralJournalAccountEntry>().SumAsync(x => x.AccountingCurrencyAmount));

        Assert.Null(await posting.FreezeCountAsync(counting.Journal.JournalId, default));
        db.ChangeTracker.Clear(); // Freeze must survive a new request, not just tracked state.
        Assert.NotNull(await InventoryCountProtection.CheckAsync(db, "dat", "ITEM-001", line.InventDimId, default));
        Assert.NotNull((await service.DeleteAsync(counting.Journal.JournalId, default)).Error);
        var blockedJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Blocked during count", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        Assert.Null((await service.AddLineAsync(blockedJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1, CostPrice = 10, TransDate = DateTime.Today }, default)).Error);
        Assert.Contains("frozen for counting", (await posting.PostAsync(blockedJournal.Journal.JournalId, default)).Error);
        var frozenTransfers = new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferOrderService(
            uow, identity, dimensions, new TestSalesInventoryNumbers(), numbers);
        var frozenTransferInput = new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferCreateDto(
            "TR-FROZEN", "JED", "MAIN", "TRANSIT", "SHOW", DateTime.Today, DateTime.Today,
            [new("ITEM-001", "pcs", 1m)]);
        Assert.Null((await frozenTransfers.CreateAsync(frozenTransferInput, default)).Error);
        var beforeFrozenTransfer = await db.Set<InventTrans>().CountAsync();
        Assert.Contains("frozen for counting", (await frozenTransfers.ShipAsync("TR-FROZEN",
            new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferPostDto("BLOCKED", DateTime.Today,
                new Dictionary<decimal, decimal> { [1m] = 1m }), default)).Error);
        Assert.Equal(beforeFrozenTransfer, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(7m, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        var frozenLine = await db.Set<InventJournalTrans>().SingleAsync(x => x.JournalId == counting.Journal.JournalId);
        Assert.Null(await posting.ConfirmCountAsync(counting.Journal.JournalId, new Dictionary<long, decimal> { [frozenLine.RecId] = 10m }, default));
        var confirmedLine = await db.Set<InventJournalTrans>().SingleAsync(x => x.RecId == frozenLine.RecId);
        Assert.Equal(3m, confirmedLine.Qty);
        Assert.Equal(3m, confirmedLine.UnitQty);
        Assert.NotNull(await posting.ConfirmCountAsync(counting.Journal.JournalId, new Dictionary<long, decimal> { [frozenLine.RecId] = 11m }, default));
        var confirmedCount = await posting.PostAsync(counting.Journal.JournalId, default);
        Assert.Null(confirmedCount.Error);
        Assert.Equal(3m, (await db.Set<InventJournalTrans>().SingleAsync(x => x.RecId == frozenLine.RecId)).UnitQty);
        Assert.Equal(10m, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        var countTransactions = await db.Set<InventTrans>().CountAsync();
        Assert.Null((await posting.PostAsync(counting.Journal.JournalId, default)).Error);
        Assert.Equal(countTransactions, await db.Set<InventTrans>().CountAsync());
        Assert.Null(await InventoryCountProtection.CheckAsync(db, "dat", "ITEM-001", line.InventDimId, default));

        var transfers = new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferOrderService(
            uow, identity, dimensions, new TestSalesInventoryNumbers(), numbers);
        var transferInput = new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferCreateDto(
            "TR-DEMO", "JED", "MAIN", "TRANSIT", "SHOW", DateTime.Today, DateTime.Today,
            [new("ITEM-001", "pcs", 2m)]);
        var destinationBefore = await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.PhysicalInvent);
        var destinationOrderedBefore = await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.Ordered);
        Assert.Null((await transfers.CreateAsync(transferInput, default)).Error);
        var orderedLegs = await db.Set<InventTrans>().Where(x => x.Voucher == "TR-DEMO:ORDER").ToListAsync();
        Assert.Equal(2, orderedLegs.Count);
        Assert.Single(orderedLegs, x => x.Qty == -2m && x.StatusIssue == StatusIssue.Ordered);
        Assert.Single(orderedLegs, x => x.Qty == 2m && x.StatusReceipt == StatusReceipt.Ordered);
        Assert.Equal(destinationOrderedBefore + 2m, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001"
            && x.InventLocationId == "SHOW")).Ordered);
        // A transfer identifier belongs to one order.
        Assert.NotNull((await transfers.CreateAsync(transferInput, default)).Error);
        // Source, transit and destination must differ.
        Assert.NotNull((await transfers.CreateAsync(transferInput with { TransferId = "TR-BAD", InventLocationIdTo = "MAIN" }, default)).Error);
        // Nothing is in transit yet, so there is nothing to receive.
        Assert.NotNull((await transfers.ReceiveAsync("TR-DEMO", Post("EARLY", 1m), default)).Error);

        // The posting voucher is allocated from the transfer journal number sequence, so it is
        // read back from the posted InventTransferJour rather than composed from the request ID.
        async Task<string> PostedVoucher(string transferId, InventTransferUpdateType type, string requestId) =>
            await db.Set<InventTransferJour>().Where(x => x.TransferId == transferId
                && x.UpdateType == type && x.PostingRequestId == requestId).Select(x => x.Voucher).SingleAsync();

        Assert.Null((await transfers.ShipAsync("TR-DEMO", Post("SHIP1", 1m), default)).Error);
        Assert.Contains("other transfer operation", (await transfers.ReceiveAsync("TR-DEMO", Post("SHIP1", 1m), default)).Error);
        var ship1Voucher = await PostedVoucher("TR-DEMO", InventTransferUpdateType.Shipment, "SHIP1");
        var shippedCount = await db.Set<InventTrans>().CountAsync();
        var shipmentLegs = await db.Set<InventTrans>().Where(x => x.Voucher == ship1Voucher).ToListAsync();
        Assert.Equal(4, shipmentLegs.Count);
        Assert.Single(shipmentLegs, x => x.StatusIssue == StatusIssue.ReservPhysical);
        Assert.Single(shipmentLegs, x => x.StatusReceipt == StatusReceipt.Ordered);
        Assert.Equal(1m, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001"
            && x.InventLocationId == "TRANSIT")).ReservPhysical);
        // The request identifier makes a repeated shipment a no-operation.
        Assert.Null((await transfers.ShipAsync("TR-DEMO", Post("SHIP1", 1m), default)).Error);
        Assert.Equal(shippedCount, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(1m, await db.Set<InventSum>().Where(x => x.InventLocationId == "TRANSIT").SumAsync(x => x.PhysicalInvent));
        var partlyShipped = (await transfers.GetAsync("TR-DEMO", default)).Order!;
        Assert.Equal(InventTransferStatus.Shipped, partlyShipped.TransferStatus);
        Assert.Equal(1m, partlyShipped.Lines[0].QtyShipped);
        Assert.Equal(1m, partlyShipped.Lines[0].QtyRemainShip);
        // Both shipment legs own an inventory transaction.
        Assert.NotEqual(string.Empty, partlyShipped.Lines[0].InventTransId);
        Assert.NotEqual(string.Empty, partlyShipped.Lines[0].InventTransIdTransitTo);

        var firstTransitCost = await db.Set<InventTrans>().Where(x => x.Voucher == ship1Voucher
                && x.Qty > 0 && x.StatusReceipt == StatusReceipt.Purchased)
            .Select(x => x.CostAmountPosted).SingleAsync();
        var laterReceipt = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Later receipt changes site average", InventSiteId = "JED", InventLocationId = "OPEN" }, default);
        Assert.Null((await service.AddLineAsync(laterReceipt.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "OPEN", Unit = "pcs", Qty = 1,
            CostPrice = 100, LedgerDimension = 22, TransDate = DateTime.Today }, default)).Error);
        Assert.Null((await posting.PostAsync(laterReceipt.Journal.JournalId, default)).Error);
        Assert.Null((await transfers.CreateAsync(transferInput with { TransferId = "TR-OTHER", Lines = [new("ITEM-001", "pcs", 1m)] }, default)).Error);
        Assert.Null((await transfers.ShipAsync("TR-OTHER", Post("SHIP", 1m), default)).Error);

        // A receipt cannot exceed the stock this order has in transit.
        Assert.NotNull((await transfers.ReceiveAsync("TR-DEMO", Post("OVER", 2m), default)).Error);
        var destinationValueBefore = await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.PostedValue);
        Assert.Null((await transfers.ReceiveAsync("TR-DEMO", Post("RECEIVE1", 1m), default)).Error);
        Assert.Contains("other transfer operation", (await transfers.ShipAsync("TR-DEMO", Post("RECEIVE1", 1m), default)).Error);
        var receive1Voucher = await PostedVoucher("TR-DEMO", InventTransferUpdateType.Receive, "RECEIVE1");
        Assert.Equal(shipmentLegs.Count, await db.Set<InventTrans>().CountAsync(x => x.Voucher == ship1Voucher || x.Voucher == receive1Voucher));
        Assert.Empty(await db.Set<InventTrans>().Where(x => x.Voucher == ship1Voucher
            && (x.StatusIssue == StatusIssue.ReservPhysical || x.StatusReceipt == StatusReceipt.Ordered)).ToListAsync());
        var destinationValueAfter = await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.PostedValue);
        Assert.Equal(firstTransitCost, destinationValueAfter - destinationValueBefore);
        Assert.Null((await transfers.ReceiveAsync("TR-OTHER", Post("RECEIVE", 1m), default)).Error);
        Assert.Null((await transfers.ShipAsync("TR-DEMO", Post("SHIP2", 1m), default)).Error);
        // The shipment is complete, so nothing more may be shipped.
        Assert.NotNull((await transfers.ShipAsync("TR-DEMO", Post("SHIP3", 1m), default)).Error);
        Assert.Null((await transfers.ReceiveAsync("TR-DEMO", Post("RECEIVE2", 0.5m), default)).Error);
        var ship2Voucher = await PostedVoucher("TR-DEMO", InventTransferUpdateType.Shipment, "SHIP2");
        Assert.Equal(0.5m, await db.Set<InventTrans>().Where(x => x.Voucher == ship2Voucher
            && x.StatusReceipt == StatusReceipt.Ordered).SumAsync(x => x.Qty));
        Assert.Equal(0.5m, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001"
            && x.InventLocationId == "TRANSIT")).ReservPhysical);
        Assert.Null((await transfers.ReceiveAsync("TR-DEMO", Post("RECEIVE3", 0.5m), default)).Error);
        var transactionsAfterFinalReceipt = await db.Set<InventTrans>().CountAsync();
        Assert.Null((await transfers.ReceiveAsync("TR-DEMO", Post("RECEIVE3", 0.5m), default)).Error);
        Assert.Equal(transactionsAfterFinalReceipt, await db.Set<InventTrans>().CountAsync());

        Assert.Equal(0m, await db.Set<InventSum>().Where(x => x.InventLocationId == "TRANSIT").SumAsync(x => x.PhysicalInvent));
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001"
            && x.InventLocationId == "TRANSIT")).ReservPhysical);
        Assert.Equal(destinationOrderedBefore, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001"
            && x.InventLocationId == "SHOW")).Ordered);
        var finished = (await transfers.GetAsync("TR-DEMO", default)).Order!;
        Assert.Equal(InventTransferStatus.Received, finished.TransferStatus);
        Assert.Equal(2m, finished.Lines[0].QtyReceived);
        Assert.Equal(0m, finished.Lines[0].QtyRemainReceive);
        Assert.Equal(InventTransferRemainStatus.Received, finished.Lines[0].RemainStatus);
        Assert.NotEqual(string.Empty, finished.Lines[0].InventTransIdTransitFrom);
        Assert.NotEqual(string.Empty, finished.Lines[0].InventTransIdReceive);
        // Two units from this order and one from the second order arrived at SHOW.
        Assert.Equal(destinationBefore + 3m, await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.PhysicalInvent));
        // Transfers never post through an inventory journal.
        Assert.False(await db.Set<InventJournalTable>().AnyAsync(x => x.Description!.Contains("TR-DEMO")));

        var duplicateItemLines = transferInput with { TransferId = "TR-TWO-LINES",
            Lines = [new("ITEM-001", "pcs", 1m), new("ITEM-001", "pcs", 1m)] };
        Assert.Null((await transfers.CreateAsync(duplicateItemLines, default)).Error);
        Assert.Null((await transfers.ShipAsync("TR-TWO-LINES", new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferPostDto(
            "SHIP", DateTime.Today, new Dictionary<decimal, decimal> { [1m] = 1m, [2m] = 1m }), default)).Error);
        Assert.Null((await transfers.ReceiveAsync("TR-TWO-LINES", new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferPostDto(
            "RECEIVE-2", DateTime.Today, new Dictionary<decimal, decimal> { [2m] = 1m }), default)).Error);
        var firstLinePendingOriginIds = await db.Set<InventTransOriginTransferTransitFrom>()
            .Where(x => x.TransferTransitFromId == "TR-TWO-LINES" && x.TransferTransitFromLineNum == 1m)
            .Select(x => x.InventTransOrigin).ToListAsync();
        var secondLinePendingOriginIds = await db.Set<InventTransOriginTransferTransitFrom>()
            .Where(x => x.TransferTransitFromId == "TR-TWO-LINES" && x.TransferTransitFromLineNum == 2m)
            .Select(x => x.InventTransOrigin).ToListAsync();
        Assert.Equal(-1m, await db.Set<InventTrans>().Where(x => firstLinePendingOriginIds.Contains(x.InventTransOrigin)
            && x.StatusIssue == StatusIssue.ReservPhysical).SumAsync(x => x.Qty));
        Assert.Equal(0m, await db.Set<InventTrans>().Where(x => secondLinePendingOriginIds.Contains(x.InventTransOrigin)
            && x.StatusIssue == StatusIssue.ReservPhysical).SumAsync(x => x.Qty));
        Assert.Null((await transfers.ReceiveAsync("TR-TWO-LINES", Post("RECEIVE-1", 1m), default)).Error);

        var pendingTransfer = transferInput with { TransferId = "TR-GUARDED", Lines = [new("ITEM-001", "pcs", 1m)] };
        Assert.Null((await transfers.CreateAsync(pendingTransfer, default)).Error);
        var releasedForTransfer = await db.Set<ReleasedItem>().SingleAsync(x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001");
        db.Add(new EcoResProductLifecycleState { DataAreaId = "dat", StateId = "HOLD-INV", BlockInventory = NoYes.Yes });
        releasedForTransfer.ProductLifecycleStateId = "HOLD-INV";
        await db.SaveChangesAsync();
        Assert.Contains("blocks inventory", (await transfers.CreateAsync(pendingTransfer with { TransferId = "TR-BLOCKED" }, default)).Error);
        Assert.Contains("blocks inventory", (await transfers.ShipAsync("TR-GUARDED", Post("LIFECYCLE", 1m), default)).Error);
        (await db.Set<ReleasedItem>().SingleAsync(x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001"))
            .ProductLifecycleStateId = string.Empty;
        await db.SaveChangesAsync();
        var closeBoundary = new InventClosing { DataAreaId = "dat", TransDate = DateTime.Today,
            AdjustmentType = InventAdjustmentType.Closing, InventCostStatus = InventCostStatus.Finished,
            Voucher = "CLOSE-TEST", PeriodCode = "CUR" };
        db.Add(closeBoundary);
        await db.SaveChangesAsync();
        Assert.Contains("financially closed", (await transfers.ShipAsync("TR-GUARDED", Post("CLOSED", 1m), default)).Error);
        db.Remove(closeBoundary);
        await db.SaveChangesAsync();

        static IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferPostDto Post(string requestId, decimal qty) =>
            new(requestId, DateTime.Today, new Dictionary<decimal, decimal> { [1m] = qty });

        var beforeRollbackTransactions = await db.Set<InventTrans>().CountAsync();
        var beforeRollbackEntries = await db.Set<GeneralJournalEntry>().CountAsync();
        var beforeRollbackQty = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent);
        var rollbackJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Atomic rollback test", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var rollbackReceipt = await service.AddLineAsync(rollbackJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1,
            CostPrice = 10, TransDate = DateTime.Today }, default);
        var invalidIssue = await service.AddLineAsync(rollbackJournal.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = -100,
            TransDate = DateTime.Today }, default);
        Assert.Null(rollbackReceipt.Error);
        Assert.Null(invalidIssue.Error);
        var rollbackResult = await posting.PostAsync(rollbackJournal.Journal.JournalId, default);
        Assert.Contains("available physical inventory", rollbackResult.Error);
        Assert.Equal(beforeRollbackTransactions, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(beforeRollbackEntries, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(beforeRollbackQty, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.False((await service.GetAsync(rollbackJournal.Journal.JournalId, default))!.Posted);

        (await db.Set<InventModelGroup>().SingleAsync()).InventModel = InventModel.StandardCost;
        await db.SaveChangesAsync();
        Assert.Contains("FIFO, LIFO, or weighted average", (await transfers.CreateAsync(
            transferInput with { TransferId = "TR-UNSUPPORTED", Lines = [new("ITEM-001", "pcs", 1m)] }, default)).Error);
        db.AddRange(
            new InventItemPrice { DataAreaId = "other", ItemId = "ITEM-001", InventDimId = line.InventDimId,
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 1000, PriceUnit = 1 },
            new InventItemPrice { DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "OTHER-DIM",
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 500, PriceUnit = 1 },
            new InventItemPrice { DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = string.Empty,
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 8, PriceUnit = 1 },
            new InventItemPrice { DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = line.InventDimId,
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 12, PriceUnit = 2 });
        await db.SaveChangesAsync();
        var standardJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Existing standard-cost item", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var standardLine = await service.AddLineAsync(standardJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1,
            CostPrice = 999, TransDate = DateTime.Today }, default);
        Assert.Null(standardLine.Error);
        var beforeVarianceTransactions = await db.Set<InventTrans>().CountAsync();
        var varianceRejected = await posting.PostAsync(standardJournal.Journal.JournalId, default);
        Assert.Contains("variance posting is not implemented", varianceRejected.Error);
        Assert.Equal(beforeVarianceTransactions, await db.Set<InventTrans>().CountAsync());
        var correctedLine = await service.UpdateLineAsync(standardJournal.Journal.JournalId,
            Assert.Single(standardLine.Journal!.Lines).RecId, new InventJournalLineInputDto
            { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1,
                CostPrice = 6, TransDate = DateTime.Today }, default);
        Assert.Null(correctedLine.Error);
        var standardPost = await posting.PostAsync(standardJournal.Journal.JournalId, default);
        Assert.Null(standardPost.Error);
        Assert.Equal(6, await db.Set<InventTrans>().Where(x => x.Voucher == standardPost.Result!.Vouchers.Single())
            .Select(x => x.CostAmountPosted).SingleAsync());

        var alternateUnitRecId = Assert.Single(alternateUnitLine.Journal!.Lines).RecId;
        var alternateUnitUpdated = await service.UpdateLineAsync(alternateUnitJournal.Journal!.JournalId,
            alternateUnitRecId, new InventJournalLineInputDto
            { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "box",
                UnitQty = 1, CostPrice = 6, LedgerDimension = 22 }, default);
        Assert.Null(alternateUnitUpdated.Error);
        var quantityBeforeAlternatePost = (await db.Set<InventSum>()
            .SingleAsync(x => x.DataAreaId == "dat" && x.InventLocationId == "MAIN")).PhysicalInvent;
        var alternateUnitPost = await posting.PostAsync(alternateUnitJournal.Journal.JournalId, default);
        Assert.Null(alternateUnitPost.Error);
        Assert.Equal(4m, await db.Set<InventTrans>()
            .Where(x => x.Voucher == alternateUnitPost.Result!.Vouchers.Single()).Select(x => x.Qty).SingleAsync());
        Assert.Equal(quantityBeforeAlternatePost + 4m, (await db.Set<InventSum>()
            .SingleAsync(x => x.DataAreaId == "dat" && x.InventLocationId == "MAIN")).PhysicalInvent);

        var releasedItem = await db.Set<ReleasedItem>().SingleAsync(x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001");
        releasedItem.Product = 100;
        db.Add(new UnitOfMeasureConversion { RecId = 47, DataAreaId = "dat", FromUnitOfMeasure = 45,
            ToUnitOfMeasure = 8, Product = 100, Factor = 2.2m, Numerator = 1, Denominator = 1,
            Rounding = UnitOfMeasureRounding.Up });
        await db.SaveChangesAsync();
        var roundedJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Product unit conversion", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var roundedLine = await service.AddLineAsync(roundedJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "box", UnitQty = 1,
            CostPrice = 6, LedgerDimension = 22 }, default);
        Assert.Null(roundedLine.Error);
        Assert.Equal(3m, Assert.Single(roundedLine.Journal!.Lines).Qty);

        db.Remove(await db.Set<InventModelGroupItem>().SingleAsync(x => x.ItemDataAreaId == "dat" && x.ItemId == "ITEM-001"));
        await db.SaveChangesAsync();
        var missingModelGroupJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Missing model group", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var missingModelGroupLine = await service.AddLineAsync(missingModelGroupJournal.Journal!.JournalId,
            new InventJournalLineInputDto { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN",
                Unit = "pcs", Qty = 1, CostPrice = 6, LedgerDimension = 22 }, default);
        Assert.Equal("Physical inventory journals require a stocked item model group.", missingModelGroupLine.Error);
        Assert.Contains("stocked item model group", (await transfers.CreateAsync(
            transferInput with { TransferId = "TR-NONSTOCK", Lines = [new("ITEM-001", "pcs", 1m)] }, default)).Error);
    }
}
