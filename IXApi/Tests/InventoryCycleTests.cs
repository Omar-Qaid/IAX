using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryCycleTests
{
    [Fact]
    public async Task Free_text_invoice_posts_services_tax_and_receivable_without_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<Currency>().Add(new Currency { RecId = 1, DataAreaId = "dat", CurrencyCode = "SAR" });
        db.Set<PaymTerm>().Add(new PaymTerm
        {
            RecId = 1, DataAreaId = "dat", PaymTermId = "NET30", Description = "30 days", NumOfDays = 30
        });
        db.Set<CustTable>().Add(new CustTable
        {
            RecId = 1, DataAreaId = "dat", AccountNum = "C-0001", InvoiceAccount = "C-0001",
            CustGroupId = "LOCAL", CurrencyCode = "SAR", PaymTermId = "NET30", TaxGroupId = "DOMESTIC"
        });
        db.Set<Ledger>().Add(new Ledger
        {
            RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", ReportingCurrency = "SAR", FiscalCalendar = 1
        });
        db.Set<FiscalCalendarPeriod>().Add(new FiscalCalendarPeriod
        {
            RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating,
            StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31)
        });
        db.Set<LedgerFiscalCalendarPeriod>().Add(new LedgerFiscalCalendarPeriod
        {
            RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open
        });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "410100" },
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "410200" },
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "210200" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "410100", DisplayValue = "410100" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "410200", DisplayValue = "410200" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, MainAccountValue = "210200", DisplayValue = "210200" });
        db.Set<CustLedgerAccounts>().Add(new CustLedgerAccounts
        {
            RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.Group,
            Num = "LOCAL", SummaryLedgerDimension = 1
        });
        db.Set<TaxGroupHeading>().Add(new TaxGroupHeading
        {
            RecId = 1, DataAreaId = "dat", TaxGroup = "DOMESTIC", TaxGroupName = "Domestic"
        });
        db.Set<TaxItemGroupHeading>().Add(new TaxItemGroupHeading
        {
            RecId = 1, DataAreaId = "dat", TaxItemGroup = "SERVICE", Name = "Service"
        });
        db.Set<TaxGroupData>().Add(new TaxGroupData
        {
            RecId = 1, DataAreaId = "dat", TaxGroup = "DOMESTIC", TaxCode = "VAT15"
        });
        db.Set<TaxOnItem>().Add(new TaxOnItem
        {
            RecId = 1, DataAreaId = "dat", TaxItemGroup = "SERVICE", TaxCode = "VAT15"
        });
        db.Set<TaxData>().Add(new TaxData
        {
            RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m,
            TaxFromDate = new DateTime(2026, 1, 1), TaxToDate = new DateTime(2026, 12, 31)
        });
        db.Set<TaxTable>().Add(new TaxTable
        {
            RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxAccountGroup = "VAT"
        });
        db.Set<TaxLedgerAccountGroup>().Add(new TaxLedgerAccountGroup
        {
            RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VAT", TaxOutgoingLedgerDimension = 4
        });
        await db.SaveChangesAsync();

        var service = new FreeTextInvoiceService(new TestUnitOfWork(db), new TestCompany(), Sequence());
        var created = await service.CreateAsync(new FreeTextInvoiceSaveDto
        {
            CustomerAccount = "C-0001",
            InvoiceDate = new DateTime(2026, 11, 1),
            PostingProfile = "GEN",
            Description = "November services",
            Lines =
            [
                new() { Description = "Maintenance", Quantity = 1, UnitPrice = 10000, LedgerDimension = 2, TaxItemGroup = "SERVICE" },
                new() { Description = "Consulting", Quantity = 4, UnitPrice = 500, LedgerDimension = 3, TaxItemGroup = "SERVICE" }
            ]
        }, default);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(12000m, created.Invoice!.NetAmount);
        Assert.Equal(1800m, created.Invoice.TaxAmount);
        Assert.Equal(new DateTime(2026, 12, 1), created.Invoice.DueDate);
        Assert.Equal(string.Empty, created.Invoice.InvoiceId);
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());

        var posted = await service.PostAsync(created.Invoice.RecId, default);

        Assert.Equal(200, posted.StatusCode);
        Assert.Equal("X-000001", posted.Invoice!.InvoiceId);
        Assert.Equal(13800m, posted.Invoice.InvoiceAmount);
        Assert.Equal(13800m, (await db.Set<CustTrans>().SingleAsync()).AmountCur);
        Assert.Equal(13800m, (await db.Set<CustTransOpen>().SingleAsync()).AmountCur);
        Assert.Equal(2, await db.Set<CustInvoiceTrans>().CountAsync());
        Assert.Equal(-1800m, (await db.Set<TaxTrans>().SingleAsync()).TaxAmountCur);
        Assert.Equal(new[] { -10000m, -2000m, -1800m, 13800m },
            (await db.Set<GeneralJournalAccountEntry>().Select(x => x.AccountingCurrencyAmount).ToListAsync())
                .OrderBy(x => x).ToArray());
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());

        var repeated = await service.PostAsync(created.Invoice.RecId, default);
        Assert.Equal("Free text invoice was already posted.", repeated.Message);
        Assert.Single(await db.Set<CustInvoiceJour>().ToListAsync());
        Assert.Single(await db.Set<CustTrans>().ToListAsync());
    }

    [Fact]
    public async Task Inventory_journal_creation_rejects_reused_sequence_without_changing_old_header()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Add(new InventJournalName { DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment });
        db.Add(new InventJournalTable { DataAreaId = "dat", JournalId = "X-000001", Description = "Original", Posted = NoYes.Yes });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var service = new InventJournalService(work, new TestCompany(), Sequence(),
            new InventDimensionResolver(work, new TestInventNumbers()));
        var result = await service.CreateAsync(new InventJournalCreateDto { JournalNameId = "ADJ", Description = "New example" }, default);
        Assert.Null(result.Journal);
        Assert.Contains("already exists", result.Error);
        var retained = await db.Set<InventJournalTable>().SingleAsync();
        Assert.Equal("Original", retained.Description);
        Assert.Equal(NoYes.Yes, retained.Posted);
    }

    [Fact]
    public async Task Vendor_transactions_are_read_only_and_company_scoped()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<VendTrans>().AddRange(
            new VendTrans { RecId = 1, DataAreaId = "dat", AccountNum = "V001", TransDate = new DateTime(2026, 1, 1), AmountCur = -100 },
            new VendTrans { RecId = 2, DataAreaId = "usmf", AccountNum = "V001", TransDate = new DateTime(2026, 1, 2), AmountCur = -200 });
        await fixture.Db.SaveChangesAsync();

        var service = new VendTransReadService(new TestUnitOfWork(fixture.Db), new TestCompany());
        var page = await service.SearchAsync(new VendTransQueryDto { AccountNum = " V001 " }, default);

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, Assert.Single(page.Rows).RecId);
        Assert.Null(await service.GetAsync(2, default));
        Assert.Equal(-100m, (await service.GetAsync(1, default))!.AmountCur);
    }

    [Fact]
    public async Task Vendor_return_request_validation_preserves_errors_before_posting()
    {
        await using var fixture = await Fixture.CreateAsync();
        var service = new PurchReturnService(new TestUnitOfWork(fixture.Db), new TestCompany(),
            Sequence(), new TestInventNumbers());

        var create = await service.CreateAsync(new PurchReturnCreateDto("VI-1", null, []), default);
        var ship = await service.ShipAsync(1, new PurchReturnShipDto("", null, []), default);
        var credit = await service.CreditAsync(1, new VendCreditNoteDto(""), default);

        Assert.Equal((400, "At least one return line is required."), (create.StatusCode, create.Message));
        Assert.Equal((400, "A return shipment number is required."), (ship.StatusCode, ship.Message));
        Assert.Equal((400, "A vendor credit note number is required."), (credit.StatusCode, credit.Message));
        Assert.Empty(await fixture.Db.Set<PurchTable>().ToListAsync());
    }

    // ---- Reservation engine (Phase 3 Ãƒâ€šÃ‚Â§7ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Å“8) --------------------------------------------------

    [Fact]
    public async Task Reserve_and_release_track_ownership_and_availability()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 100, ReservPhysical = 0, OnOrder = 30, AvailPhysical = 100 });
        db.Set<InventTransOrigin>().Add(new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "SO-L1", ItemId = "ITEM", ReferenceId = "SO1", ReferenceCategory = InventRefType.SalesTable });
        db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = -30, StatusIssue = StatusIssue.Ordered });
        await db.SaveChangesAsync();
        var service = new InventoryReservationService(new TestUnitOfWork(db), new TestCompany());

        // Reserve 20 of 30 demand against 100 on hand.
        var (reserved, error) = await service.ReserveAsync("SO-L1", 20m, false, default);
        Assert.Null(error);
        Assert.Equal(20m, reserved!.Reserved);
        Assert.Equal(80m, reserved.AvailablePhysical);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(20m, sum.ReservPhysical);
        Assert.Equal(10m, sum.OnOrder);
        Assert.Equal(80m, sum.AvailPhysical);
        Assert.Equal(20m, -(await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.ReservPhysical)).Qty);

        // Idempotent set-target: reserving the same 20 again is a no-op.
        var (same, _) = await service.ReserveAsync("SO-L1", 20m, false, default);
        Assert.Equal(20m, same!.Reserved);
        Assert.Equal(20m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        // Cannot reserve above remaining demand (30).
        Assert.NotNull((await service.ReserveAsync("SO-L1", 40m, false, default)).Error);

        // Release 5 ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ reserved 15, available 85.
        var (released, releaseError) = await service.ReleaseAsync("SO-L1", 5m, default);
        Assert.Null(releaseError);
        Assert.Equal(15m, released!.Reserved);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(15m, sum.ReservPhysical);
        Assert.Equal(15m, sum.OnOrder);
        Assert.Equal(85m, sum.AvailPhysical);
    }

    [Fact]
    public async Task Reserve_rejects_more_than_available_physical_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 10, ReservPhysical = 0, OnOrder = 30, AvailPhysical = 10 });
        db.Set<InventTransOrigin>().Add(new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "SO-L1", ItemId = "ITEM", ReferenceId = "SO1", ReferenceCategory = InventRefType.SalesTable });
        db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = -30, StatusIssue = StatusIssue.Ordered });
        await db.SaveChangesAsync();
        var service = new InventoryReservationService(new TestUnitOfWork(db), new TestCompany());

        Assert.NotNull((await service.ReserveAsync("SO-L1", 25m, false, default)).Error);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        // allowPartial reserves only what is available (10).
        var (partial, error) = await service.ReserveAsync("SO-L1", 25m, true, default);
        Assert.Null(error);
        Assert.Equal(10m, partial!.Reserved);
        Assert.Equal(0m, partial.AvailablePhysical);
    }

    // ---- FIFO inventory close (Phase 7 worked example: 1,600 COGS / 600 remaining) -----------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Close_excludes_balanced_transfer_order_movements_and_rejects_unbalanced_ones(bool balanced)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().AddRange(
            new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "SOURCE", InventSiteId = "S1", InventLocationId = "WH1" },
            new InventDim { RecId = 2, DataAreaId = "dat", InventDimId = "TRANSIT", InventSiteId = "S1", InventLocationId = "TRANSIT" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        db.Set<InventTransOrigin>().AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "R1", ItemId = "ITEM", ReferenceId = "IJ1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "T1", ItemId = "ITEM", ReferenceId = "TO1", ReferenceCategory = InventRefType.TransferOrder },
            new InventTransOrigin { RecId = 3, DataAreaId = "dat", InventTransId = "T2", ItemId = "ITEM", ReferenceId = "TO1", ReferenceCategory = InventRefType.TransferOrder });
        db.Set<InventTrans>().AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "SOURCE", InventTransOrigin = 1, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "SOURCE", InventTransOrigin = 2, Qty = -10, StatusIssue = StatusIssue.Sold, CostAmountPosted = -100, Voucher = "TO1:SHIP", DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        if (balanced)
            db.Set<InventTrans>().Add(new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "TRANSIT", InventTransOrigin = 3, Qty = 10, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100, Voucher = "TO1:SHIP", DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();

        var (result, error) = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30), default);
        if (balanced)
        {
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal(0, result.IssuesSettled);
            Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        }
        else
            Assert.Contains("transfer order must have balanced", error);
    }

    [Fact]
    public async Task Fifo_close_settles_issue_to_actual_fifo_cost_and_posts_adjustment()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        // Two financially posted receipts and one provisional-cost issue (running average 2200/200 = 11).
        db.Set<InventTransOrigin>().AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "R1", ItemId = "ITEM", ReferenceId = "IJ1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "R2", ItemId = "ITEM", ReferenceId = "IJ2", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 3, DataAreaId = "dat", InventTransId = "I1", ItemId = "ITEM", ReferenceId = "IJ3", ReferenceCategory = InventRefType.InventoryJournal });
        db.Set<InventTrans>().AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 2, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1200, DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3, Qty = -150, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1650, Voucher = "ISSUE", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 50, PostedValue = 550 });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", Voucher = "ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue,
            PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(),
            new TestUser(), Sequence());
        var (result, error) = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30), default);

        Assert.Null(error);
        Assert.Equal(1, result!.IssuesSettled);
        Assert.Equal(2, result.SettlementsCreated);
        Assert.Equal(50m, result.TotalAdjustment);
        Assert.Equal("ITEM", (await db.Set<InventClosing>().SingleAsync()).ScopeItemId);
        Assert.NotNull(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", new DateTime(2026, 6, 30), default));
        Assert.Null(await InventoryFinancialBoundary.CheckAsync(db, "dat", "OTHER", new DateTime(2026, 6, 30), default));
        Assert.All(await db.Set<InventSettlement>().ToListAsync(), x => Assert.NotNull(x.ReceiptTransRecId));

        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(-1600m, issue.CostAmountPosted + issue.CostAmountAdjustment); // final FIFO COGS
        Assert.Equal(600m, (await db.Set<InventSum>().SingleAsync()).PostedValue); // remaining inventory value

        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(2, settlements.Count);
        Assert.Equal(-1600m, settlements.Sum(x => x.CostAmountSettled));
        Assert.Equal(-150m, settlements.Sum(x => x.QtySettled));

        var closing = await db.Set<InventClosing>().SingleAsync();
        Assert.Equal(InventCostStatus.Finished, closing.InventCostStatus);

        var ledgerLines = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(2, ledgerLines.Count);                              // one balanced Dr/Cr pair
        Assert.Equal(0m, ledgerLines.Sum(x => x.AccountingCurrencyAmount)); // debits and credits balance
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == 50m);  // Dr Inventory +50
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == -50m); // Cr COGS -50
        var settlementCount = await db.Set<InventSettlement>().CountAsync();
        var companyClose = await service.CloseAsync(new DateTime(2026, 6, 30), default);
        Assert.Null(companyClose.Error); // A scoped example must not block subsequent company-wide close.
        Assert.Equal(0, companyClose.Result!.IssuesSettled);
        Assert.Equal(settlementCount, await db.Set<InventSettlement>().CountAsync());
    }

    [Fact]
    public async Task Fifo_close_is_idempotent_and_blocks_backdated_runs()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var first = await service.CloseAsync(new DateTime(2026, 6, 30), default);
        Assert.Null(first.Error);
        var second = await service.CloseAsync(new DateTime(2026, 6, 30), default); // same cutoff -> idempotent
        Assert.Null(second.Error);
        Assert.Equal(first.Result!.RunNum, second.Result!.RunNum);
        Assert.Single(await db.Set<InventClosing>().ToListAsync());

        var backdated = await service.CloseAsync(new DateTime(2026, 5, 31), default); // before last close
        Assert.NotNull(backdated.Error);
    }

    // ---- Purchase order Milestone 1 (Purchorder.md Ãƒâ€šÃ‚Â§12.2): PO 100 @ 50, receive/invoice 60 + 40 ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Purchase_order_partial_receipts_and_invoices_complete_the_cycle(bool taxable)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        if (taxable) SeedPurchaseTax(db);
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "Monthly", NumOfMonths = 1, PaymMethod = PaymMethod.Net });
        db.Set<VendTable>().Local.Single().PaymTermId = "Monthly";
        await db.SaveChangesAsync();
        var (orders, confirm, receipts, invoices) = PurchaseServices(db);

        var created = await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default);
        Assert.Equal(200, created.StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        var added = await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default);
        Assert.Equal(200, added.StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();

        // Step 1: approved PO -> ordered inbound 100, no physical stock.
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.Ordered);
        Assert.Equal(0m, sum.PhysicalInvent);
        Assert.Equal(StatusReceipt.Ordered, (await db.Set<InventTrans>().SingleAsync()).StatusReceipt);

        // Step 2: confirmation stores a version, no stock effect.
        Assert.Equal(200, (await confirm.ConfirmAsync(po.RecId, default)).StatusCode);
        Assert.Single(await db.Set<VendPurchOrderJour>().ToListAsync());
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);

        // Step 3: product receipt 60 -> physical 60, remain physical 40, remain financial 100.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default)).StatusCode);
        line = await db.Set<PurchLine>().SingleAsync();
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(40m, sum.Ordered);
        Assert.Equal(40m, line.RemainPurchPhysical);
        Assert.Equal(100m, line.RemainPurchFinancial);

        // Negative: invoice more than received, over-receipt, duplicate receipt number.
        Assert.Equal(422, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-X", new DateTime(2026, 10, 29), [new(line.RecId, 61)]), default)).StatusCode);
        Assert.Equal(422, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0009", null, [new(line.RecId, 41)]), default)).StatusCode);
        Assert.Equal(422, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", null, [new(line.RecId, 1)]), default)).StatusCode);

        // Step 4: invoice 60 -> no extra physical stock, financial value 3,000, remain financial 40.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 29), [new(line.RecId, 60)]), default)).StatusCode);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(3000m, sum.PostedValue);
        Assert.Equal(40m, (await db.Set<PurchLine>().SingleAsync()).RemainPurchFinancial);
        Assert.Equal(422, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", null, [new(line.RecId, 1)]), default)).StatusCode);

        // Steps 5-6: receive and invoice the remaining 40.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0002", new DateTime(2026, 10, 30), [new(line.RecId, 40)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0002", new DateTime(2026, 10, 31), [new(line.RecId, 40)]), default)).StatusCode);

        line = await db.Set<PurchLine>().SingleAsync();
        po = await db.Set<PurchTable>().SingleAsync();
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, line.RemainPurchPhysical);
        Assert.Equal(0m, line.RemainPurchFinancial);
        Assert.Equal(PurchStatus.Invoiced, po.PurchStatus);
        Assert.Equal(100m, sum.PhysicalInvent);
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(0m, sum.Received);
        Assert.Equal(5000m, sum.PostedValue);
        Assert.Equal(0m, sum.PhysicalValue);

        var trans = await db.Set<InventTrans>().ToListAsync();
        Assert.All(trans, t => Assert.Equal(StatusReceipt.Purchased, t.StatusReceipt));
        Assert.Equal(100m, trans.Sum(t => t.Qty));
        Assert.Equal(5000m, trans.Sum(t => t.CostAmountPosted));
        Assert.Equal(2, await db.Set<VendPackingSlipJour>().CountAsync());
        Assert.Equal(2, await db.Set<VendInvoiceJour>().CountAsync());
        Assert.Equal(taxable ? -5750m : -5000m, (await db.Set<VendTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.All(await db.Set<VendTrans>().ToListAsync(), row => Assert.Equal(row.TransDate.AddMonths(1), row.DueDate));
        Assert.Equal(2, await db.Set<VendInvoicePackingSlipQuantityMatch>().CountAsync());
        Assert.Equal(taxable ? 750m : 0m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        var ledger = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(0m, ledger.Sum(x => x.AccountingCurrencyAmount));                     // balanced
        Assert.Equal(taxable ? 5750m : 5000m, ledger.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount));
    }

    // ---- Two-step receipt: arrival registration then product receipt (Logic_Readonly.md §10) ----

    /// <summary>
    /// The rule §10 states outright: a registration and the product receipt that follows it are
    /// <b>not</b> two receipts of the same goods. Register 60 of 100, then receive the whole 100;
    /// physical on-hand ends at 100, not 160, and the registered bucket is emptied again.
    /// </summary>
    [Fact]
    public async Task Registering_an_arrival_then_receiving_does_not_count_the_quantity_twice()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        var registrations = new PurchRegistrationService(new TestUnitOfWork(db), new TestCompany());

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        var ledgerBefore = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count;

        var result = await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var sum = await db.Set<InventSum>().SingleAsync();
        // Registered is its own bucket: it leaves ordered and does not become physical on-hand.
        Assert.Equal(60m, sum.Registered);
        Assert.Equal(40m, sum.Ordered);
        Assert.Equal(0m, sum.PhysicalInvent);
        Assert.Equal(0m, sum.PhysicalValue);
        // Registration posts nothing and does not consume the line's remainder.
        Assert.Equal(ledgerBefore, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        Assert.Empty(await db.Set<VendPackingSlipJour>().ToListAsync());
        line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(100m, line.RemainPurchPhysical);

        // The receipt then posts the whole 100 once, consuming the registered 60 first.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.PhysicalInvent);          // not 160
        Assert.Equal(0m, sum.Registered);                // the bucket is emptied, not stranded
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(5000m, sum.PhysicalValue);
        Assert.Equal(0m, (await db.Set<PurchLine>().SingleAsync()).RemainPurchPhysical);
        // Every ordered or registered row has moved on; none is left open.
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(),
            t => t.StatusReceipt is StatusReceipt.Ordered or StatusReceipt.Registered);
    }

    /// <summary>
    /// Registering the same goods twice runs out of ordered supply and is refused, which is what
    /// stops a registration from inflating on-hand by repetition.
    /// </summary>
    [Fact]
    public async Task Registering_more_than_the_ordered_supply_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        var registrations = new PurchRegistrationService(new TestUnitOfWork(db), new TestCompany());

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();

        Assert.Equal(200, (await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(new DateTime(2026, 10, 28), [new(line.RecId, 100)]), default)).StatusCode);
        var refused = await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(new DateTime(2026, 10, 28), [new(line.RecId, 100)]), default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("may already be registered or received", refused.Message);
        // Rolled back: the first registration stands and nothing is doubled.
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.Registered);
        Assert.Equal(0m, sum.Ordered);

        // Over-registering against the line's remainder is refused before inventory is touched.
        var tooMuch = await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(null, [new(line.RecId, 101)]), default);
        Assert.Equal(422, tooMuch.StatusCode);
        Assert.Contains("exceeds the remaining quantity", tooMuch.Message);
    }

    // ---- Purchase order cancellation: only the unreceived remainder goes away ----

    /// <summary>
    /// The case the cycle had no ending for. Order 100, receive 60, invoice nothing, then cancel:
    /// the 40 that will never arrive is released from ordered supply, and the 60 already delivered
    /// stays invoiceable. Nothing already posted is disturbed and no ledger entry arises, because
    /// ordered supply is neither physical nor financial inventory.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_partly_received_order_releases_the_remainder_and_keeps_the_receipt_invoiceable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default)).StatusCode);
        var ledgerBefore = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count;

        var cancelled = await orders.CancelRemainderAsync(po.RecId, default);

        Assert.Equal(200, cancelled.StatusCode);
        line = await db.Set<PurchLine>().SingleAsync();
        var sum = await db.Set<InventSum>().SingleAsync();
        // The 40 expected units are gone from ordered supply; the 60 received are untouched.
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(0m, line.RemainPurchPhysical);
        // The receipt is still waiting to be invoiced, which is the whole point.
        Assert.Equal(60m, line.RemainPurchFinancial);
        Assert.Equal(PurchStatus.Received, line.PurchStatus);
        // No ordered inventory transaction is left open.
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), t => t.StatusReceipt == StatusReceipt.Ordered);
        // Cancelling ordered supply has no accounting effect.
        Assert.Equal(ledgerBefore, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count);

        // And the delivered 60 can still be invoiced afterwards, closing the order properly.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 29), [new(line.RecId, 60)]), default)).StatusCode);
        line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(0m, line.RemainPurchFinancial);
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
        Assert.Equal(3000m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    /// <summary>
    /// An order that received nothing is cancelled outright: the line and the order both reach
    /// Canceled and all the ordered supply is released.
    /// </summary>
    [Fact]
    public async Task Cancelling_an_untouched_order_cancels_the_line_and_the_order()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(100m, (await db.Set<InventSum>().SingleAsync()).Ordered);

        Assert.Equal(200, (await orders.CancelRemainderAsync(po.RecId, default)).StatusCode);

        line = await db.Set<PurchLine>().SingleAsync();
        po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(PurchStatus.Canceled, line.PurchStatus);
        Assert.Equal(PurchStatus.Canceled, po.PurchStatus);
        Assert.Equal(0m, line.RemainPurchPhysical);
        Assert.Equal(0m, line.RemainPurchFinancial);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(0m, sum.PhysicalInvent);

        // Cancelling twice, and receiving against a cancelled order, are both refused.
        Assert.Equal(422, (await orders.CancelRemainderAsync(po.RecId, default)).StatusCode);
        Assert.Equal(422, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0002", null, [new(line.RecId, 10)]), default)).StatusCode);
    }

    /// <summary>
    /// A fully received and invoiced order has no remainder, so cancellation is refused by name
    /// rather than quietly doing nothing.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_fully_received_order_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 10, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 10)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 29), [new(line.RecId, 10)]), default)).StatusCode);

        var refused = await orders.CancelRemainderAsync(po.RecId, default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("no unreceived quantity left to cancel", refused.Message);
        // The invoiced order is left exactly as it was.
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
    }

    /// <summary>
    /// If the line's remainder and its ordered inventory transactions disagree, the supply is not
    /// understood well enough to release the right quantity, so the whole cancellation rolls back
    /// rather than cancelling a quantity that might belong to a receipt.
    /// </summary>
    [Fact]
    public async Task Cancellation_is_refused_when_ordered_inventory_does_not_match_the_remainder()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        // Desynchronise the ordered transaction from the line, as stale data would be.
        var ordered = await db.Set<InventTrans>().SingleAsync();
        ordered.Qty = 70m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var refused = await orders.CancelRemainderAsync(po.RecId, default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("does not match the uninvoiced remainder", refused.Message);
        // Rolled back: the line keeps its remainder and the order stays open.
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(100m, line.RemainPurchPhysical);
        Assert.Equal(PurchStatus.Backorder, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
    }

    // ---- Line-level cancellation and open-quantity reduction ----

    /// <summary>
    /// Reducing an open quantity is not cancelling the line: order 100, reduce by 40, and 60 stays
    /// expected and invoiceable. The released 40 leaves ordered supply, the line keeps the status it
    /// had because it is still waiting for goods, and the ordered inventory transaction keeps its
    /// remaining 60 as Ordered rather than being closed.
    /// </summary>
    [Fact]
    public async Task Reducing_a_line_quantity_releases_only_that_part_and_keeps_the_rest_expected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        var ledgerBefore = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count;

        var reduced = await orders.CancelLineRemainderAsync(line.RecId, 40m, default);

        Assert.Equal(200, reduced.StatusCode);
        Assert.Contains("quantity reduced", reduced.Message);
        line = await db.Set<PurchLine>().SingleAsync();
        // 60 is still coming and still invoiceable; only the 40 was released.
        Assert.Equal(60m, line.RemainPurchPhysical);
        Assert.Equal(60m, line.RemainPurchFinancial);
        Assert.Equal(60m, (await db.Set<InventSum>().SingleAsync()).Ordered);
        // A line still expecting goods keeps the status it had: only the quantity was trimmed.
        Assert.Equal(PurchStatus.Backorder, line.PurchStatus);
        Assert.Equal(PurchStatus.Backorder, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
        // The part still expected stays Ordered rather than being closed out.
        var ordered = await db.Set<InventTrans>().SingleAsync(x => x.StatusReceipt == StatusReceipt.Ordered);
        Assert.Equal(60m, ordered.Qty);
        // Releasing ordered supply has no accounting effect.
        Assert.Equal(ledgerBefore, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count);

        // The reduced quantity still receives and invoices normally, closing the order at 60.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 29), [new(line.RecId, 60)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 30), [new(line.RecId, 60)]), default)).StatusCode);
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
        Assert.Equal(3000m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    /// <summary>
    /// Cancelling one line of a two-line order leaves the other line untouched and the order open,
    /// which is what order-level cancellation could not express.
    /// </summary>
    [Fact]
    public async Task Cancelling_one_line_leaves_the_other_line_and_the_order_open()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 20, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var lines = (await db.Set<PurchLine>().ToListAsync()).OrderBy(x => x.LineNumber).ToList();
        var first = lines[0];
        // Both lines are the same item, so they share one InventSum: 100 + 20 ordered.
        Assert.Equal(120m, (await db.Set<InventSum>().SingleAsync()).Ordered);

        Assert.Equal(200, (await orders.CancelLineRemainderAsync(first.RecId, null, default)).StatusCode);

        lines = (await db.Set<PurchLine>().ToListAsync()).OrderBy(x => x.LineNumber).ToList();
        first = lines[0];
        var second = lines[1];
        Assert.Equal(PurchStatus.Canceled, first.PurchStatus);
        // The untouched line keeps its whole quantity, and only its 20 is left on order.
        Assert.Equal(PurchStatus.Backorder, second.PurchStatus);
        Assert.Equal(20m, second.RemainPurchPhysical);
        Assert.Equal(20m, second.RemainPurchFinancial);
        Assert.Equal(20m, (await db.Set<InventSum>().SingleAsync()).Ordered);
        // One cancelled line does not close an order that still expects the other.
        Assert.Equal(PurchStatus.Backorder, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);

        // Cancelling the same line twice is refused by name.
        var refused = await orders.CancelLineRemainderAsync(first.RecId, null, default);
        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("already canceled", refused.Message);
    }

    /// <summary>
    /// Asking to cancel more than the line has left, or nothing at all, is refused with the figures
    /// named and the line left exactly as it was.
    /// </summary>
    [Fact]
    public async Task Reducing_a_line_by_more_than_it_expects_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        // 60 received leaves 40 unreceived, so 50 is more than the line can release.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default)).StatusCode);

        var refused = await orders.CancelLineRemainderAsync(line.RecId, 50m, default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("which is less than the 50 requested", refused.Message);
        Assert.Equal(422, (await orders.CancelLineRemainderAsync(line.RecId, 0m, default)).StatusCode);
        // Rolled back on both counts: the remainder and the ordered supply are unchanged.
        line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(40m, line.RemainPurchPhysical);
        Assert.Equal(100m, line.RemainPurchFinancial);
        Assert.Equal(40m, (await db.Set<InventSum>().SingleAsync()).Ordered);
    }

    [Fact]
    public async Task Purchase_line_rejects_warehouse_from_another_site_and_blocked_vendor()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        db.Set<InventLocation>().Add(new InventLocation { RecId = 2, DataAreaId = "dat", InventLocationId = "WH-03", InventSiteId = "S02" });
        db.Set<VendTable>().Add(new VendTable { RecId = 2, DataAreaId = "dat", AccountNum = "V-BLOCK", Currency = "SAR", Blocked = CustVendBlocked.All });
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);

        Assert.Equal(422, (await orders.CreateAsync(new PurchOrderCreateDto("V-BLOCK", null, "SAR", null, "S01", "WH-01", null), default)).StatusCode);
        Assert.Equal(422, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", null, "S01", "WH-03", null), default)).StatusCode);
        Assert.Empty(await db.Set<PurchTable>().ToListAsync());
    }

    // ---- Sales order cycle (CycleSalesorder.md / ARTables.md Ãƒâ€šÃ‚Â§14 state table) -------------------

    [Fact]
    public async Task Sales_invoice_without_an_exchange_rate_posts_nothing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedSalesMasterData(db);
        SeedForeignOrder(db);
        await db.SaveChangesAsync();
        var order = await db.Set<SalesTable>().SingleAsync(x => x.SalesId == "SO-FX");
        var invoices = new SalesInvoiceService(new TestUnitOfWork(db), new TestCompany(), Sequence());

        // No rate is configured for USD/SAR, so the invoice is refused rather than posted at an
        // assumed rate. Nothing is written.
        var result = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 7), "fx-1"), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("No exchange rate is available", result.Message);
        Assert.Empty(await db.Set<CustInvoiceJour>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Fact]
    public async Task Sales_invoice_translates_a_foreign_currency_order_into_the_accounting_currency()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        SeedRate(db, "USD", "SAR", 3.75m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();

        // Buy in SAR, sell 10 @ 100 USD + 15% VAT: 1,000 revenue and 150 tax, so the customer owes
        // 1,150 USD. At 3.75 that is 3,750 / 562.50 / 4,312.50 in the accounting currency.
        await StockAndSellTenAsync(db, numbers, salesCurrency: "USD");

        var journal = await db.Set<CustInvoiceJour>().AsNoTracking().SingleAsync();
        Assert.Equal("USD", journal.CurrencyCode);
        Assert.Equal(1150m, journal.InvoiceAmount);
        Assert.Equal(4312.5m, journal.InvoiceAmountMst);
        Assert.Equal(1000m, journal.SalesBalance);
        Assert.Equal(3750m, journal.SalesBalanceMst);
        Assert.Equal(150m, journal.SumTax);
        Assert.Equal(562.5m, journal.SumTaxMst);

        // The customer's debt is held in both currencies.
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.Invoice == journal.InvoiceId);
        Assert.Equal("USD", custTrans.CurrencyCode);
        Assert.Equal(1150m, custTrans.AmountCur);
        Assert.Equal(4312.5m, custTrans.AmountMst);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == custTrans.RecId);
        Assert.Equal(1150m, open.AmountCur);
        Assert.Equal(4312.5m, open.AmountMst);

        // The invoice voucher covers the invoice entry and the reversal of the packing slip's
        // physical posting, so both are read together.
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == journal.InvoiceId).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();

        // Revenue keeps the order's currency on the transaction amount and is translated for the ledger.
        var revenue = lines.Single(x => x.PostingType == LedgerPostingType.SalesRevenue);
        Assert.Equal("USD", revenue.TransactionCurrencyCode);
        Assert.Equal(-1000m, revenue.TransactionCurrencyAmount);
        Assert.Equal(-3750m, revenue.AccountingCurrencyAmount);
        var customer = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        Assert.Equal(1150m, customer.TransactionCurrencyAmount);
        Assert.Equal(4312.5m, customer.AccountingCurrencyAmount);

        // Cost of goods came from inventory and is already an accounting-currency amount, so it is
        // posted in that currency and is not translated: 10 units at cost 60 is 600 either way.
        var cogs = lines.Single(x => x.PostingType == LedgerPostingType.SalesConsump);
        Assert.Equal("SAR", cogs.TransactionCurrencyCode);
        Assert.Equal(600m, cogs.TransactionCurrencyAmount);
        Assert.Equal(600m, cogs.AccountingCurrencyAmount);

        // The voucher balances in the accounting currency, which is the balance that must hold when
        // its lines are in more than one transaction currency.
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));

        // Tax is stored in both currencies, so settlement sums accounting amounts.
        var tax = await db.Set<TaxTrans>().AsNoTracking().SingleAsync(x => x.Voucher == journal.InvoiceId);
        Assert.Equal(-150m, tax.TaxAmountCur);
        Assert.Equal(-562.5m, tax.TaxAmount);
    }

    [Fact]
    public async Task A_foreign_currency_invoice_balances_exactly_when_the_rate_rounds()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        // A rate chosen so that translating each line and translating the total disagree: at
        // 0.123456, revenue 1,000 gives 123.46 and tax 150 gives 18.52, summing to 141.98, while
        // the total of 1,150 translates to 141.97. Deriving the customer line from the lines
        // posted is what keeps the voucher balanced.
        SeedRate(db, "USD", "SAR", 0.123456m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();

        await StockAndSellTenAsync(db, numbers, salesCurrency: "USD");

        var journal = await db.Set<CustInvoiceJour>().AsNoTracking().SingleAsync();
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == journal.InvoiceId).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();

        // The point of the test: both ledger currencies balance to the cent, even though the
        // individual lines were rounded independently.
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(0m, lines.Sum(x => x.ReportingCurrencyAmount));

        // And the customer's subledger balance is the same figure the ledger received, so the
        // two cannot drift apart.
        var customer = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.Invoice == journal.InvoiceId);
        Assert.Equal(customer.AccountingCurrencyAmount, custTrans.AmountMst);
        Assert.Equal(customer.AccountingCurrencyAmount, journal.InvoiceAmountMst);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == custTrans.RecId);
        Assert.Equal(custTrans.AmountMst, open.AmountMst);

        // The transaction currency still holds the amount actually invoiced.
        Assert.Equal(1150m, custTrans.AmountCur);
    }

    [Fact]
    public async Task A_foreign_currency_credit_note_translates_revenue_and_leaves_cost_in_the_accounting_currency()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().Add(new ReturnDispositionCode
        { RecId = 1, DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit });
        db.Set<ReturnReasonCode>().Add(new ReturnReasonCode { RecId = 1, DataAreaId = "dat", ReasonCodeId = "DAMAGED" });
        SeedRate(db, "USD", "SAR", 0.123456m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();

        // Sold 10 @ 100 USD + 15% VAT; return 2 of them, so revenue 200 USD and VAT 30 USD reverse.
        var invoiceId = await StockAndSellTenAsync(db, numbers, salesCurrency: "USD");
        var sold = await db.Set<CustInvoiceTrans>().SingleAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        Assert.Equal(200, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, "DAMAGED",
            [new(sold.RecId, 2, "CREDIT")]), default)).StatusCode);
        var rma = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        Assert.Equal("USD", rma.CurrencyCode);
        var creditLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId);
        Assert.Equal(200, (await returns.ReceiveAsync(rma.RecId,
            new CustReturnReceiptDto("rr-1", new DateTime(2026, 10, 20), [new(creditLine.RecId, 2)]), default)).StatusCode);

        var credit = await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-1"), default);
        Assert.True(credit.StatusCode == 200, credit.Message);

        var note = await db.Set<CustInvoiceJour>().AsNoTracking().SingleAsync(x => x.InvoiceAmount < 0);
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == note.InvoiceId).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();

        // Revenue is credited in the order currency and translated for the ledger.
        var revenue = lines.Single(x => x.PostingType == LedgerPostingType.SalesRevenue);
        Assert.Equal("USD", revenue.TransactionCurrencyCode);
        Assert.Equal(200m, revenue.TransactionCurrencyAmount);

        // The returned goods go back into inventory at their original cost, which is already an
        // accounting-currency amount and is not translated: 2 units at 60 is 120.
        var issue = lines.Single(x => x.PostingType == LedgerPostingType.SalesIssue);
        Assert.Equal("SAR", issue.TransactionCurrencyCode);
        Assert.Equal(120m, issue.AccountingCurrencyAmount);

        // Both ledger currencies balance to the cent despite independent per-line rounding.
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(0m, lines.Sum(x => x.ReportingCurrencyAmount));

        // The customer's credit in the subledger is the figure the ledger received.
        var customer = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.Voucher == note.InvoiceId);
        Assert.Equal(customer.AccountingCurrencyAmount, custTrans.AmountMst);
        Assert.Equal(customer.AccountingCurrencyAmount, note.InvoiceAmountMst);
        Assert.Equal(-230m, custTrans.AmountCur); // 200 revenue + 30 VAT, credited
    }

    [Fact]
    public async Task A_foreign_currency_purchase_invoice_stores_inventory_cost_in_the_accounting_currency()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedRate(db, "USD", "SAR", 3.75m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);

        // Buy 100 @ 60 USD against a ledger that keeps its accounts in SAR.
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "USD", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default)).StatusCode);
        Assert.Equal(22500m, (await db.Set<InventSum>().SingleAsync()).PhysicalValue);
        Assert.Equal(22500m, (await db.Set<InventTrans>().SingleAsync(x => x.StatusReceipt == StatusReceipt.Received)).CostAmountPhysical);
        Assert.Equal(22500m, (await db.Set<VendPackingSlipTrans>().SingleAsync()).ValueMst);
        var invoice = await vendorInvoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default);
        Assert.True(invoice.StatusCode == 200, invoice.Message);

        // The point of the test: inventory value is held in the accounting currency. 6,000 USD at
        // 3.75 is 22,500 SAR, so the cost per unit that every later settlement reads is 225, not 60.
        var purchased = await db.Set<InventTrans>().AsNoTracking()
            .SingleAsync(x => x.StatusReceipt == StatusReceipt.Purchased);
        Assert.Equal(22500m, purchased.CostAmountPosted);
        var sum = await db.Set<InventSum>().AsNoTracking().SingleAsync();
        Assert.Equal(100m, sum.PostedQty);
        Assert.Equal(22500m, sum.PostedValue);
        Assert.Equal(0m, sum.PhysicalValue);

        // The vendor is owed in the order currency, and the same debt translated.
        var vendTrans = await db.Set<VendTrans>().AsNoTracking().SingleAsync();
        Assert.Equal("USD", vendTrans.CurrencyCode);
        Assert.Equal(-6000m, vendTrans.AmountCur);
        Assert.Equal(-22500m, vendTrans.AmountMst);

        // The invoice voucher balances in the accounting currency.
        var journal = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync();
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == journal.LedgerVoucher).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        var inventory = lines.Single(x => x.PostingType == LedgerPostingType.PurchReceipt
            && x.AccountingCurrencyAmount > 0);
        Assert.Equal("USD", inventory.TransactionCurrencyCode);
        Assert.Equal(6000m, inventory.TransactionCurrencyAmount);
        Assert.Equal(22500m, inventory.AccountingCurrencyAmount);
    }

    [Fact]
    public async Task Foreign_currency_product_receipt_without_rate_rolls_back()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "USD", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 10, "PCS", 60, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();

        var result = await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("NO-RATE", new DateTime(2026, 10, 2), [new(line.RecId, 10)]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("No exchange rate is available", result.Message);
        Assert.Empty(await db.Set<VendPackingSlipJour>().ToListAsync());
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).PhysicalValue);
    }

    /// <summary>A one-line order of 100 USD against a ledger that keeps its accounts in SAR.</summary>
    private static void SeedForeignOrder(CycleContext db)
    {
        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-FX", CustAccount = "C-001", InvoiceAccount = "C-001",
            CurrencyCode = "USD", SalesStatus = SalesStatus.Backorder
        };
        db.Set<SalesTable>().Add(order);
        db.Set<SalesLine>().Add(new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001",
            SalesQty = 1, QtyOrdered = 1, RemainSalesFinancial = 1, RemainSalesPhysical = 0,
            LineAmount = 100, CurrencyCode = "USD"
        });
    }

    /// <summary>An active rate type, currency pair and rate, which is what translation reads.</summary>
    private static void SeedRate(CycleContext db, string from, string to, decimal rate)
    {
        db.Set<ExchangeRateType>().Add(new ExchangeRateType
        { RecId = 1, DataAreaId = "dat", Name = "Default", IsActive = true });
        db.Set<ExchangeRateCurrencyPair>().Add(new ExchangeRateCurrencyPair
        {
            RecId = 1, DataAreaId = "dat", ExchangeRateType = 1, FromCurrencyCode = from, ToCurrencyCode = to,
            ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One, IsActive = true
        });
        db.Set<ExchangeRate>().Add(new ExchangeRate
        {
            RecId = 1, DataAreaId = "dat", ExchangeRateCurrencyPair = 1,
            ExchangeRateValue = rate, ValidFrom = new DateTime(2026, 1, 1), IsActive = true
        });
    }

    [Fact]
    public async Task Sales_order_partial_delivery_partial_invoice_and_payment_reconcile()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<InventModelGroup>().Local.Single().PostPhysicalRevenue = NoYes.Yes;
        db.Set<InventModelGroup>().Local.Single().PostOnHandPhysical = NoYes.Yes;
        db.AddRange(
            new InventPosting { RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var numbers = new TestInventNumbers();

        // Stock 100 PCS at 60 through the real purchase cycle.
        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default)).StatusCode);

        // SO-0001: 10 PCS at 100, VAT 15%.
        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-0001", CustAccount = "C-001", InvoiceAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S01", InventLocationId = "WH-01", OrderDate = new DateTime(2026, 10, 5), PaymTerm = "NET30"
        };
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "NET30", PaymMethod = PaymMethod.Net, NumOfDays = 30 });
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, SalesUnit = "PCS", SalesPrice = 100, PriceUnit = 1,
            LineAmount = 1000, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, CurrencyCode = "SAR",
            TaxGroup = "VAT15", TaxItemGroup = "VAT15", ReceiptDateRequested = new DateTime(2026, 10, 10)
        };
        var demand = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        await demand.CreateAsync(order, line, "S01", "WH-01");
        db.Set<SalesLine>().Add(line);
        await db.SaveChangesAsync();

        var reservation = new InventoryReservationService(work, company);
        Assert.Null((await reservation.ReserveAsync(line.InventTransId, 10m, false, default)).Error);
        Assert.Equal(10m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 6m, default)).Error);
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 2m, default)).Error); // unpick restores reservation
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 6m, default)).Error);
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 6m, default)).Error); // target retry is idempotent
        var beforeDelivery = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, beforeDelivery.PhysicalInvent);
        Assert.Equal(6m, beforeDelivery.Picked);
        Assert.Equal(4m, beforeDelivery.ReservPhysical);
        Assert.Equal(90m, beforeDelivery.AvailPhysical);
        Assert.NotNull((await reservation.SetPickedAsync(line.InventTransId, 11m, default)).Error);
        Assert.NotNull((await reservation.SetPickedAsync("another-company-source", 1m, default)).Error);

        var packingSlips = new PackingSlipJournalService(work, company, Sequence());
        var invoices = new SalesInvoiceService(work, company, Sequence());

        // Packing slip 6 -> remain physical 4, remain financial 10; reservation consumed.
        var ps1 = await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 10, 6), "Picked", true, "ps-1",
            [new PackingSlipPostingLineRequest(line.RecId.ToString(), 6)]), default);
        Assert.Equal(200, ps1.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(10m, line.RemainSalesFinancial);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(94m, sum.PhysicalInvent);
        Assert.Equal(4m, sum.ReservPhysical);
        Assert.Equal(0m, sum.Picked);

        // Invoice 4 of the 6 delivered -> remain financial 6; invoicing beyond delivered is rejected.
        var inv1 = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-1", [new(line.RecId, 4)]), default);
        Assert.Equal(200, inv1.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(6m, line.RemainSalesFinancial);
        var remainingDelivered = await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.Deducted);
        Assert.Equal(-2m, remainingDelivered.Qty);
        Assert.Equal(-120m, remainingDelivered.CostAmountPhysical);
        Assert.Equal(-120m, (await db.Set<InventSum>().SingleAsync()).PhysicalValue);
        Assert.Equal(NoYes.Yes, (await db.Set<InventTransPosting>().SingleAsync(x => x.InventTransPostingType == InventTransPostingType.Physical)).IsPosted);
        Assert.Equal(422, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(null, null, [new(line.RecId, 3)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(null, "inv-1", [new(line.RecId, 4)]), default)).StatusCode);
        Assert.Single(await db.Set<CustInvoiceJour>().ToListAsync()); // idempotent retry

        // Change posting accounts between deliveries: each invoice must reverse its own slip.
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.SalesPackingSlip)).LedgerDimension = 5;
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.SalesPackingSlipOffsetAccount)).LedgerDimension = 6;
        await db.SaveChangesAsync();
        // Packing slip 4, then invoice the remaining 6 delivered.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 10, 8), "All", true, "ps-2",
            [new PackingSlipPostingLineRequest(line.RecId.ToString(), 4)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 9), "inv-2"), default)).StatusCode);
        var financialPostings = await db.Set<InventTransPosting>()
            .Where(x => x.InventTransPostingType == InventTransPostingType.Financial && x.ItemId == "ITEM-001")
            .ToListAsync();
        var financialSalesOrigin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == line.InventTransId);
        var salesPostings = financialPostings.Where(x => x.InventTransOrigin == financialSalesOrigin.RecId).ToList();
        Assert.Equal(2, salesPostings.Count);
        Assert.Equal(2, salesPostings.Select(x => x.Voucher).Distinct().Count());
        Assert.All(salesPostings, x => { Assert.Equal(LedgerPostingType.SalesIssue, x.PostingType); Assert.Equal(LedgerPostingType.SalesConsump, x.PostingTypeOffset); });
        var physicalPostings = await db.Set<InventTransPosting>().Where(x => x.InventTransOrigin == financialSalesOrigin.RecId
            && x.InventTransPostingType == InventTransPostingType.Physical).ToListAsync();
        Assert.Equal(2, physicalPostings.Count);
        Assert.All(physicalPostings, x => Assert.Equal(NoYes.No, x.IsPosted));
        var physicalLedger = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Where(x =>
            x.PostingType is LedgerPostingType.SalesPackingSlip or LedgerPostingType.SalesOffsetAccountPackingSlip);
        Assert.All(physicalLedger.GroupBy(x => x.LedgerDimension), group => Assert.Equal(0m, group.Sum(x => x.AccountingCurrencyAmount)));

        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, line.SalesStatus);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync()).SalesStatus);

        var journals = await db.Set<CustInvoiceJour>().ToListAsync();
        Assert.Equal(1000m, journals.Sum(x => x.SalesBalance));
        Assert.Equal(150m, journals.Sum(x => x.SumTax));
        Assert.Equal(1150m, journals.Sum(x => x.InvoiceAmount));
        Assert.All(journals, x => Assert.Equal(x.InvoiceDate.AddDays(30), x.DueDate));
        Assert.All(await db.Set<CustTrans>().ToListAsync(), x => Assert.Equal(x.TransDate.AddDays(30), x.DueDate));
        Assert.All(await db.Set<CustTransOpen>().ToListAsync(), x => Assert.Equal(x.TransDate.AddDays(30), x.DueDate));
        Assert.Equal(-150m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(90m, sum.PostedQty);
        Assert.Equal(0m, sum.Deducted);
        Assert.Equal(0m, sum.ReservPhysical);
        Assert.Equal(5400m, sum.PostedValue); // 6,000 purchased - 600 COGS (10 x 60)
        var salesOrigin = await db.Set<InventTransOrigin>().SingleAsync(x => x.ReferenceCategory == InventRefType.SalesTable);
        var salesTrans = await db.Set<InventTrans>().Where(x => x.InventTransOrigin == salesOrigin.RecId).ToListAsync();
        Assert.All(salesTrans, x => Assert.Equal(StatusIssue.Sold, x.StatusIssue));
        Assert.Equal(-10m, salesTrans.Sum(x => x.Qty));
        Assert.Equal(-600m, salesTrans.Sum(x => x.CostAmountPosted));

        Assert.Equal(1150m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));

        // Customer payment of 1,150 settles both invoices; the open receivable becomes zero.
        var payments = new CustPaymentService(work, company, Sequence());
        Assert.Equal(400, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m,
            RequestId: new string('x', FieldLengths.PaymReference + 1)), default)).StatusCode);
        var otherCustomerInvoice = new CustTrans
        {
            DataAreaId = "dat", AccountNum = "C-OTHER", Invoice = "OTHER-INV", CurrencyCode = "SAR",
            AmountCur = 50m, AmountMst = 50m, ReportingCurrencyAmount = 50m,
            TransDate = new DateTime(2026, 10, 1), DueDate = new DateTime(2026, 10, 31)
        };
        db.Add(otherCustomerInvoice);
        await db.SaveChangesAsync();
        var mislinkedOpen = new CustTransOpen
        {
            DataAreaId = "dat", AccountNum = "C-001", RefRecId = otherCustomerInvoice.RecId,
            AmountCur = 50m, AmountMst = 50m, ReportingCurrencyAmount = 50m,
            TransDate = otherCustomerInvoice.TransDate, DueDate = otherCustomerInvoice.DueDate
        };
        db.Add(mislinkedOpen);
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 50m,
            new DateTime(2026, 10, 15), "BANK", "OTHER-INV", "pay-cross-customer"), default)).StatusCode);
        mislinkedOpen = await db.Set<CustTransOpen>().SingleAsync(x => x.RecId == mislinkedOpen.RecId);
        db.Remove(mislinkedOpen);
        otherCustomerInvoice = await db.Set<CustTrans>().SingleAsync(x => x.RecId == otherCustomerInvoice.RecId);
        db.Remove(otherCustomerInvoice);
        await db.SaveChangesAsync();
        var mismatchedOpen = await db.Set<CustTransOpen>().OrderBy(x => x.RecId).FirstAsync();
        mismatchedOpen.AmountMst += 1m;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m,
            new DateTime(2026, 10, 15), "BANK", null, "pay-mismatch"), default)).StatusCode);
        Assert.Empty(await db.Set<BankAccountTrans>().ToListAsync());
        mismatchedOpen = await db.Set<CustTransOpen>().SingleAsync(x => x.RecId == mismatchedOpen.RecId);
        mismatchedOpen.AmountMst -= 1m;
        await db.SaveChangesAsync();
        var paid = await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m, new DateTime(2026, 10, 15), "BANK", null, "pay-1"), default);
        Assert.Equal(200, paid.StatusCode);
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        var custTrans = await db.Set<CustTrans>().ToListAsync();
        Assert.Equal(0m, custTrans.Sum(x => x.AmountCur));
        Assert.All(custTrans.Where(x => x.TransType == LedgerTransType.Sales), x => Assert.Equal(x.AmountCur, x.SettleAmountCur));
        Assert.Equal(4, await db.Set<CustSettlement>().CountAsync());
        Assert.Equal(NoYes.Yes, (await db.Set<LedgerJournalTable>().SingleAsync()).Posted);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m, null, null, null, "pay-1"), default)).StatusCode);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync()); // idempotent retry
        Assert.Equal(1150m, (await db.Set<BankAccountTrans>().SingleAsync()).AmountCur);
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1151m, RequestId: "pay-1"), default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("OTHER", "BANK-1", 1150m, RequestId: "pay-1"), default)).StatusCode);
        Assert.Single(await db.Set<BankAccountTrans>().ToListAsync());
        db.Set<BankAccountTrans>().Remove(await db.Set<BankAccountTrans>().SingleAsync());
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m,
            RequestId: "pay-1"), default)).StatusCode);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
    }

    [Fact]
    public async Task Vendor_payments_settle_partially_then_fully_preserve_overpayment_and_reject_conflicting_retries()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        db.Add(new MainAccount { RecId = 10, DataAreaId = "dat", MainAccountId = "BANK" });
        db.Add(new DimensionAttributeValueCombination { RecId = 10, DataAreaId = "dat", MainAccount = 10, DisplayValue = "BANK" });
        db.Add(new BankAccountTable { DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 10, CurrencyCode = "SAR" });
        var invoice = new VendTrans { DataAreaId = "dat", AccountNum = "V-0001", Invoice = "INV-1", CurrencyCode = "SAR",
            AmountCur = -100, AmountMst = -100, TransDate = new DateTime(2026, 10, 1), DueDate = new DateTime(2026, 10, 31), TransType = LedgerTransType.Purch };
        db.Add(invoice);
        await db.SaveChangesAsync();
        VendorOpenBalance.Add(db, invoice);
        // Vendor payment validates the method of payment against its setup table.
        db.Set<VendPaymModeTable>().Add(new VendPaymModeTable { RecId = 1, DataAreaId = "dat", PaymMode = "BANK" });
        await db.SaveChangesAsync();
        var payments = new VendPaymentService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), Sequence());
        var request = new VendPaymentPostDto("V-0001", "BANK-1", 40, "VP1", new DateTime(2026, 10, 15), "BANK", "INV-1");
        Assert.Equal(200, (await payments.PostAsync(request, default)).StatusCode);
        Assert.Equal(-60m, (await db.Set<VendTransOpen>().SingleAsync()).AmountCur);
        Assert.Equal(200, (await payments.PostAsync(request, default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(request with { Amount = 41 }, default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(request with { VendAccount = "OTHER" }, default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(request with { RequestId = "BAD", SettleInvoiceId = "MISSING" }, default)).StatusCode);
        (await db.Set<BankAccountTable>().SingleAsync()).CurrencyCode = "USD";
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(request with { RequestId = "FX" }, default)).StatusCode);
        (await db.Set<BankAccountTable>().SingleAsync()).CurrencyCode = "SAR";
        (await db.Set<VendTable>().SingleAsync()).Blocked = CustVendBlocked.Payment;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(request with { RequestId = "BLOCKED" }, default)).StatusCode);
        (await db.Set<VendTable>().SingleAsync()).Blocked = CustVendBlocked.No;
        await db.SaveChangesAsync();
        Assert.Equal(200, (await payments.PostAsync(request with { RequestId = "VP2", Amount = 70 }, default)).StatusCode);
        var outstanding = await db.Set<VendTransOpen>().SingleAsync();
        Assert.Equal(10m, outstanding.AmountCur);
        Assert.Equal(LedgerTransType.Payment, (await db.Set<VendTrans>().SingleAsync(x => x.RecId == outstanding.RefRecId)).TransType);
        Assert.Equal(-100m, (await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "INV-1")).SettleAmountCur);
        Assert.Equal(2, await db.Set<VendSettlement>().CountAsync());
        Assert.Equal(2, await db.Set<LedgerJournalTable>().CountAsync());
        Assert.Equal(2, await db.Set<BankAccountTrans>().CountAsync());
        Assert.Equal(-110m, (await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
    }

    // ---- Returns (CycleSalesorder.md 10.3, Purchorder.md 8) ---------------------------------------

    [Fact]
    public async Task Customer_return_rejects_combined_request_lines_exceeding_invoiced_quantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().AddRange(
            new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit },
            new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        var summaryBefore = await db.Set<InventSum>().Select(x => x.Ordered).SingleAsync();
        var originCountBefore = await db.Set<InventTransOrigin>().CountAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        var result = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 6m, "CREDIT"), new(invoiceLine.RecId, 5m, "CRONLY")]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("exceeds the returnable quantity", result.Message);
        Assert.Empty(await db.Set<SalesTable>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync());
        Assert.Equal(originCountBefore, await db.Set<InventTransOrigin>().CountAsync());
        Assert.Equal(summaryBefore, await db.Set<InventSum>().Select(x => x.Ordered).SingleAsync());
    }

    [Fact]
    public async Task Customer_return_request_cannot_exceed_selected_partial_invoice_line()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        invoiceLine.Qty = 5m;
        invoiceLine.InventQty = 5m;
        db.Add(new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = "SECOND-INVOICE", SalesId = invoiceLine.SalesId,
            InventTransId = invoiceLine.InventTransId, ItemId = invoiceLine.ItemId, Qty = 5m, InventQty = 5m });
        await db.SaveChangesAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        var result = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 3m, "CRONLY"), new(invoiceLine.RecId, 3m, "CRONLY")]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("selected invoice line", result.Message);
        Assert.Empty(await db.Set<SalesTable>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync());
    }

    [Fact]
    public async Task Customer_return_rejects_cumulative_excess_against_selected_partial_invoice()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        invoiceLine.Qty = 5m;
        invoiceLine.InventQty = 5m;
        db.Add(new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = "SECOND-INVOICE", SalesId = invoiceLine.SalesId,
            InventTransId = invoiceLine.InventTransId, ItemId = invoiceLine.ItemId, Qty = 5m, InventQty = 5m });
        await db.SaveChangesAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        Assert.Equal(200, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 4m, "CRONLY")]), default)).StatusCode);
        var second = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 2m, "CRONLY")]), default);
        Assert.Equal(422, second.StatusCode);
        Assert.Contains("remaining quantity", second.Message);
        Assert.Equal(-4m, (await db.Set<SalesLine>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync()).Sum(x => x.SalesQty));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Customer_return_keeps_attributed_partial_invoice_entitlements_separate(bool sameRequest)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var first = await db.Set<CustInvoiceTrans>().SingleAsync();
        first.Qty = first.InventQty = 5m;
        var secondInvoiceId = sameRequest ? invoiceId : "SECOND-INVOICE";
        if (!sameRequest)
            db.Add(new CustInvoiceJour { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId });
        var second = new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId,
            InventTransId = first.InventTransId, ItemId = first.ItemId, Qty = 5m, InventQty = 5m };
        db.Add(second);
        await db.SaveChangesAsync();
        var service = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        if (sameRequest)
            Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(invoiceId, null,
                [new(first.RecId, 4m, "CRONLY"), new(second.RecId, 4m, "CRONLY")]), default)).StatusCode);
        else
        {
            Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(invoiceId, null,
                [new(first.RecId, 4m, "CRONLY")]), default)).StatusCode);
            Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(secondInvoiceId, null,
                [new(second.RecId, 4m, "CRONLY")]), default)).StatusCode);
        }
        var returnLines = await db.Set<SalesLine>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync();
        Assert.Equal(2, returnLines.Count);
        Assert.Equal(-4m, Assert.Single(returnLines, x => x.OriginalInvoiceTransRecId == first.RecId).SalesQty);
        Assert.Equal(-4m, Assert.Single(returnLines, x => x.OriginalInvoiceTransRecId == second.RecId).SalesQty);
        Assert.Equal(422, (await service.CreateAsync(new CustReturnCreateDto(secondInvoiceId, null,
            [new(second.RecId, 2m, "CRONLY")]), default)).StatusCode);
    }

    [Fact]
    public async Task Customer_return_with_unattributed_legacy_line_retains_conservative_limit()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var first = await db.Set<CustInvoiceTrans>().SingleAsync();
        first.Qty = first.InventQty = 5m;
        const string secondInvoiceId = "SECOND-INVOICE";
        db.Add(new CustInvoiceJour { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId });
        var second = new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId,
            InventTransId = first.InventTransId, ItemId = first.ItemId, Qty = 5m, InventQty = 5m };
        db.Add(second);
        await db.SaveChangesAsync();
        var service = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(first.RecId, 4m, "CRONLY")]), default)).StatusCode);
        (await db.Set<SalesLine>().SingleAsync(x => x.SalesType == SalesType.ReturnItem)).OriginalInvoiceTransRecId = null;
        await db.SaveChangesAsync();

        var rejected = await service.CreateAsync(new CustReturnCreateDto(secondInvoiceId, null,
            [new(second.RecId, 2m, "CRONLY")]), default);
        Assert.Equal(422, rejected.StatusCode);
        Assert.Single(await db.Set<SalesLine>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync());
    }

    [Fact]
    public async Task Customer_return_costs_from_the_selected_invoice_across_partial_invoices()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        var origin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == invoiceLine.InventTransId);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == origin.RecId && x.StatusIssue == StatusIssue.Sold);
        issue.Qty = -5m;
        issue.CostAmountPosted = -300m;
        db.Add(new InventTrans { DataAreaId = "dat", InventTransOrigin = origin.RecId, ItemId = issue.ItemId,
            InventDimId = issue.InventDimId, Qty = -5m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -350m,
            InvoiceId = "SECOND-INVOICE", Voucher = "SECOND-INVOICE", DateFinancial = issue.DateFinancial, ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        var result = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 1, "CREDIT")]), default);

        // The return records the invoice line it came from, so the 60.00 cost of the selected
        // invoice is used instead of refusing the differing 70.00 cost of the other invoice.
        Assert.Equal(200, result.StatusCode);
        var returnLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        Assert.Equal(invoiceLine.RecId, returnLine.OriginalInvoiceTransRecId);

        Assert.Equal(-1m, returnLine.SalesQty);
    }

    [Fact]
    public async Task Customer_return_credits_received_goods_at_original_cost_and_credit_only_without_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().AddRange(
            new ReturnDispositionCode { RecId = 1, DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit },
            new ReturnDispositionCode { RecId = 2, DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly },
            new ReturnDispositionCode { RecId = 3, DataAreaId = "dat", DispositionCodeId = "SCRAP", DispositionAction = DispositionAction.Scrap });
        db.Set<ReturnReasonCode>().Add(new ReturnReasonCode { RecId = 1, DataAreaId = "dat", ReasonCodeId = "DAMAGED" });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers); // 100 @ 60 bought, 10 @ 100 + 15% VAT sold
        var sold = await db.Set<CustInvoiceTrans>().SingleAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        // Unsupported disposition and over-return are rejected.
        Assert.Equal(422, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, "DAMAGED", [new(sold.RecId, 1, "SCRAP")]), default)).StatusCode);
        var created = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, "DAMAGED",
            [new(sold.RecId, 2, "CREDIT"), new(sold.RecId, 1, "CRONLY")]), default);
        Assert.Equal(200, created.StatusCode);
        Assert.Equal(422, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null, [new(sold.RecId, 8, "CREDIT")]), default)).StatusCode);

        var rma = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        Assert.False(string.IsNullOrEmpty(rma.ReturnItemNum));
        var creditLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId && x.ReturnDispositionCodeId == "CREDIT");
        var creditOnlyLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId && x.ReturnDispositionCodeId == "CRONLY");
        Assert.Equal(-2m, creditLine.SalesQty);
        Assert.Equal(sold.InventTransId, creditLine.InventTransIdReturn);
        Assert.Equal(2m, (await db.Set<InventSum>().SingleAsync()).Ordered); // expected return receipt only for the Credit line

        // Receive the 2 credited units back at the original cost of 60; credit-only lines are not received.
        Assert.Equal(422, (await returns.ReceiveAsync(rma.RecId, new CustReturnReceiptDto("rr-x", null, [new(creditOnlyLine.RecId, 1)]), default)).StatusCode);
        Assert.Equal(200, (await returns.ReceiveAsync(rma.RecId, new CustReturnReceiptDto("rr-1", new DateTime(2026, 10, 20), [new(creditLine.RecId, 2)]), default)).StatusCode);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(92m, sum.PhysicalInvent);
        Assert.Equal(2m, sum.Received);
        Assert.Equal(120m, sum.PhysicalValue);

        // Credit note: revenue 300 + VAT 45 reversed; received goods financially restored at 120.
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        rma.CurrencyCode = "USD";
        await db.SaveChangesAsync();
        var rejectedCredit = await returns.CreditAsync(rma.RecId,
            new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-fx"), default);
        Assert.Equal(422, rejectedCredit.StatusCode);
        Assert.Empty(await db.Set<CustInvoiceJour>().Where(x => x.SalesId == rma.SalesId).ToListAsync());
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        rma.CurrencyCode = "SAR";
        var creditLedger = await db.Set<Ledger>().SingleAsync();
        creditLedger.ReportingCurrency = "USD";
        await db.SaveChangesAsync();
        var rejectedReporting = await returns.CreditAsync(rma.RecId,
            new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-reporting"), default);
        Assert.Equal(422, rejectedReporting.StatusCode);
        Assert.Empty(await db.Set<CustInvoiceJour>().Where(x => x.SalesId == rma.SalesId).ToListAsync());
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        creditLedger = await db.Set<Ledger>().SingleAsync();
        creditLedger.ReportingCurrency = "SAR";
        await db.SaveChangesAsync();
        var credit = await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-1"), default);
        Assert.Equal(200, credit.StatusCode);
        var note = await db.Set<CustInvoiceJour>().SingleAsync(x => x.SalesId == rma.SalesId);
        var returnOrigin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == creditLine.InventTransId);
        var returnPosting = await db.Set<InventTransPosting>().SingleAsync(x => x.InventTransOrigin == returnOrigin.RecId
            && x.InventTransPostingType == InventTransPostingType.Financial && x.Voucher == note.InvoiceId);
        Assert.Equal(LedgerPostingType.SalesIssue, returnPosting.PostingType);
        Assert.Equal(LedgerPostingType.SalesConsump, returnPosting.PostingTypeOffset);
        Assert.Equal(-300m, note.SalesBalance);
        Assert.Equal(-45m, note.SumTax);
        Assert.Equal(-345m, note.InvoiceAmount);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(92m, sum.PhysicalInvent);
        Assert.Equal(92m, sum.PostedQty);
        Assert.Equal(0m, sum.Received);
        Assert.Equal(0m, sum.PhysicalValue);
        Assert.Equal(5520m, sum.PostedValue); // 6,000 - 600 sold + 120 returned
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        Assert.Equal(ReturnStatusHeader.Closed, rma.ReturnStatus);
        Assert.All(await db.Set<SalesLine>().Where(x => x.SalesId == rma.SalesId).ToListAsync(),
            x => Assert.Equal(ReturnStatusLine.Invoiced, x.ReturnStatus));
        Assert.Equal(1150m - 345m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur)); // open balance after credit
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(null, "cn-1"), default)).StatusCode);
        Assert.Equal(2, await db.Set<CustInvoiceJour>().CountAsync()); // retry does not post again
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Vendor_return_ships_at_original_cost_and_vendor_credit_reduces_payable(bool taxable)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        if (taxable) SeedPurchaseTax(db);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, _, receipts, invoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default);
        var invoiced = await db.Set<VendInvoiceTrans>().SingleAsync();
        if (taxable)
        {
            // A later rate change must not change the tax reversed from the original invoice.
            (await db.Set<TaxData>().SingleAsync()).TaxValue = 20m;
            await db.SaveChangesAsync();
        }
        var returns = new PurchReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        Assert.Equal(422, (await returns.CreateAsync(new PurchReturnCreateDto("VI-1", null, [new(invoiced.RecId, 101)]), default)).StatusCode);
        Assert.Equal(200, (await returns.CreateAsync(new PurchReturnCreateDto("VI-1", "RMA-V-7", [new(invoiced.RecId, 10)]), default)).StatusCode);
        var ret = await db.Set<PurchTable>().SingleAsync(x => x.PurchaseType == PurchaseType.ReturnItem);
        var retLine = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == ret.PurchId);
        Assert.Equal(-10m, retLine.PurchQty);
        Assert.Equal(10m, (await db.Set<InventSum>().SingleAsync()).OnOrder);
        Assert.Equal(422, (await invoices.PostAsync(ret.RecId, new VendorInvoicePostDto("VI-X", null, [new(retLine.RecId, 1)]), default)).StatusCode);

        // Ship back 10 -> physical 90 (issue); then the vendor credit note finalizes it at the original 60.
        Assert.Equal(200, (await returns.ShipAsync(ret.RecId, new PurchReturnShipDto("RS-1", new DateTime(2026, 10, 5), [new(retLine.RecId, 10)]), default)).StatusCode);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(10m, sum.Deducted);
        Assert.Equal(0m, sum.OnOrder);
        var vendorCreditLedger = await db.Set<Ledger>().SingleAsync();
        vendorCreditLedger.ReportingCurrency = "USD";
        await db.SaveChangesAsync();
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId,
            new VendCreditNoteDto("VCN-FX", new DateTime(2026, 10, 6)), default)).StatusCode);
        Assert.Empty(await db.Set<VendInvoiceJour>().Where(x => x.PurchId == ret.PurchId).ToListAsync());
        vendorCreditLedger = await db.Set<Ledger>().SingleAsync();
        vendorCreditLedger.ReportingCurrency = "SAR";
        ret = await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId);
        ret.CurrencyCode = "USD";
        await db.SaveChangesAsync();
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId,
            new VendCreditNoteDto("VCN-FOREIGN", new DateTime(2026, 10, 6)), default)).StatusCode);
        Assert.Empty(await db.Set<VendInvoiceJour>().Where(x => x.PurchId == ret.PurchId).ToListAsync());
        ret = await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId);
        ret.CurrencyCode = "SAR";
        await db.SaveChangesAsync();
        Assert.Equal(200, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1", new DateTime(2026, 10, 6)), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(90m, sum.PostedQty);
        Assert.Equal(0m, sum.Deducted);
        Assert.Equal(5400m, sum.PostedValue);
        Assert.Equal(taxable ? -6210m : -5400m, (await db.Set<VendTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(taxable ? 810m : 0m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId)).PurchStatus);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1"), default)).StatusCode);
    }

    [Fact]
    public async Task Vendor_return_uses_selected_invoice_even_when_number_is_reused_and_costs_differ()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        db.Add(new VendInvoiceJour { DataAreaId = "dat", InvoiceId = "VI-2", InternalInvoiceId = "OTHER-ID",
            PurchId = "OTHER-PO", OrderAccount = "OTHER", InvoiceAccount = "OTHER", CurrencyCode = "SAR" });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, _, receipts, invoices) = PurchaseServices(db, numbers);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 20, "PCS", 60, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(line.RecId, 10, 60)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("VI-2", new DateTime(2026, 10, 4), [new(line.RecId, 10, 70)]), default)).StatusCode);
        var selected = await db.Set<VendInvoiceTrans>().SingleAsync(x => x.InvoiceId == "VI-2");
        var returnSequence = Sequence();
        await returnSequence.NextAsync("PurchTable", "dat", default);
        var returns = new PurchReturnService(new TestUnitOfWork(db), new TestCompany(), returnSequence, numbers);
        var create = await returns.CreateAsync(new PurchReturnCreateDto("VI-2", null, [new(selected.RecId, 2)]), default);
        Assert.Equal(200, create.StatusCode);
        var returnOrder = await db.Set<PurchTable>().SingleAsync(x => x.PurchaseType == PurchaseType.ReturnItem);
        Assert.Equal("V-0001", returnOrder.InvoiceAccount);
        var returnLine = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == returnOrder.PurchId);
        Assert.Equal(selected.RecId, returnLine.OriginalInvoiceTransRecId);
        Assert.Equal(200, (await returns.ShipAsync(returnOrder.RecId,
            new PurchReturnShipDto("RETURN-1", new DateTime(2026, 10, 5), [new(returnLine.RecId, 2)]), default)).StatusCode);
        var origin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == returnLine.InventTransId);
        Assert.Equal(-140m, (await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == origin.RecId)).CostAmountPhysical);
        Assert.Equal(200, (await returns.CreditAsync(returnOrder.RecId, new VendCreditNoteDto("CREDIT-1", new DateTime(2026, 10, 6)), default)).StatusCode);
        Assert.Equal(-140m, (await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == origin.RecId)).CostAmountPosted);
        var persistedReturn = await db.Set<PurchLine>().AsNoTracking().SingleAsync(x => x.RecId == returnLine.RecId);
        Assert.Equal(selected.RecId, persistedReturn.OriginalInvoiceTransRecId);
        Assert.Equal(selected.InventTransId, persistedReturn.InventTransIdReturn);
        var selectedReceipt = await db.Set<InventTrans>().AsNoTracking().SingleAsync(x => x.InvoiceId == selected.InternalInvoiceId && x.StatusReceipt == StatusReceipt.Purchased);
        Assert.Equal(10m, selectedReceipt.Qty);
        var close = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM-001", new DateTime(2026, 10, 31));
        Assert.Null(close.Error);
        var settlement = Assert.Single(await db.Set<InventSettlement>().Where(x => x.TransRecId != 0).ToListAsync());
        var settledReceipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == settlement.ReceiptTransRecId);
        Assert.Equal(selected.InternalInvoiceId, settledReceipt.InvoiceId);
        Assert.Equal(-140m, settlement.CostAmountSettled);
    }

    // ---- Sales order cancellation of a partly delivered order ----

    /// <summary>
    /// The case the order-to-cash cycle had no ending for. Order 10, deliver 6, invoice nothing,
    /// then cancel: the 4 that will never ship is released along with its reservation, and the 6
    /// already delivered stays invoiceable. Cancelling demand touches no physical stock and posts
    /// no ledger entry.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_partly_delivered_order_releases_the_remainder_and_keeps_the_delivery_invoiceable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);

        var beforeStock = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(4m, beforeStock.ReservPhysical); // the undelivered 4 are still reserved
        var ledgerCount = await db.Set<GeneralJournalAccountEntry>().CountAsync();

        var cancelled = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.Cancelled, cancelled.Status);
        line = await db.Set<SalesLine>().SingleAsync();
        var sum = await db.Set<InventSum>().SingleAsync();
        // The reservation and the ordered demand behind the undelivered 4 are both released.
        Assert.Equal(0m, sum.ReservPhysical);
        Assert.Equal(0m, sum.OnOrder);
        Assert.Equal(0m, line.RemainSalesPhysical);
        // The delivery is still waiting to be invoiced, which is the whole point.
        Assert.Equal(6m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Delivered, line.SalesStatus);
        // Physical stock and the ledger are untouched: 94 left after shipping 6 of 100.
        Assert.Equal(94m, sum.PhysicalInvent);
        Assert.Equal(ledgerCount, await db.Set<GeneralJournalAccountEntry>().CountAsync());

        // And the delivered 6 still invoice afterwards, closing the order properly.
        var invoiced = await new SalesInvoiceService(work, company, Sequence())
            .PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 8), "inv-cancel", [new(line.RecId, 6)]), default);
        Assert.Equal(200, invoiced.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync(x => x.SalesId == order.SalesId)).SalesStatus);
    }

    /// <summary>
    /// Picking is a physical registration, so cancellation refuses it by name and leaves it to the
    /// existing unpick path rather than silently reversing it. After unpicking, the same
    /// cancellation succeeds.
    /// </summary>
    [Fact]
    public async Task Cancelling_an_order_with_a_picked_quantity_is_refused_until_it_is_unpicked()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);
        var reservation = new InventoryReservationService(work, company);
        // Pick 3 of the 4 still undelivered.
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 3m, default)).Error);

        var refused = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.Refused, refused.Status);
        Assert.Contains("must be unpicked", refused.Message);
        // Rolled back: the line keeps its remainder and the pick is still there.
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(3m, (await db.Set<InventSum>().SingleAsync()).Picked);

        // Unpick through the documented path, and the cancellation then goes through.
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 0m, default)).Error);
        var cancelled = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);
        Assert.Equal(SalesOrderCancelStatus.Cancelled, cancelled.Status);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(6m, line.RemainSalesFinancial);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).Picked);
    }

    /// <summary>
    /// A fully delivered and invoiced order has no remainder, so cancellation is refused by name
    /// rather than quietly doing nothing.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_fully_delivered_order_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);
        // Deliver and invoice everything.
        Assert.Equal(200, (await new PackingSlipJournalService(work, company, Sequence()).PostAsync(order.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 10, 8), "All", true, "ps-rest",
                [new PackingSlipPostingLineRequest(line.RecId.ToString(), 4)]), default)).StatusCode);
        Assert.Equal(200, (await new SalesInvoiceService(work, company, Sequence()).PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 10, 9), "inv-all", [new(line.RecId, 10)]), default)).StatusCode);

        var refused = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.NothingToCancel, refused.Status);
        Assert.Contains("no undelivered quantity left to cancel", refused.Message);
    }

    /// <summary>
    /// Order 10, deliver 6, invoice 6, then cancel the rest: nothing is left owing, so the line and
    /// the order both reach Invoiced rather than Canceled - part of the order really happened.
    /// </summary>
    [Fact]
    public async Task Cancelling_the_rest_of_a_fully_invoiced_delivery_closes_the_order_as_invoiced()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);
        Assert.Equal(200, (await new SalesInvoiceService(work, company, Sequence()).PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-6", [new(line.RecId, 6)]), default)).StatusCode);

        var cancelled = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.Cancelled, cancelled.Status);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, line.SalesStatus);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync(x => x.SalesId == order.SalesId)).SalesStatus);
    }

    /// <summary>
    /// Stocks 100 and sells 10, reserving all of them and delivering 6, so the order is left partly
    /// delivered with 4 reserved and nothing invoiced.
    /// </summary>
    private static async Task<(SalesTable Order, SalesLine Line, SalesInventoryDemandService Demand)>
        PartlyDeliveredSalesOrderAsync(CycleContext db, TestUnitOfWork work, TestCompany company, TestInventNumbers numbers)
    {
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        await db.SaveChangesAsync();

        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default)).StatusCode);

        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-PARTIAL", CustAccount = "C-001", InvoiceAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S01", InventLocationId = "WH-01",
            OrderDate = new DateTime(2026, 10, 5), PaymTerm = "NET30"
        };
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "NET30", PaymMethod = PaymMethod.Net, NumOfDays = 30 });
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, SalesUnit = "PCS", SalesPrice = 100, PriceUnit = 1,
            LineAmount = 1000, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, CurrencyCode = "SAR",
            TaxGroup = "VAT15", TaxItemGroup = "VAT15", ReceiptDateRequested = new DateTime(2026, 10, 10)
        };
        var demand = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        await demand.CreateAsync(order, line, "S01", "WH-01");
        db.Set<SalesLine>().Add(line);
        await db.SaveChangesAsync();

        var reservation = new InventoryReservationService(work, company);
        Assert.Null((await reservation.ReserveAsync(line.InventTransId, 10m, false, default)).Error);
        // Deliver 6, leaving 4 reserved and undelivered and all 10 still to invoice.
        Assert.Equal(200, (await new PackingSlipJournalService(work, company, Sequence()).PostAsync(order.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 10, 6), "All", true, "ps-partial",
                [new PackingSlipPostingLineRequest(line.RecId.ToString(), 6)]), default)).StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(10m, line.RemainSalesFinancial);
        return (order, line, demand);
    }

    [Fact]
    public async Task Cancellation_releases_split_reserved_and_ordered_demand_without_stock_or_ledger_changes()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        await StockAndSellTenAsync(db, numbers);
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var order = new SalesTable { DataAreaId = "dat", SalesId = "SO-CANCEL", CustAccount = "C-001", SalesStatus = SalesStatus.Backorder };
        db.Add(order);
        var line = new SalesLine { DataAreaId = "dat", SalesId = order.SalesId, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes };
        var demand = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        await demand.CreateAsync(order, line, "S01", "WH-01");
        db.Add(line);
        await db.SaveChangesAsync();
        var reservation = new InventoryReservationService(work, company);
        Assert.Null((await reservation.ReserveAsync(line.InventTransId, 10, false, default)).Error);
        Assert.Null((await reservation.ReleaseAsync(line.InventTransId, 4, default)).Error);
        var ledgerCount = await db.Set<GeneralJournalAccountEntry>().CountAsync();
        Assert.Equal(SalesOrderCancelStatus.Cancelled, (await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default)).Status);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, sum.ReservPhysical);
        Assert.Equal(0m, sum.OnOrder);
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(5400m, sum.PostedValue);
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(ledgerCount, await db.Set<GeneralJournalAccountEntry>().CountAsync());
    }

    // Stocks 100 @ 60 through the purchase cycle and sells/ships/invoices 10 @ 100 (+15% VAT); returns the invoice id.
    // The sales side can be put in another currency to exercise translation; buying stays in the
    // accounting currency so the inventory cost is unambiguous.
    private static async Task<string> StockAndSellTenAsync(CycleContext db, TestInventNumbers numbers,
        string salesCurrency = "SAR")
    {
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        await vendorInvoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default);

        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-0001", CustAccount = "C-001", InvoiceAccount = "C-001", CurrencyCode = salesCurrency,
            SalesStatus = SalesStatus.Backorder, SalesType = SalesType.Sales, InventSiteId = "S01", InventLocationId = "WH-01",
            OrderDate = new DateTime(2026, 10, 5)
        };
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, RemainInventPhysical = 10, RemainInventFinancial = 10,
            SalesUnit = "PCS", SalesPrice = 100, PriceUnit = 1, LineAmount = 1000, SalesStatus = SalesStatus.Backorder,
            SalesType = SalesType.Sales, StockedProduct = NoYes.Yes, CurrencyCode = salesCurrency, TaxGroup = "VAT15", TaxItemGroup = "VAT15",
            ReceiptDateRequested = new DateTime(2026, 10, 10)
        };
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers)).CreateAsync(order, line, "S01", "WH-01");
        db.Set<SalesLine>().Add(line);
        await db.SaveChangesAsync();
        Assert.Equal(200, (await new PackingSlipJournalService(work, company, Sequence()).PostAsync(order.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 10, 6), "All", true, "ps-1",
                [new PackingSlipPostingLineRequest(line.RecId.ToString(), 10)]), default)).StatusCode);
        Assert.Equal(200, (await new SalesInvoiceService(work, company, Sequence()).PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-1"), default)).StatusCode);
        return (await db.Set<CustInvoiceJour>().SingleAsync()).InvoiceId;
    }

    private static void SeedSalesMasterData(CycleContext db)
    {
        db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "SAR" });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "220100" },  // output VAT
            new MainAccount { RecId = 5, DataAreaId = "dat", MainAccountId = "410100" },  // sales revenue
            new MainAccount { RecId = 6, DataAreaId = "dat", MainAccountId = "120100" },  // accounts receivable
            new MainAccount { RecId = 7, DataAreaId = "dat", MainAccountId = "110100" }); // bank
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, DisplayValue = "220100" },
            new DimensionAttributeValueCombination { RecId = 5, DataAreaId = "dat", MainAccount = 5, DisplayValue = "410100" },
            new DimensionAttributeValueCombination { RecId = 6, DataAreaId = "dat", MainAccount = 6, DisplayValue = "120100" },
            new DimensionAttributeValueCombination { RecId = 7, DataAreaId = "dat", MainAccount = 7, DisplayValue = "110100" });
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 10, DataAreaId = "dat", InventAccountType = InventAccountType.SalesRevenue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 5 },
            new InventPosting { RecId = 11, DataAreaId = "dat", InventAccountType = InventAccountType.SalesConsump, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 },
            new InventPosting { RecId = 12, DataAreaId = "dat", InventAccountType = InventAccountType.SalesIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 });
        db.Set<CustLedgerAccounts>().Add(new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = 6 });
        db.Set<TaxGroupData>().Add(new TaxGroupData { RecId = 1, DataAreaId = "dat", TaxGroup = "VAT15", TaxCode = "VAT15C" });
        db.Set<TaxOnItem>().Add(new TaxOnItem { RecId = 1, DataAreaId = "dat", TaxItemGroup = "VAT15", TaxCode = "VAT15C" });
        db.Set<TaxData>().Add(new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15C", TaxValue = 15m });
        db.Set<TaxTable>().Add(new TaxTable { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15C", TaxAccountGroup = "VATG" });
        db.Set<TaxLedgerAccountGroup>().Add(new TaxLedgerAccountGroup { RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VATG", TaxOutgoingLedgerDimension = 4 });
        db.Set<BankAccountTable>().Add(new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 7, CurrencyCode = "SAR" });
    }

    private static void SeedPurchaseTax(CycleContext db)
    {
        db.Set<VendTable>().Local.Single().TaxGroup = "VAT15";
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).TaxItemGroupId = "VAT15";
        db.Add(new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "INPUTVAT" });
        db.Add(new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, DisplayValue = "INPUTVAT" });
        db.Add(new TaxGroupData { RecId = 1, DataAreaId = "dat", TaxGroup = "VAT15", TaxCode = "VAT15C" });
        db.Add(new TaxOnItem { RecId = 1, DataAreaId = "dat", TaxItemGroup = "VAT15", TaxCode = "VAT15C" });
        db.Add(new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15C", TaxValue = 15m });
        db.Add(new TaxTable { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15C", TaxAccountGroup = "VATG" });
        db.Add(new TaxLedgerAccountGroup { RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VATG", TaxIncomingLedgerDimension = 4 });
    }
    [Fact]
    public async Task Purchase_order_prepayment_definition_respects_the_order_limit_and_is_repeatable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        var category = new EcoResCategory { Name = "Purchase prepayment", IsActive = true };
        db.Add(category);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", 10m, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        Assert.Equal(422, (await orders.DefinePrepaymentAsync(order.RecId, new(1100m, category.RecId, "Deposit"), default)).StatusCode);
        Assert.Empty(await db.Set<PurchPrepayTable>().ToListAsync());
        var input = new PurchPrepaymentDefinitionDto(300m, category.RecId, "Deposit");
        Assert.Equal(200, (await orders.DefinePrepaymentAsync(order.RecId, input, default)).StatusCode);
        Assert.Equal(200, (await orders.DefinePrepaymentAsync(order.RecId, input, default)).StatusCode);
        var definition = await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync();
        Assert.Equal(order.PurchId, definition.PurchTable);
        Assert.Equal("SAR", definition.Currency);
        Assert.Equal(1, definition.Type);
        Assert.Equal(300m, definition.PrepayAvailable);
        Assert.Equal(1000m, definition.PrepayLimit);
        Assert.Equal(0m, definition.PrepayApplicationRemaining);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Fact]
    public async Task Purchase_order_prepayment_invoice_uses_the_configured_advance_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var category = new EcoResCategory { Name = "Purchase prepayment", IsActive = true };
        db.Add(category);
        await db.SaveChangesAsync();
        db.Add(new VendParameters { DataAreaId = "dat", VendPostingProfile = "GEN", AdvancePostingProfile = "ADV" });
        db.Add(new VendLedgerAccounts { DataAreaId = "dat", PostingProfile = "ADV", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 });
        db.Add(new InventPosting { DataAreaId = "dat", InventAccountType = InventAccountType.PurchAdvance,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3, CategoryRelation = category.RecId + 1 });
        db.Add(new InventPosting { DataAreaId = "dat", InventAccountType = InventAccountType.PurchAdvance,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1, CategoryRelation = category.RecId });
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        var paymentNumbers = Sequence();
        var invoices = new VendorInvoiceService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), paymentNumbers);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", 10m, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await orders.DefinePrepaymentAsync(order.RecId, new(300m, category.RecId, "Deposit"), default)).StatusCode);
        Assert.Equal(422, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 350m, default)).StatusCode);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
        var posted = await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 300m, default);
        Assert.True(posted.StatusCode == 200, posted.Message);
        Assert.Equal(200, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 300m, default)).StatusCode);
        Assert.Equal(422, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 299m, default)).StatusCode);
        Assert.Equal(422, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-2", date, 1m, default)).StatusCode);

        var invoice = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync();
        Assert.Equal(NoYes.Yes, invoice.Prepayment);
        Assert.Equal(order.PurchId, invoice.PurchId);
        Assert.Equal(-300m, (await db.Set<VendTransOpen>().SingleAsync()).AmountCur);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(300m, Assert.Single(entries, x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(-300m, Assert.Single(entries, x => x.LedgerDimension == 2).AccountingCurrencyAmount);
        Assert.Equal(0m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayAvailable);
        db.Add(new BankAccountTable { DataAreaId = "dat", AccountId = "PREPAY-BANK", LedgerDimension = 90, CurrencyCode = "SAR" });
        var profile = await db.Set<VendLedgerAccounts>().SingleAsync(x => x.PostingProfile == "ADV");
        profile.SummaryLedgerDimension = 3;
        await db.SaveChangesAsync();
        var payments = new VendPaymentService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), paymentNumbers);
        var payment = new VendPaymentPostDto("V-0001", "PREPAY-BANK", 300m, "ADV-PAY", date.AddDays(1), SettleInvoiceId: "ADV-1");
        Assert.Equal(422, (await payments.PostAsync(payment with { Amount = 301m }, default)).StatusCode);
        var paid = await payments.PostAsync(payment, default);
        Assert.True(paid.StatusCode == 200, paid.Message);
        Assert.Equal(200, (await payments.PostAsync(payment, default)).StatusCode);
        Assert.Empty(await db.Set<VendTransOpen>().ToListAsync());
        Assert.Equal(300m, (await db.Set<VendSettlement>().SingleAsync()).AmountCur);
        var paymentLine = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.PaymReference == "ADV-PAY");
        Assert.Equal(2, paymentLine.LedgerDimension);
        Assert.Equal(300m, paymentLine.AmountCurDebit);
        var reversalService = new VendPaymentReversalService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), paymentNumbers);
        var reversed = await reversalService.ReverseAsync(paymentLine.JournalNum, date.AddDays(2), default);
        Assert.True(reversed.StatusCode == 200, reversed.Message);
        var reopened = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync();
        Assert.Equal(-300m, reopened.AmountCur);
        var reversalJournal = await db.Set<LedgerJournalTable>().SingleAsync(x => x.OriginalJournalNum == paymentLine.JournalNum);
        var reversalLine = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.JournalNum == reversalJournal.JournalNum);
        Assert.Equal(2, reversalLine.LedgerDimension);
        Assert.Equal(300m, reversalLine.AmountCurCredit);
        Assert.Equal(10m, (await db.Set<PurchLine>().AsNoTracking().SingleAsync()).RemainPurchFinancial);
        Assert.Equal(0m, (await db.Set<InventSum>().AsNoTracking().SingleAsync()).PhysicalInvent);
        Assert.Equal(0m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);

        var (_, _, receipts, _) = PurchaseServices(db);
        var purchaseLine = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new("ADV-R", date.AddDays(2), [new(purchaseLine.RecId, 10m)]), default)).StatusCode);
        var finalPosted = await invoices.PostAsync(order.RecId, new("ADV-FINAL", date.AddDays(3), [new(purchaseLine.RecId, 10m)]), default);
        Assert.True(finalPosted.StatusCode == 200, finalPosted.Message);
        Assert.Equal(200, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 300m, default)).StatusCode);
        var final = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync(x => x.InvoiceId == "ADV-FINAL");
        Assert.Equal(422, (await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(4), 200m, default)).StatusCode);
        var repaid = await payments.PostAsync(payment with { RequestId = "ADV-PAY-2", PaymentDate = date.AddDays(4) }, default);
        Assert.True(repaid.StatusCode == 200, repaid.Message);
        Assert.Equal(300m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
        Assert.Equal(422, (await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(5), 301m, default)).StatusCode);
        var applied = await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(5), 200m, default);
        Assert.True(applied.StatusCode == 200, applied.Message);
        Assert.Equal(200, (await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(5), 200m, default)).StatusCode);
        var appliedMatch = Assert.IsType<VendSettlement>(applied.Data);
        var appliedEntry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.SubledgerVoucher == appliedMatch.Voucher);
        var transfer = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == appliedEntry.RecId).ToListAsync();
        Assert.Equal(200m, Assert.Single(transfer, x => x.LedgerDimension == 3).AccountingCurrencyAmount);
        Assert.Equal(-200m, Assert.Single(transfer, x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(-800m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync()).AmountCur);
        Assert.Equal(100m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
        var secondPayment = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.PaymReference == "ADV-PAY-2");
        Assert.Equal(422, (await reversalService.ReverseAsync(secondPayment.JournalNum, date.AddDays(6), default)).StatusCode);

        var undo = await invoices.ReversePrepaymentApplicationAsync(order.RecId, appliedMatch.RecId, date.AddDays(6), default);
        Assert.True(undo.StatusCode == 200, undo.Message);
        Assert.Equal(200, (await invoices.ReversePrepaymentApplicationAsync(order.RecId, appliedMatch.RecId, date.AddDays(6), default)).StatusCode);
        Assert.Equal(300m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
        Assert.Equal(-1000m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync()).AmountCur);
        var undoMatch = Assert.IsType<VendSettlement>(undo.Data);
        var undoEntry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.SubledgerVoucher == undoMatch.Voucher);
        var undoLines = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == undoEntry.RecId).ToListAsync();
        Assert.Equal(-200m, Assert.Single(undoLines, x => x.LedgerDimension == 3).AccountingCurrencyAmount);
        Assert.Equal(200m, Assert.Single(undoLines, x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        var retryReversePayment = await reversalService.ReverseAsync(secondPayment.JournalNum, date.AddDays(7), default);
        Assert.True(retryReversePayment.StatusCode == 200, retryReversePayment.Message);
        Assert.Equal(0m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
    }

    [Fact]
    public async Task Receipt_accrual_reverses_only_matched_quantities_using_original_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 60, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("ACCRUAL-1", new DateTime(2026, 10, 2), [new(line.RecId, 60)]), default)).StatusCode);
        var accrual = await db.Set<PurchReceiptAccrual>().SingleAsync();
        Assert.Equal(3000m, accrual.AmountMst);
        Assert.Equal(0m, accrual.ReversedAmountMst);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("ACCRUAL-I1", new DateTime(2026, 10, 3), [new(line.RecId, 40)]), default)).StatusCode);
        Assert.Equal(2000m, accrual.ReversedAmountMst);
        Assert.Equal(40m, accrual.ReversedInventQuantity);
        // Later setup edits must not redirect the reversal of the original liability.
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.PurchPackingSlipOffsetAccount)).LedgerDimension = 2;
        await db.SaveChangesAsync();
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("ACCRUAL-I2", new DateTime(2026, 10, 4), [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(3000m, accrual.ReversedAmountMst);
        Assert.Equal(60m, accrual.ReversedInventQuantity);
        var matches = await db.Set<VendInvoicePackingSlipQuantityMatch>().ToListAsync();
        Assert.Equal(3000m, matches.Sum(x => x.AccrualReversalAmountMst));
        Assert.All(matches, x => Assert.Equal(accrual.RecId, x.ReceiptAccrualRecId));
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.DoesNotContain(entries, x => x.LedgerDimension == 2);
        Assert.Equal(3000m, entries.Where(x => x.LedgerDimension == 1).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(-3000m, (await db.Set<VendTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        // The receipt's posting record drove the reversal accounts and is retired once fully matched.
        var posting = await db.Set<InventTransPosting>().SingleAsync(x => x.InventTransPostingType == InventTransPostingType.Physical);
        Assert.Equal(1, posting.LedgerDimension);
        Assert.Equal(3, posting.OffsetLedgerDimension);
        Assert.Equal(NoYes.No, posting.IsPosted);
        Assert.Equal(422, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("ACCRUAL-I2", null, [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(2, await db.Set<VendInvoicePackingSlipQuantityMatch>().CountAsync());
    }

    [Fact]
    public async Task Partial_receipt_accrual_reversals_do_not_exceed_the_original_amount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 4, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("ACCRUAL-ROUND", new DateTime(2026, 10, 2), [new(line.RecId, 4)]), default)).StatusCode);
        var accrual = await db.Set<PurchReceiptAccrual>().SingleAsync();
        accrual.AmountMst = 0.0002m;

        var posting = new PurchaseReceiptAccrualPosting(db);
        foreach (var expected in new[] { 0.0001m, 0m, 0.0001m, 0m })
        {
            var match = new VendInvoicePackingSlipQuantityMatch { PackingSlipTransRecId = accrual.PackingSlipTransRecId, InventQuantity = 1m };
            await posting.ReverseAsync("dat", match, accrual.GeneralJournalEntryRecId, "SAR", default);
            Assert.Equal(expected, match.AccrualReversalAmountMst);
        }
        Assert.Equal(accrual.AmountMst, accrual.ReversedAmountMst);
        Assert.Equal(accrual.InventQuantity, accrual.ReversedInventQuantity);
    }

    [Fact]
    public async Task Correcting_a_receipt_preserves_history_and_reverses_only_the_removed_accrual()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", 100m, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new("RCPT-CORRECT", date.AddDays(1), [new(line.RecId, 60m)]), default)).StatusCode);
        var receipt = await db.Set<VendPackingSlipJour>().AsNoTracking().SingleAsync();
        var source = await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync();

        var bad = await receipts.CorrectAsync(receipt.RecId, new(source.RecId, 70m, date.AddDays(2)), default);
        Assert.Equal(422, bad.StatusCode);
        Assert.Equal(60m, (await db.Set<InventSum>().AsNoTracking().SingleAsync()).PhysicalInvent);
        Assert.Single(await db.Set<VendPackingSlipVersion>().ToListAsync());
        var correction = new ProductReceiptCorrectionDto(source.RecId, 50m, date.AddDays(2));
        // A stock rejection occurs after the reversal voucher is staged. The transaction must undo it.
        var reserved = await db.Set<InventSum>().SingleAsync();
        reserved.ReservPhysical = 55m;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await receipts.CorrectAsync(receipt.RecId, correction, default)).StatusCode);
        Assert.Single(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Single(await db.Set<VendPackingSlipVersion>().ToListAsync());
        Assert.Equal(0m, (await db.Set<PurchReceiptAccrual>().AsNoTracking().SingleAsync()).ReversedAmountMst);
        Assert.Equal(60m, (await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync()).Qty);
        reserved = await db.Set<InventSum>().SingleAsync();
        reserved.ReservPhysical = 0m;
        await db.SaveChangesAsync();
        var result = await receipts.CorrectAsync(receipt.RecId, correction, default);
        Assert.True(result.StatusCode == 200, result.Message);
        Assert.Equal(200, (await receipts.CorrectAsync(receipt.RecId, correction, default)).StatusCode);

        var versions = await db.Set<VendPackingSlipVersion>().OrderBy(x => x.VersionDateTime).ToListAsync();
        Assert.Equal(new[] { 60m, 50m }, versions.Select(x => x.Qty));
        Assert.NotEqual(versions[0].LedgerVoucher, versions[1].LedgerVoucher);
        var history = await db.Set<VendPackingSlipTransHistory>().OrderBy(x => x.ValidFrom).ToListAsync();
        Assert.Equal(new[] { 6000m, 5000m }, history.Select(x => x.ValueMst));
        Assert.Equal(history[0].ValidTo, history[1].ValidFrom);
        var current = await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, current.Qty);
        Assert.Equal(50m, (await db.Set<PurchLine>().AsNoTracking().SingleAsync()).RemainPurchPhysical);
        var stock = await db.Set<InventSum>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, stock.PhysicalInvent);
        Assert.Equal(50m, stock.Ordered);
        Assert.Equal(5000m, stock.PhysicalValue);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        var ledger = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(5000m, ledger.Where(x => x.LedgerDimension == 1).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(-5000m, ledger.Where(x => x.LedgerDimension == 3).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new("CORRECT-I", date.AddDays(3), [new(line.RecId, 50m)]), default)).StatusCode);
        Assert.Equal(6000m, (await db.Set<PurchReceiptAccrual>().AsNoTracking().SingleAsync()).ReversedAmountMst);
    }

    [Fact]
    public async Task Receipt_cancellation_reverses_every_line_in_one_transaction()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        foreach (var qty in new[] { 20m, 30m })
            Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", qty, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        var lines = await db.Set<PurchLine>().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new("CANCEL-TWO", date.AddDays(1), lines.Select(x => new PurchPostLineDto(x.RecId, x.PurchQty)).ToList()), default)).StatusCode);
        var receipt = await db.Set<VendPackingSlipJour>().AsNoTracking().SingleAsync();

        var stock = await db.Set<InventSum>().SingleAsync();
        stock.ReservPhysical = 25m;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await receipts.CancelAsync(receipt.RecId, date.AddDays(2), default)).StatusCode);
        Assert.Equal(50m, (await db.Set<InventSum>().AsNoTracking().SingleAsync()).PhysicalInvent);
        Assert.Equal(2, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Single(await db.Set<VendPackingSlipVersion>().ToListAsync());
        Assert.All(await db.Set<PurchReceiptAccrual>().AsNoTracking().ToListAsync(), x => Assert.Equal(0m, x.ReversedAmountMst));

        stock = await db.Set<InventSum>().SingleAsync();
        stock.ReservPhysical = 0m;
        await db.SaveChangesAsync();
        var result = await receipts.CancelAsync(receipt.RecId, date.AddDays(2), default);
        Assert.True(result.StatusCode == 200, result.Message);
        Assert.Equal(200, (await receipts.CancelAsync(receipt.RecId, date.AddDays(2), default)).StatusCode);
        stock = await db.Set<InventSum>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, stock.Ordered);
        Assert.Equal(0m, stock.PhysicalInvent);
        Assert.Equal(0m, stock.PhysicalValue);
        Assert.Equal(3, await db.Set<GeneralJournalEntry>().CountAsync());
        var versions = await db.Set<VendPackingSlipVersion>().OrderBy(x => x.VersionDateTime).ToListAsync();
        Assert.Equal(new[] { 50m, 0m }, versions.Select(x => x.Qty));
        Assert.All(await db.Set<VendPackingSlipTrans>().AsNoTracking().ToListAsync(), x => Assert.Equal(0m, x.Qty));
        Assert.All(await db.Set<PurchReceiptAccrual>().AsNoTracking().ToListAsync(), x => Assert.Equal(x.AmountMst, x.ReversedAmountMst));
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
    }

    [Fact]
    public async Task Invoice_spanning_receipts_reverses_each_receipts_original_accrual_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 60, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("RCPT-1", new DateTime(2026, 10, 2), [new(line.RecId, 40)]), default)).StatusCode);
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.PurchPackingSlipOffsetAccount)).LedgerDimension = 2;
        await db.SaveChangesAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("RCPT-2", new DateTime(2026, 10, 3), [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("INV-ALL", new DateTime(2026, 10, 4), [new(line.RecId, 60)]), default)).StatusCode);

        var accruals = await db.Set<PurchReceiptAccrual>().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(new long[] { 3, 2 }, accruals.Select(x => x.AccrualLedgerDimension));
        Assert.All(accruals, x => Assert.Equal(x.InventQuantity, x.ReversedInventQuantity));
        var postings = await db.Set<InventTransPosting>().Where(x => x.InventTransPostingType == InventTransPostingType.Physical).ToListAsync();
        Assert.Equal(2, postings.Count);
        Assert.All(postings, x => Assert.Equal(NoYes.No, x.IsPosted));
        var liabilityLines = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Where(x =>
            x.PostingType == LedgerPostingType.LedgerJournal && (x.LedgerDimension == 2 || x.LedgerDimension == 3));
        Assert.All(liabilityLines.GroupBy(x => x.LedgerDimension), x => Assert.Equal(0m, x.Sum(y => y.AccountingCurrencyAmount)));
    }

    // ---- Invoice price matching tolerance (D365: AP parameters > Invoice validation) ----

    /// <summary>
    /// Order 100 @ 50 and receive it, so only the price side of matching is left to test.
    /// </summary>
    private static async Task<(VendorInvoiceService Invoices, PurchTable Order, PurchLine Line)> ReceivedOrderForMatchingAsync(
        CycleContext db, MatchingPolicy itemPolicy, MatchingPolicy vendorPolicy, decimal? tolerance,
        Action<VendParameters>? configure = null)
    {
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        db.Set<InventTable>().Local.Single().MatchingPolicy = itemPolicy;
        db.Set<VendTable>().Local.Single().MatchingPolicy = vendorPolicy;
        if (tolerance != null || configure != null)
        {
            var parameters = new VendParameters
            {
                RecId = 1, DataAreaId = "dat", Key = 0, TotalPriceTolerancePercentage = tolerance ?? 0m
            };
            configure?.Invoke(parameters);
            db.Set<VendParameters>().Add(parameters);
        }
        await db.SaveChangesAsync();
        var (orders, confirm, receipts, invoices) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR",
            new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await confirm.ConfirmAsync(po.RecId, default)).StatusCode);
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 100)]), default)).StatusCode);
        return (invoices, po, await db.Set<PurchLine>().SingleAsync());
    }

    // ---- Net unit price (extended price) matching: D365 USEEXTENDEDPRICEMATCHING ----
    // Price totals matching is switched off in these tests so the unit-price comparison is what is
    // being asserted; the two are independent gates and an order can fail either.

    [Fact]
    public async Task A_net_unit_price_within_the_configured_percentage_posts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.ExtendedPriceTolerancePercentage = 5m;
                x.UseTotalPriceMatching = NoYes.No;
            });

        // A unit price of 51 against 50 is a 2% variance, inside the 5% tolerance.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default)).StatusCode);
        Assert.Equal(5100m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task A_net_unit_price_outside_the_configured_percentage_is_refused_with_the_variance()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.ExtendedPriceTolerancePercentage = 5m;
                x.UseTotalPriceMatching = NoYes.No;
            });

        // 54 against 50 is 8%, outside 5%.
        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice net unit price for item ITEM-001 differs from the purchase order by 8%, "
            + "which exceeds the configured net unit price tolerance of 5%.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    /// <summary>
    /// The decision this pins: D365 holds both a percentage and an absolute amount, and <b>any</b>
    /// configured tolerance that is exceeded fails the match. A generous percentage therefore does
    /// not let a line through that the amount tolerance refuses, so configuring a second tolerance
    /// cannot widen the effective one.
    /// </summary>
    [Fact]
    public async Task The_net_unit_price_amount_tolerance_is_enforced_alongside_the_percentage()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.ExtendedPriceTolerancePercentage = 50m;   // 8% would pass this on its own
                x.ExtendedPriceToleranceAmount = 2m;        // but 4 per unit breaks this
                x.UseTotalPriceMatching = NoYes.No;
            });

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice net unit price for item ITEM-001 differs from the purchase order by 4, "
            + "which exceeds the configured net unit price tolerance of 2.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());

        // Inside both tolerances the same line posts, so the refusal above is the amount and not
        // the switch simply refusing everything.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-2", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default)).StatusCode);
    }

    [Fact]
    public async Task Net_unit_price_matching_with_no_tolerance_configured_requires_an_exact_unit_price()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.UseTotalPriceMatching = NoYes.No;
            });

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice net unit price for item ITEM-001 is 51 against the ordered 50, "
            + "and no net unit price tolerance is configured.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task Without_a_configured_tolerance_an_invoice_price_difference_is_still_refused_by_name()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, null);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice price for item ITEM-001 is 51 against the ordered 50 under the configured "
            + "matching policy, and no price match tolerance is configured.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task An_invoice_price_within_the_configured_tolerance_posts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, 5m);

        // 5100 invoiced against 5000 ordered is a 2% variance, inside the 5% tolerance.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default)).StatusCode);
        Assert.Equal(5100m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
        // The invoiced price is what reaches inventory, not the ordered price.
        Assert.Equal(5100m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    [Fact]
    public async Task An_invoice_price_outside_the_configured_tolerance_is_refused_with_the_variance()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, 5m);

        // 5400 against 5000 is 8%, outside 5%.
        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice price for item ITEM-001 differs from the purchase order by 8%, "
            + "which exceeds the configured match tolerance of 5%.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task The_tolerance_is_measured_both_ways_so_a_cheaper_invoice_is_matched_the_same()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, 5m);

        // Under-charging by 2% is a discrepancy of the same size and is accepted alike.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 49)]), default)).StatusCode);
        Assert.Equal(4900m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task The_vendor_matching_policy_is_honored_where_it_was_previously_stored_and_ignored()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        // Item None, vendor three-way: before this change only the item was read, so the price
        // difference posted unmatched.
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.None, MatchingPolicy.ThreeWay, 1m);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("differs from the purchase order by 8%", result.Message);
    }

    [Fact]
    public async Task With_no_matching_policy_on_either_side_the_invoice_price_is_taken_as_billed()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.None, MatchingPolicy.None, null);

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        Assert.Equal(5400m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task UseInvoiceMatching_No_switches_matching_off_entirely()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.ThreeWay, null, x => x.UseInvoiceMatching = NoYes.No);

        // The master switch wins over both policies.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        Assert.Equal(5400m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task UseTotalPriceMatching_No_keeps_matching_on_but_stops_the_price_comparison()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x => x.UseTotalPriceMatching = NoYes.No);

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        // The quantity side of the three-way policy still applies: more than received is refused.
        Assert.Equal(422, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-2", new DateTime(2026, 10, 29), [new(line.RecId, 1, 54)]), default)).StatusCode);
    }

    [Fact]
    public async Task The_company_matching_policy_applies_when_neither_the_vendor_nor_the_item_sets_one()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.None,
            MatchingPolicy.None, null, x => x.MatchingPolicy = MatchingPolicy.ThreeWay);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("under the configured matching policy", result.Message);
    }

    [Fact]
    public async Task Forbidding_override_makes_the_company_policy_the_only_one_that_counts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        // Company None, item three-way, override forbidden: the item policy is ignored.
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.MatchingPolicy = MatchingPolicy.None;
                x.MatchingPolicyAllowOverride = NoYes.No;
            });

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        Assert.Equal(5400m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task A_parameters_row_left_at_its_defaults_matches_exactly_as_no_row_does()
    {
        // Guards the deliberate deviation: D365 ships UseInvoiceMatching as No, but defaulting it to
        // Yes here keeps invoices that are matched today matched.
        await using var withRow = await Fixture.CreateAsync();
        var (rowInvoices, rowPo, rowLine) = await ReceivedOrderForMatchingAsync(withRow.Db,
            MatchingPolicy.ThreeWay, MatchingPolicy.None, null, _ => { });
        var withRowResult = await rowInvoices.PostAsync(rowPo.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(rowLine.RecId, 100, 54)]), default);

        await using var withoutRow = await Fixture.CreateAsync();
        var (bareInvoices, barePo, bareLine) = await ReceivedOrderForMatchingAsync(withoutRow.Db,
            MatchingPolicy.ThreeWay, MatchingPolicy.None, null);
        var withoutRowResult = await bareInvoices.PostAsync(barePo.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(bareLine.RecId, 100, 54)]), default);

        Assert.Equal(422, withRowResult.StatusCode);
        Assert.Equal(withoutRowResult.StatusCode, withRowResult.StatusCode);
        Assert.Equal(withoutRowResult.Message, withRowResult.Message);
    }

    // ---- Tax origin (D365 TaxBaseType) and the pricing date (D365 PriceDateType) ----

    /// <summary>
    /// Every flow applies a resolved rate as net * rate / 100. A code configured with any other
    /// origin, calculation method, limit or compounding must be refused rather than posted at the
    /// wrong base, which is what happened before these fields were read.
    /// </summary>
    [Theory]
    [InlineData(TaxBaseType.PctPerGross, "has origin PctPerGross")]
    [InlineData(TaxBaseType.PctPerTax, "has origin PctPerTax")]
    [InlineData(TaxBaseType.AmountByUnit, "has origin AmountByUnit")]
    [InlineData(TaxBaseType.PctGrossOnNet, "has origin PctGrossOnNet")]
    [InlineData(TaxBaseType.PctPerMargin, "has origin PctPerMargin")]
    public async Task An_uncalculated_tax_origin_is_refused_instead_of_posting_the_wrong_base(
        TaxBaseType origin, string expected)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForTaxAsync(db, x => x.TaxBase = origin);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains(expected, result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task An_uncalculated_tax_calculation_method_limit_or_compounding_is_refused_by_name()
    {
        foreach (var (configure, expected) in new (Action<TaxTable>, string)[]
        {
            (x => x.TaxCalcMethod = TaxCalcMethod.Total, "is calculated on the Total amount"),
            (x => x.TaxLimitBase = TaxLimitBase.Invoice, "has a Invoice tax limit"),
            (x => x.TaxLimitBase = TaxLimitBase.Line, "has a Line tax limit"),
            (x => x.TaxOnTax = "OTHER", "is compounded on tax code 'OTHER'")
        })
        {
            await using var fixture = await Fixture.CreateAsync();
            var db = fixture.Db;
            var (invoices, po, line) = await ReceivedOrderForTaxAsync(db, configure);

            var result = await invoices.PostAsync(po.RecId,
                new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default);
            Assert.Equal(422, result.StatusCode);
            Assert.Contains(expected, result.Message);
        }
    }

    [Fact]
    public async Task The_stored_default_tax_configuration_still_posts_the_percentage_of_net()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        // PctPerNet / Line / None / no compounding is the stored default, so nothing configured
        // today is affected by the refusals above.
        var (invoices, po, line) = await ReceivedOrderForTaxAsync(db, _ => { });

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default)).StatusCode);
        // 5000 net at 15% of net.
        Assert.Equal(750m, (await db.Set<TaxTrans>().SingleAsync()).TaxAmount);
    }

    private static async Task<(VendorInvoiceService Invoices, PurchTable Order, PurchLine Line)> ReceivedOrderForTaxAsync(
        CycleContext db, Action<TaxTable> configure)
    {
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedPurchaseTax(db);
        configure(db.Set<TaxTable>().Local.Single());
        await db.SaveChangesAsync();
        var (orders, confirm, receipts, invoices) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR",
            new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await confirm.ConfirmAsync(po.RecId, default)).StatusCode);
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 100)]), default)).StatusCode);
        return (invoices, po, await db.Set<PurchLine>().SingleAsync());
    }

    [Fact]
    public async Task The_pricing_date_follows_PriceDateType_so_a_delivery_dated_agreement_can_be_selected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        // Valid only around the 2026-10-28 delivery date, never "today".
        var delivery = PurchaseAgreement(1, 33m);
        delivery.FromDate = new DateTime(2026, 10, 20);
        delivery.ToDate = new DateTime(2026, 11, 1);
        db.Set<PriceDiscTable>().Add(delivery);
        db.Set<PurchParameters>().Add(new PurchParameters
        {
            RecId = 7, DataAreaId = "dat", PriceDateType = PriceDateType.DeliveryDate
        });
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, new DateTime(2026, 10, 28), null,
                UsePriceAgreement: true), default)).StatusCode);
        Assert.Equal(33m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task With_PriceDateType_Today_an_agreement_that_expired_before_today_is_not_selected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        var expiredBeforeToday = PurchaseAgreement(1, 33m);
        expiredBeforeToday.FromDate = new DateTime(2026, 10, 20);
        expiredBeforeToday.ToDate = new DateTime(2026, 11, 1);
        db.Set<PriceDiscTable>().Add(expiredBeforeToday);
        // Today is the default, and no parameters row means the default.
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, new DateTime(2026, 10, 28), null,
                UsePriceAgreement: true), default)).StatusCode);
        // Falls back to the item's purchase price rather than using the delivery-dated agreement.
        Assert.Equal(45m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    // ---- Purchase trade agreements (D365: Procurement > Prices and discounts > purchase price) ----

    private static async Task<(PurchOrderService Orders, PurchTable Order)> OpenPurchaseOrderAsync(CycleContext db)
    {
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null,
                new DateTime(2026, 10, 20)), default)).StatusCode);
        return (orders, await db.Set<PurchTable>().SingleAsync());
    }

    private static PriceDiscTable PurchaseAgreement(long recId, decimal amount,
        PriceDiscPartyCodeType accountCode = PriceDiscPartyCodeType.Table, string accountRelation = "V-0001",
        PriceDiscProductCodeType itemCode = PriceDiscProductCodeType.Table, string itemRelation = "ITEM-001",
        decimal quantityFrom = 0m, string inventDimId = "") => new()
        {
            RecId = recId, DataAreaId = "dat", Module = ModuleInventCustVend.Vend, Relation = PriceType.PricePurch,
            AccountCode = accountCode, AccountRelation = accountRelation, ItemCode = itemCode, ItemRelation = itemRelation,
            Currency = "SAR", UnitId = "PCS", PriceUnit = 1m, Amount = amount, QuantityAmountFrom = quantityFrom,
            InventDimId = inventDimId, FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 12, 31)
        };

    [Fact]
    public async Task A_purchase_line_takes_its_price_verbatim_unless_it_opts_into_an_agreement()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().Add(PurchaseAgreement(1, 40m));
        await db.SaveChangesAsync();

        // The flag is opt-in, so every existing caller keeps sending its own price.
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(50m, line.PurchPrice);
        Assert.Equal(5000m, line.LineAmount);
    }

    [Fact]
    public async Task A_purchase_line_resolves_the_vendor_item_agreement_and_prices_the_line_from_it()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().Add(PurchaseAgreement(1, 40m));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        // The sent price of 50 is discarded for the agreed 40.
        Assert.Equal(40m, line.PurchPrice);
        Assert.Equal(4000m, line.LineAmount);
    }

    [Fact]
    public async Task The_most_specific_purchase_agreement_wins_over_the_all_vendor_rule()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 48m, PriceDiscPartyCodeType.All, "", PriceDiscProductCodeType.All, ""),
            PurchaseAgreement(2, 44m, itemCode: PriceDiscProductCodeType.All, itemRelation: ""),
            PurchaseAgreement(3, 40m));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(40m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_purchase_agreement_quantity_break_applies_only_from_its_own_quantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 40m),
            PurchaseAgreement(2, 35m, quantityFrom: 100m));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 99, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        // SQLite cannot ORDER BY a decimal column, so LineNumber is ordered client-side.
        var lines = (await db.Set<PurchLine>().ToListAsync()).OrderBy(x => x.LineNumber).ToList();
        Assert.Equal(40m, lines[0].PurchPrice);
        Assert.Equal(35m, lines[1].PurchPrice);
    }

    [Fact]
    public async Task An_expired_or_foreign_currency_purchase_agreement_is_not_applied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        var expired = PurchaseAgreement(1, 40m);
        expired.ToDate = new DateTime(2026, 1, 31);
        var foreign = PurchaseAgreement(2, 30m);
        foreign.Currency = "USD";
        db.Set<PriceDiscTable>().AddRange(expired, foreign);
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        // No agreement applies, so the item's own purchase price is used, as on the sales side.
        Assert.Equal(45m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_dimension_specific_purchase_agreement_is_applied_after_the_dimension_resolves()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        // The supply service assigns DIM-000001; a dimension-specific rule cannot match before that.
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 40m),
            PurchaseAgreement(2, 36m, inventDimId: "DIM-000001"));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal("DIM-000001", line.InventDimId);
        Assert.Equal(36m, line.PurchPrice);
        Assert.Equal(3600m, line.LineAmount);
    }

    [Fact]
    public async Task A_purchase_agreement_for_another_vendor_or_the_sales_axis_is_not_applied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        var otherVendor = PurchaseAgreement(1, 20m, accountRelation: "V-0002");
        var salesAxis = PurchaseAgreement(2, 21m);
        salesAxis.Module = ModuleInventCustVend.Cust;
        salesAxis.Relation = PriceType.PriceSales;
        db.Set<PriceDiscTable>().AddRange(otherVendor, salesAxis);
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(45m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_vendor_price_group_agreement_applies_to_a_vendor_in_that_group()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<VendTable>().Local.Single().PriceGroup = "WHOLESALE";
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 48m, PriceDiscPartyCodeType.All, "", PriceDiscProductCodeType.All, ""),
            PurchaseAgreement(2, 42m, PriceDiscPartyCodeType.GroupId, "WHOLESALE"));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        // The group rule is more specific than the all-vendor rule, as on the sales side.
        Assert.Equal(42m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    private static async Task<(PriceDiscJournalPostingService Service, PriceDiscAdmTable Header)> PurchaseAgreementJournalAsync(
        CycleContext db, Action<PriceDiscAdmTrans> line)
    {
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        db.Set<PriceDiscAdmName>().Add(new PriceDiscAdmName { RecId = 1, DataAreaId = "dat", JournalName = "PURCH-PRICE" });
        var header = new PriceDiscAdmTable
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "PDJ-001", JournalName = "PURCH-PRICE",
            DefaultRelation = PriceType.PricePurch, Posted = NoYes.No
        };
        db.Set<PriceDiscAdmTable>().Add(header);
        var trans = new PriceDiscAdmTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "PDJ-001", LineNum = 1m,
            Module = ModuleInventCustVend.Vend, AccountCode = PriceDiscPartyCodeType.Table, AccountRelation = "V-0001",
            ItemCode = PriceDiscProductCodeType.Table, ItemRelation = "ITEM-001",
            Currency = "SAR", UnitId = "PCS", PriceUnit = 1m, Amount = 40m,
            FromDate = new DateTime(2026, 1, 1), ToDate = new DateTime(2026, 12, 31)
        };
        line(trans);
        db.Set<PriceDiscAdmTrans>().Add(trans);
        await db.SaveChangesAsync();
        return (new PriceDiscJournalPostingService(new TestUnitOfWork(db), new TestCompany()), header);
    }

    [Fact]
    public async Task A_purchase_agreement_journal_publishes_a_vendor_rule_the_purchase_line_then_resolves()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (journal, header) = await PurchaseAgreementJournalAsync(db, _ => { });

        var posted = await journal.PostAsync(header.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Posted, posted.Status);
        var rule = await db.Set<PriceDiscTable>().SingleAsync();
        Assert.Equal(ModuleInventCustVend.Vend, rule.Module);
        Assert.Equal(PriceType.PricePurch, rule.Relation);
        Assert.Equal(40m, rule.Amount);

        // The published rule is what the purchase line resolves, closing the loop.
        db.ChangeTracker.Clear();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR",
            new DateTime(2026, 10, 28), "S01", "WH-01", null, new DateTime(2026, 10, 20)), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(40m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_purchase_agreement_journal_refuses_the_customer_module_and_an_unknown_vendor()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (journal, header) = await PurchaseAgreementJournalAsync(db, line => line.Module = ModuleInventCustVend.Cust);
        var wrongModule = await journal.PostAsync(header.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Invalid, wrongModule.Status);
        Assert.Contains("Line 1: purchase agreements must use the vendor module.", wrongModule.Errors!);
        Assert.Empty(await db.Set<PriceDiscTable>().ToListAsync());

        await using var second = await Fixture.CreateAsync();
        var (other, otherHeader) = await PurchaseAgreementJournalAsync(second.Db, line => line.AccountRelation = "V-0404");
        var unknown = await other.PostAsync(otherHeader.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Invalid, unknown.Status);
        Assert.Contains("Line 1: vendor 'V-0404' was not found.", unknown.Errors!);
    }

    [Fact]
    public async Task A_sales_agreement_journal_still_refuses_the_vendor_module()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (journal, header) = await PurchaseAgreementJournalAsync(db, _ => { });
        // Flip the journal to the sales axis while its line stays on the vendor module.
        (await db.Set<PriceDiscAdmTable>().SingleAsync()).DefaultRelation = PriceType.LineDiscSales;
        var trans = await db.Set<PriceDiscAdmTrans>().SingleAsync();
        trans.Relation = PriceType.PriceSales;
        trans.Amount = 5m;
        await db.SaveChangesAsync();

        var result = await journal.PostAsync(header.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Invalid, result.Status);
        Assert.Contains("Line 1: sales agreements must use the customer module.", result.Errors!);
    }

    private static void SeedPurchaseMasterData(CycleContext db)
    {
        db.Set<VendTable>().Add(new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-0001", Currency = "SAR", VendGroup = "LOCAL" });
        db.Set<InventSite>().AddRange(
            new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S01", Name = "Jeddah Main" },
            new InventSite { RecId = 2, DataAreaId = "dat", SiteId = "S02", Name = "Riyadh Branch" });
        db.Set<InventLocation>().Add(new InventLocation { RecId = 1, DataAreaId = "dat", InventLocationId = "WH-01", InventSiteId = "S01" });
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", NameAlias = "Cotton Uniform" });
        db.Set<InventTableModule>().AddRange(
            new InventTableModule { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "PCS" },
            new InventTableModule { RecId = 2, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Purchase, UnitId = "PCS" });
        db.Set<UnitOfMeasure>().Add(new UnitOfMeasure { RecId = 1, Symbol = "PCS", DecimalPrecision = 0 });
        if (!db.Set<InventModelGroup>().Local.Any())
            db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM-001" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM-001", ItemGroupId = "FINISHED" });
        // Purchase inventory receipt -> inventory asset 130100; vendor summary -> AP 210100.
        db.Set<MainAccount>().Add(new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "210100" });
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, DisplayValue = "210100" });
        db.Set<InventPosting>().Add(new InventPosting { RecId = 3, DataAreaId = "dat", InventAccountType = InventAccountType.PurchReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 });
        // Offset to the purchase inventory receipt; a return records it so a later cost adjustment
        // can reuse the pair instead of the current profile.
        db.Set<MainAccount>().Add(new MainAccount { RecId = 90, DataAreaId = "dat", MainAccountId = "600180" });
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination { RecId = 90, DataAreaId = "dat", MainAccount = 90, DisplayValue = "600180" });
        db.Set<InventPosting>().Add(new InventPosting { RecId = 90, DataAreaId = "dat", InventAccountType = InventAccountType.PurchConsump, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 90 });
        db.Set<VendLedgerAccounts>().Add(new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = 3 });
    }

    private static (PurchOrderService, PurchConfirmationService, ProductReceiptService, VendorInvoiceService) PurchaseServices(CycleContext db, TestInventNumbers? sharedNumbers = null)
    {
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var numbers = sharedNumbers ?? new TestInventNumbers();
        var supply = new PurchInventorySupplyService(work, numbers, new InventDimensionResolver(work, numbers));
        return (new PurchOrderService(work, company, Sequence(), supply, new SalesUnitConversionService(work)),
            new PurchConfirmationService(work, company, Sequence()),
            new ProductReceiptService(work, company, Sequence()),
            new VendorInvoiceService(work, company, Sequence()));
    }

    internal sealed class TestInventNumbers : ISalesInventoryNumberService
    {
        private int _dim, _trans;
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default) => Task.FromResult($"DIM-{++_dim:000000}");
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default) => Task.FromResult($"LOT-{++_trans:000000}");
    }

    private static void SeedLedgerAndPosting(CycleContext db)
    {
        // DefaultExchangeRateType only matters to the foreign-currency tests: a document already
        // in the accounting currency never looks a rate up.
        db.Set<Ledger>().Add(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", FiscalCalendar = 1, DefaultExchangeRateType = 1 });
        db.Set<FiscalCalendarPeriod>().Add(new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) });
        db.Set<LedgerFiscalCalendarPeriod>().Add(new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "130100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "510100" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, DisplayValue = "130100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, DisplayValue = "510100" });
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 1, DataAreaId = "dat", InventAccountType = InventAccountType.InventReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 2, DataAreaId = "dat", InventAccountType = InventAccountType.InventIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 },
            // The purchase pair: a purchase-related cost adjustment never uses the Inventory tab
            // accounts, which apply only to transactions unrelated to sales, purchases or production.
            new InventPosting { RecId = 91, DataAreaId = "dat", InventAccountType = InventAccountType.PurchReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 92, DataAreaId = "dat", InventAccountType = InventAccountType.PurchConsump, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
    }

    private static ISysNumberSequenceService Sequence() =>
        DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>();

    // ---- The inventory reference cycle (CycleErpExamples/Inventory/Read_only.md) --------------

    /// <summary>
    /// Runs the eleven-step example from the inventory cycle reference end to end through the real
    /// purchase, reservation, delivery, invoice and close services, then asserts the figures the
    /// reference publishes: <b>90</b> on hand, inventory value <b>1,080</b>, cost of goods sold
    /// <b>720</b>, and a net cost adjustment of <b>120</b> split <b>-80</b> over the first issue
    /// and <b>-40</b> over the second. The crux of periodic weighted average is what this proves:
    /// the second receipt arrives <i>after</i> both sales and still lifts the period average to 12,
    /// so the already-invoiced issues are adjusted upward at close.
    /// Quantities, prices and dates follow the reference; item, site and warehouse ids follow this
    /// fixture, and the example carries no tax, charge or discount exactly as the reference states.
    /// </summary>
    [Fact]
    public async Task Inventory_reference_cycle_closes_at_ninety_on_hand_and_one_thousand_eighty_value()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        // The reference costs this example with periodic weighted average.
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        // The sales delivery pair, seeded inline as the other delivery tests do.
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
        // The reference's service line. A Service item with no model group assignment is
        // non-stocked, so it invoices without touching inventory.
        db.Set<InventTable>().Add(new InventTable { RecId = 2, DataAreaId = "dat", ItemId = "SRV-01", NameAlias = "Installation", ItemType = ItemType.Service });
        db.Set<InventTableModule>().Add(new InventTableModule { RecId = 3, DataAreaId = "dat", ItemId = "SRV-01", ModuleType = ModuleInventPurchSales.Sales, UnitId = "PCS" });
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "NET30", PaymMethod = PaymMethod.Net, NumOfDays = 30 });
        await db.SaveChangesAsync();

        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var numbers = new TestInventNumbers();
        var (orders, confirmations, receipts, vendorInvoices) = PurchaseServices(db, numbers);

        // Steps 1-3: buy 100 at 10, receive all of it, then invoice all of it.
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 1), "S01", "WH-01", null), default)).StatusCode);
        var po1 = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po1.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 10, 1, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await confirmations.ConfirmAsync(po1.RecId, default)).StatusCode);
        var po1Line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po1.RecId,
            new ProductReceiptPostDto("PR-001", new DateTime(2026, 11, 2), [new(po1Line.RecId, 100)]), default)).StatusCode);
        Assert.Equal(100m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po1.RecId,
            new VendorInvoicePostDto("VI-001", new DateTime(2026, 11, 3), [new(po1Line.RecId, 100)]), default)).StatusCode);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.PhysicalInvent);
        Assert.Equal(100m, sum.PostedQty);
        Assert.Equal(1000m, sum.PostedValue);

        // Step 4: one sales order carrying the stocked item and the service together.
        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-001", CustAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S01", InventLocationId = "WH-01",
            OrderDate = new DateTime(2026, 11, 4), PaymTerm = "NET30"
        };
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var itemLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 60, QtyOrdered = 60,
            RemainSalesPhysical = 60, RemainSalesFinancial = 60, SalesUnit = "PCS", SalesPrice = 25, PriceUnit = 1,
            LineAmount = 1500, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, CurrencyCode = "SAR",
            ReceiptDateRequested = new DateTime(2026, 11, 5)
        };
        var demand = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        await demand.CreateAsync(order, itemLine, "S01", "WH-01");
        db.Set<SalesLine>().Add(itemLine);
        var serviceLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 2, ItemId = "SRV-01", SalesQty = 1, QtyOrdered = 1,
            RemainSalesPhysical = 1, RemainSalesFinancial = 1, SalesUnit = "PCS", SalesPrice = 300, PriceUnit = 1,
            LineAmount = 300, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.No, CurrencyCode = "SAR",
            ReceiptDateRequested = new DateTime(2026, 11, 5)
        };
        db.Set<SalesLine>().Add(serviceLine);
        await db.SaveChangesAsync();

        // Step 5: reserve 60 physically. On hand stays 100 and available drops to 40.
        var reservation = new InventoryReservationService(work, company);
        Assert.Null((await reservation.ReserveAsync(itemLine.InventTransId, 60m, false, default)).Error);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.PhysicalInvent);
        Assert.Equal(60m, sum.ReservPhysical);
        Assert.Equal(40m, sum.AvailPhysical);

        var packingSlips = new PackingSlipJournalService(work, company, Sequence());
        var salesInvoices = new SalesInvoiceService(work, company, Sequence());

        // Step 6: deliver 40. On hand 60, still 20 reserved, 40 available.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 11, 6), "All", true, "PS-001",
            [new PackingSlipPostingLineRequest(itemLine.RecId.ToString(), 40)]), default)).StatusCode);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(20m, sum.ReservPhysical);
        Assert.Equal(40m, sum.AvailPhysical);

        // Step 7: invoice the 40 delivered. An initial cost of 400 at 10 each.
        Assert.Equal(200, (await salesInvoices.PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 11, 7), "SI-001", [new(itemLine.RecId, 40)]), default)).StatusCode);
        Assert.Equal(-400m, (await db.Set<InventTrans>().Where(x => x.StatusIssue == StatusIssue.Sold).ToListAsync()).Sum(x => x.CostAmountPosted));

        // Step 8: deliver the remaining 20. On hand 40 and nothing left reserved.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 11, 8), "All", true, "PS-002",
            [new PackingSlipPostingLineRequest(itemLine.RecId.ToString(), 20)]), default)).StatusCode);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(40m, sum.PhysicalInvent);
        Assert.Equal(0m, sum.ReservPhysical);

        // Step 9: invoice the last 20 together with the service. Item cost reaches 600.
        var finalInvoice = await salesInvoices.PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 11, 9), "SI-002", [new(itemLine.RecId, 20), new(serviceLine.RecId, 1)]), default);
        Assert.True(finalInvoice.StatusCode == 200, finalInvoice.Message);
        Assert.Equal(-600m, (await db.Set<InventTrans>().Where(x => x.StatusIssue == StatusIssue.Sold).ToListAsync()).Sum(x => x.CostAmountPosted));
        // The service billed 300 and never produced an inventory transaction.
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), x => x.ItemId == "SRV-01");

        // Step 10: buy 50 more at 16 after both sales. On hand 90, inventory value 1,200.
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 10), "S01", "WH-01", null), default)).StatusCode);
        var po2 = await db.Set<PurchTable>().SingleAsync(x => x.RecId != po1.RecId);
        Assert.Equal(200, (await orders.AddLineAsync(po2.RecId, new PurchLineInputDto("ITEM-001", 50, "PCS", 16, 1, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await confirmations.ConfirmAsync(po2.RecId, default)).StatusCode);
        var po2Line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == po2.PurchId);
        Assert.Equal(200, (await receipts.PostAsync(po2.RecId,
            new ProductReceiptPostDto("PR-002", new DateTime(2026, 11, 11), [new(po2Line.RecId, 50)]), default)).StatusCode);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po2.RecId,
            new VendorInvoicePostDto("VI-002", new DateTime(2026, 11, 12), [new(po2Line.RecId, 50)]), default)).StatusCode);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);

        // Step 11: close. Period average is (100*10 + 50*16) / 150 = 12.
        var close = await new InventCloseService(work, company, new TestUser(), Sequence())
            .CloseItemAsync("ITEM-001", new DateTime(2026, 11, 30));
        Assert.Null(close.Error);

        var issues = await db.Set<InventTrans>().Where(x => x.StatusIssue == StatusIssue.Sold).ToListAsync();
        // Cost of goods sold becomes 60 * 12 = 720, so the net adjustment is 120.
        Assert.Equal(-720m, issues.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment));
        Assert.Equal(-120m, issues.Sum(x => x.CostAmountAdjustment));
        // Split across the two issues: 40 * 2 and 20 * 2.
        Assert.Equal(-80m, issues.Where(x => x.Qty == -40m).Sum(x => x.CostAmountAdjustment));
        Assert.Equal(-40m, issues.Where(x => x.Qty == -20m).Sum(x => x.CostAmountAdjustment));
        // Receipts stay at 1,800 and the remaining inventory is 90 * 12 = 1,080.
        var purchased = await db.Set<InventTrans>().Where(x => x.Qty > 0).ToListAsync();
        Assert.Equal(1800m, purchased.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment));
        Assert.Equal(1080m, purchased.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment)
            + issues.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment));
    }

    // ---- Inventory marking --------------------------------------------------------------------

    [Fact]
    public async Task Marking_links_an_issue_to_a_receipt_on_both_sides_and_rejects_remarking()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());

        var (marked, error) = await service.MarkAsync("I1", "R2", default);
        Assert.Null(error);
        Assert.True(marked!.IsMarked);

        // D365 holds the link on both transactions, each pointing at the other's origin.
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2);
        Assert.Equal(2L, issue.MarkingRefInventTransOrigin);   // origin RecId of R2
        Assert.Equal(3L, receipt.MarkingRefInventTransOrigin); // origin RecId of I1

        // The same pair again is a no-operation, a different pair is refused.
        Assert.Null((await service.MarkAsync("I1", "R2", default)).Error);
        Assert.Contains("already marked", (await service.MarkAsync("I1", "R1", default)).Error);
        // Direction and item are enforced.
        Assert.Contains("not an issue", (await service.MarkAsync("R1", "R2", default)).Error);
        Assert.Contains("cannot be marked to itself", (await service.MarkAsync("I1", "I1", default)).Error);

        var listed = Assert.Single(await service.ListAsync(null, default) ?? []);
        Assert.Equal(("I1", "R2"), (listed.IssueInventTransId, listed.ReceiptInventTransId));

        // Removing the marking clears both sides.
        Assert.Null((await service.UnmarkAsync("I1", default)).Error);
        Assert.Equal(0L, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).MarkingRefInventTransOrigin);
        Assert.Equal(0L, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2)).MarkingRefInventTransOrigin);
        Assert.Contains("not marked", (await service.UnmarkAsync("I1", default)).Error);
    }

    [Fact]
    public async Task Split_inventory_movement_can_be_unmarked_by_its_record_id()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        var first = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        first.Qty = -25m;
        first.CostAmountPosted = -275m;
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3,
            Qty = -25m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -275m,
            DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());
        Assert.Null((await service.MarkAsync("I1", "R2", 3, null, default)).Error);
        Assert.Contains("split across", (await service.UnmarkAsync("I1", default)).Error);
        Assert.Contains("does not belong", (await service.UnmarkAsync("I1", 999, default)).Error);
        Assert.Null((await service.UnmarkAsync("I1", 3, default)).Error);
        var movements = await db.Set<InventTrans>().Where(x => x.RecId == 2 || x.RecId == 3 || x.RecId == 4).ToListAsync();
        Assert.All(movements, x => Assert.Equal(0L, x.MarkingRefInventTransOrigin));
    }

    [Fact]
    public async Task Split_marking_rejects_indistinguishable_pairs_and_refuses_ambiguous_removal()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3,
            Qty = -10m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -110m,
            DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 2,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 120m,
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());
        Assert.Null((await service.MarkAsync("I1", "R2", 3, 2, default)).Error);
        Assert.Contains("cannot be identified safely", (await service.MarkAsync("I1", "R2", 4, 5, default)).Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5);
        Assert.Equal(0L, issue.MarkingRefInventTransOrigin);
        Assert.Equal(0L, receipt.MarkingRefInventTransOrigin);
        // Existing ambiguous records must not cause removal of several counterpart markings.
        issue.MarkingRefInventTransOrigin = 2;
        receipt.MarkingRefInventTransOrigin = 3;
        await db.SaveChangesAsync();
        Assert.Contains("cannot be uniquely identified", (await service.UnmarkAsync("I1", 3, default)).Error);
        var marked = await db.Set<InventTrans>().Where(x => x.RecId == 2 || x.RecId == 3 || x.RecId == 4 || x.RecId == 5).ToListAsync();
        Assert.All(marked, x => Assert.NotEqual(0L, x.MarkingRefInventTransOrigin));
    }

    [Fact]
    public async Task Marking_removal_refuses_a_settled_or_missing_counterpart()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());
        Assert.Null((await service.MarkAsync("I1", "R2", default)).Error);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2);
        receipt.ValueOpen = InventTransOpen.No;
        await db.SaveChangesAsync();
        Assert.Contains("financially settled", (await service.UnmarkAsync("I1", default)).Error);
        receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2);
        receipt.ValueOpen = InventTransOpen.Yes;
        receipt.MarkingRefInventTransOrigin = 0;
        await db.SaveChangesAsync();
        Assert.Contains("cannot be uniquely identified", (await service.UnmarkAsync("I1", default)).Error);
        Assert.Equal(2L, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).MarkingRefInventTransOrigin);
    }

    [Fact]
    public async Task Marked_issue_settles_against_its_marked_receipt_instead_of_the_model_order()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        // Marked to the dearer second receipt, so FIFO order is deliberately overridden.
        Assert.Null((await new InventoryMarkingService(work, new TestCompany()).MarkAsync("I1", "R2", default)).Error);

        var (result, error) = await new InventCloseService(work, new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30), default);

        Assert.Null(error);
        Assert.Equal(1, result!.IssuesSettled);
        var settlement = Assert.Single(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(2L, settlement.ReceiptTransRecId);          // the marked receipt, not the oldest
        Assert.Equal(-600m, settlement.CostAmountSettled);       // 50 units at the marked receipt's 12
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(-600m, issue.CostAmountSettled);
        Assert.Equal(-50m, issue.CostAmountAdjustment);          // provisional was 11 per unit
        // The oldest receipt is untouched, which is the whole point of marking.
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
    }

    [Theory]
    [InlineData(InventModel.FIFO, 100, -50, -50)]
    [InlineData(InventModel.LIFO, 150, -50, -100)]
    public async Task Close_reserves_only_marked_quantity_for_the_later_marked_issue(
        InventModel model, int earlyQuantity, int firstQuantity, int secondQuantity)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = model;
        await db.SaveChangesAsync();
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "EARLY", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -earlyQuantity, StatusIssue = StatusIssue.Sold, CostAmountPosted = -10m * earlyQuantity, Voucher = "EARLY-VOUCHER",
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "EARLY-VOUCHER",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        Assert.Null((await new InventoryMarkingService(work, new TestCompany()).MarkAsync("I1", "R1", default)).Error);
        var result = await new InventCloseService(work, new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        var early = settlements.Where(x => x.TransRecId == 4).ToList();
        Assert.Equal(2, early.Count);
        Assert.Equal(firstQuantity, Assert.Single(early, x => x.ReceiptTransRecId == 1).QtySettled);
        Assert.Equal(secondQuantity, Assert.Single(early, x => x.ReceiptTransRecId == 2).QtySettled);
        var marked = Assert.Single(settlements, x => x.TransRecId == 3);
        Assert.Equal(1L, marked.ReceiptTransRecId);
        Assert.Equal(-50m, marked.QtySettled);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task Weighted_average_close_excludes_marked_quantity_from_unmarked_issue_cost(int issueDay)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 3).MarkingRefInventTransOrigin = 2;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "OTHER-ISSUE", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -150m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1650m, Voucher = "OTHER-ISSUE",
            DateFinancial = new DateTime(2026, 6, issueDay), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "OTHER-ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue,
            PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        var marked = Assert.Single(settlements, x => x.TransRecId == 3);
        Assert.Equal(2L, marked.ReceiptTransRecId);
        Assert.Equal(-600m, marked.CostAmountSettled);
        Assert.Equal(-1600m, settlements.Where(x => x.TransRecId == 4).Sum(x => x.CostAmountSettled));
    }

    [Fact]
    public async Task Weighted_average_mixed_pool_allows_remaining_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 3).MarkingRefInventTransOrigin = 2;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "OTHER-ISSUE", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -1m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -11m, Voucher = "OTHER-ISSUE",
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "OTHER-ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(-600m, settlements.Where(x => x.TransRecId == 3).Sum(x => x.CostAmountSettled));
        Assert.Equal(-10.67m, settlements.Where(x => x.TransRecId == 4).Sum(x => x.CostAmountSettled));
    }

    [Fact]
    public async Task Weighted_average_carries_remaining_value_to_next_period_and_reverses()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var first = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(first.Error);

        db.AddRange(
            new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "R3", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 5, DataAreaId = "dat", InventTransId = "I2", ItemId = "ITEM", ReferenceId = "IJ5", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
                Qty = 50m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000m, DateFinancial = new DateTime(2026, 7, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 5,
                Qty = -100m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1100m, Voucher = "I2-VOUCHER", DateFinancial = new DateTime(2026, 7, 2), ValueOpen = InventTransOpen.Yes },
            new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 5, ItemId = "ITEM", Voucher = "I2-VOUCHER",
                InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
                OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var second = await service.CloseItemAsync("ITEM", new DateTime(2026, 7, 31));
        Assert.Null(second.Error);
        Assert.Equal(-1325m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).CostAmountSettled);
        Assert.Null((await service.CancelAsync(second.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).CostAmountSettled);
        var repeated = await service.CloseItemAsync("ITEM", new DateTime(2026, 7, 31));
        Assert.Null(repeated.Error);
        Assert.Equal(-1325m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).CostAmountSettled);
    }

    [Fact]
    public async Task Weighted_average_refuses_marking_an_original_layer_after_average_carry_forward()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        Assert.Null((await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30))).Error);
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "I2", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -1m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -11m, Voucher = "I2", MarkingRefInventTransOrigin = 2,
            DateFinancial = new DateTime(2026, 7, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var second = await service.CloseItemAsync("ITEM", new DateTime(2026, 7, 31));
        Assert.Contains("linked receipt value is unavailable", second.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).QtySettled);
    }

    [Fact]
    public async Task Fifo_close_refuses_a_partially_available_marked_receipt_without_substituting_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.MarkingRefInventTransOrigin = 2;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).QtySettled = 75m;
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("partially marked issue", result.Error);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
    }

    [Fact]
    public async Task Marking_rejects_receipts_from_another_financial_site()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        db.Add(new InventDim { RecId = 2, DataAreaId = "dat", InventDimId = "DIM-OTHER", InventSiteId = "S2", InventLocationId = "WH2" });
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).InventDimId = "DIM-OTHER";
        await db.SaveChangesAsync();
        var result = await new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany()).MarkAsync("I1", "R2", default);
        Assert.Contains("same financial site", result.Error);
        Assert.All(await db.Set<InventTrans>().ToListAsync(), x => Assert.Equal(0, x.MarkingRefInventTransOrigin));
    }

    [Fact]
    public async Task Close_does_not_substitute_an_unmarked_receipt_when_the_marked_receipt_is_unavailable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.MarkingRefInventTransOrigin = 2;
        var markedReceipt = db.Set<InventTrans>().Local.Single(x => x.RecId == 2);
        markedReceipt.QtySettled = markedReceipt.Qty;
        markedReceipt.ValueOpen = InventTransOpen.No;
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("Marked receipt", result.Error);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
    }

    [Fact]
    public async Task Weighted_average_issue_with_insufficient_marked_receipt_is_not_approximated()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -150m;
        issue.CostAmountPosted = -1650m;
        issue.MarkingRefInventTransOrigin = 2;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("partially marked weighted-average issue", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Fact]
    public async Task Weighted_average_purchase_return_settles_at_its_linked_receipt_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.PurchaseOrder;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 3).CostAmountPosted = -600m;
        db.Set<InventSum>().Local.Single().PostedValue = 1600m;
        db.Add(new PurchLine { DataAreaId = "dat", ItemId = "ITEM", PurchId = "RETURN", InventTransId = "I1", InventTransIdReturn = "R2" });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        Assert.Equal(0m, result.Result!.TotalAdjustment);
        var settlement = Assert.Single(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(2L, settlement.ReceiptTransRecId);
        Assert.Equal(-600m, settlement.CostAmountSettled);
        Assert.Equal(1600m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Fact]
    public async Task Weighted_average_marked_issue_uses_marked_receipt_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        Assert.Null((await new InventoryMarkingService(work, new TestCompany()).MarkAsync("I1", "R2", default)).Error);
        var result = await new InventCloseService(work, new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlement = Assert.Single(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(2L, settlement.ReceiptTransRecId);
        Assert.Equal(-600m, settlement.CostAmountSettled);
        Assert.Equal(-50m, result.Result!.TotalAdjustment);
    }

    [Fact]
    public async Task Multi_receipt_adjustment_allocates_rounding_remainder_and_reverses_exactly()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var first = db.Set<InventTrans>().Local.Single(x => x.RecId == 1);
        var second = db.Set<InventTrans>().Local.Single(x => x.RecId == 2);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        first.Qty = second.Qty = 1m;
        first.CostAmountPosted = second.CostAmountPosted = 1m;
        issue.Qty = -3m;
        issue.CostAmountPosted = -3.01m;
        var summary = db.Set<InventSum>().Local.Single();
        summary.PhysicalInvent = 0m;
        summary.PostedValue = -0.01m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "R3", ItemId = "ITEM",
            ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 1m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1m,
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(close.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(new[] { 0.0033m, 0.0033m, 0.0034m }, settlements.Select(x => x.CostAmountAdjustment).OrderBy(x => x));
        Assert.Equal(0.01m, settlements.Sum(x => x.CostAmountAdjustment));
        Assert.Equal(0m, summary.PostedValue);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Equal(-0.01m, summary.PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Cost_adjustment_requires_inventory_summary_and_rolls_back_when_missing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Remove(db.Set<InventSum>().Local.Single());
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("Inventory summary is missing", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
        Assert.Empty(await db.Set<InventClosing>().ToListAsync());
    }

    [Fact]
    public async Task Reversal_requires_inventory_summary_and_keeps_original_run_when_missing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        db.Remove(await db.Set<InventSum>().SingleAsync());
        await db.SaveChangesAsync();
        var reversal = await service.CancelAsync(recalculation.Result!.ClosingRecId);
        Assert.Contains("Inventory summary is missing", reversal.Error);
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync()).Cancellation);
        Assert.All(await db.Set<InventSettlement>().ToListAsync(), x => Assert.Equal(NoYes.No, x.Cancelled));
        Assert.Equal(2, await db.Set<GeneralJournalAccountEntry>().CountAsync());
    }

    [Fact]
    public async Task Purchase_return_cost_adjustment_uses_the_original_posting_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.PurchaseOrder;
        db.Add(new PurchLine { DataAreaId = "dat", ItemId = "ITEM", PurchId = "RETURN", InventTransId = "I1", InventTransIdReturn = "R2" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);

        // A vendor return settles against its original receipt, and the close adjusts the accounts
        // the original transaction used. This path was refused outright before.
        Assert.NotEmpty(await db.Set<InventClosing>().ToListAsync());
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.NotEmpty(entries);
        // The pair recorded when the return was posted, not whatever profile is current now.
        Assert.Contains(entries, x => x.PostingType == LedgerPostingType.InventIssue && x.LedgerDimension == 1);
        Assert.Contains(entries, x => x.PostingType == LedgerPostingType.InventLoss && x.LedgerDimension == 2);
        // The pair balances, so the correction leaves the voucher balanced.
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        // The issue carried -550 provisionally and settles against the 10.00 FIFO layer at -500.
        Assert.Equal(-50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
    }

    [Fact]
    public async Task Inventory_close_refuses_ambiguous_sales_return_invoice_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var firstIssue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        firstIssue.InvoiceId = "FIRST";
        firstIssue.ValueOpen = InventTransOpen.No;
        firstIssue.QtySettled = firstIssue.Qty;
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", InventDimId = "DIM",
            Qty = -10m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -150m, InvoiceId = "SECOND",
            Voucher = "SECOND", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.No, QtySettled = -10m });
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM",
            ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("different inventory costs", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Sales_return_close_requires_only_the_selected_invoice_to_be_settled(bool selectedSettled, bool recalculate)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var selected = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        selected.InvoiceId = "FIRST";
        selected.DateFinancial = new DateTime(2026, 6, 3);
        selected.ValueOpen = selectedSettled ? InventTransOpen.No : InventTransOpen.Yes;
        selected.QtySettled = selectedSettled ? selected.Qty : 0m;
        var otherOpen = selectedSettled || !recalculate;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 1).QtySettled = selectedSettled ? 50m : otherOpen ? 0m : 10m;
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", InventDimId = "DIM",
            Qty = -10m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -150m, InvoiceId = "SECOND", Voucher = "SECOND",
            DateFinancial = new DateTime(2026, 6, 5), ValueOpen = otherOpen ? InventTransOpen.Yes : InventTransOpen.No,
            QtySettled = otherOpen ? 0m : -10m });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", Voucher = "SECOND",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        db.Add(new CustInvoiceTrans { RecId = 10, DataAreaId = "dat", InventTransId = "I1", InvoiceId = "FIRST" });
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1", OriginalInvoiceTransRecId = 10 });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.SalesIssue, PostingTypeOffset = LedgerPostingType.SalesConsump, ProjId = "" });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var close = recalculate
            ? await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30))
            : await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        if (recalculate)
        {
            Assert.Contains("original financially settled sales issue", close.Error);
            Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
            return;
        }
        Assert.Null(close.Error);
        Assert.Equal(selectedSettled ? 10m : 0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Equal(selectedSettled ? InventTransOpen.No : InventTransOpen.Yes, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).ValueOpen);
        Assert.Equal(InventTransOpen.Yes, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).ValueOpen);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Fifo_close_settles_same_period_sale_before_costing_its_return_and_reverses_both(bool laterReceipt)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        db.Set<InventSum>().Local.Single().PostedValue = 1760m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 110m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.SalesIssue, PostingTypeOffset = LedgerPostingType.SalesConsump, ProjId = "" });
        if (laterReceipt)
        {
            db.Add(new InventTransOrigin { RecId = 5, DataAreaId = "dat", InventTransId = "LATER", ItemId = "ITEM", ReferenceId = "IJ5", ReferenceCategory = InventRefType.InventoryJournal });
            db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 5,
                Qty = 50m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000m,
                DateFinancial = new DateTime(2026, 6, 5), ValueOpen = InventTransOpen.Yes });
        }
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));

        Assert.Null(close.Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4);
        Assert.Equal(-50m, issue.QtySettled);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(-10m, receipt.CostAmountAdjustment);
        Assert.Equal(1800m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Equal(0m, receipt.CostAmountAdjustment);
        Assert.Equal(1760m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Same_period_sales_return_rolls_back_when_original_issue_cannot_be_fully_settled()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -250m;
        issue.CostAmountPosted = -2750m;
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 110m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransOrigin { RecId = 5, DataAreaId = "dat", InventTransId = "LATER", ItemId = "ITEM", ReferenceId = "IJ5", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 5,
            Qty = 50m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000m,
            DateFinancial = new DateTime(2026, 6, 5), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();

        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));

        Assert.Contains("original financially settled sales issue", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<InventClosing>().ToListAsync());
    }

    [Fact]
    public async Task Sales_return_cost_adjustment_uses_the_credit_note_posting_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var originalIssue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        originalIssue.QtySettled = -50m;
        originalIssue.ValueOpen = InventTransOpen.No;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 1).QtySettled = 50m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM",
            ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.SalesIssue,
            PostingTypeOffset = LedgerPostingType.SalesConsump, ProjId = "" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(close.Error);
        Assert.Equal(10m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.All(entries, x => Assert.Contains(x.LedgerDimension, new long[] { 1, 2 }));
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Sales_cost_adjustments_use_accounts_from_the_original_invoice_voucher()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        var posting = db.Set<InventTransPosting>().Local.Single(x => x.InventTransOrigin == 3);
        posting.PostingType = LedgerPostingType.SalesIssue;
        posting.PostingTypeOffset = LedgerPostingType.SalesConsump;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        Assert.Null((await service.CancelAsync(recalculation.Result!.ClosingRecId)).Error);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.All(entries, x => Assert.Contains(x.LedgerDimension, new long[] { 1, 2 }));
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Journal_recalculation_refuses_missing_original_financial_posting()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Remove(db.Set<InventTransPosting>().Local.Single());
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("original financial inventory posting", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(1650m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Fact]
    public async Task Journal_recalculation_and_reversal_use_original_posting_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Voucher = "ISSUE-VOUCHER";
        db.Add(new InventJournalTrans { DataAreaId = "dat", JournalId = "IJ3", InventTransId = "I1", JournalType = InventJournalType.Adjustment });
        db.Set<InventTransPosting>().Local.Single(x => x.InventTransOrigin == 3).Voucher = "ISSUE-VOUCHER";
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "NEWINV" },
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "NEWLOSS" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, DisplayValue = "NEWINV" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, DisplayValue = "NEWLOSS" });
        db.Set<InventPosting>().Local.Single(x => x.InventAccountType == InventAccountType.InventIssue).LedgerDimension = 3;
        db.Add(new InventPosting { RecId = 3, DataAreaId = "dat", InventAccountType = InventAccountType.InventLoss,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 4 });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        Assert.Null((await service.CancelAsync(recalculation.Result!.ClosingRecId)).Error);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.All(entries, x => Assert.Contains(x.LedgerDimension, new long[] { 1, 2 }));
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Earlier_close_automatically_reverses_later_recalculation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 3));
        Assert.Null(close.Error);
        Assert.Equal(NoYes.Yes, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == recalculation.Result!.ClosingRecId)).Cancellation);
        Assert.Single(await db.Set<InventClosing>().Where(x => x.CancelClosing == NoYes.Yes).ToListAsync());
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.NotNull(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", new DateTime(2026, 6, 3), default));
        Assert.Equal(3, close.Result!.RunNum);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Item_close_reverses_its_recalculation_without_reversing_a_later_independent_item()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        db.Add(new InventModelGroupItem { ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "OTHER" });
        db.Add(new InventItemGroupItem { ItemDataAreaId = "dat", ItemId = "OTHER", ItemGroupId = "TRADE" });
        db.AddRange(
            new InventTransOrigin { RecId = 10, DataAreaId = "dat", InventTransId = "OR", ItemId = "OTHER", ReferenceId = "O1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 11, DataAreaId = "dat", InventTransId = "OI", ItemId = "OTHER", ReferenceId = "O2", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTrans { RecId = 10, DataAreaId = "dat", ItemId = "OTHER", InventDimId = "DIM", InventTransOrigin = 10, Qty = 10, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 11, DataAreaId = "dat", ItemId = "OTHER", InventDimId = "DIM", InventTransOrigin = 11, Qty = -5, StatusIssue = StatusIssue.Sold, CostAmountPosted = -60, Voucher = "OTHER-ISSUE", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes },
            new InventSum { DataAreaId = "dat", ItemId = "OTHER", InventDimId = "DIM", PhysicalInvent = 5, PostedValue = 40 },
            new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 11, ItemId = "OTHER", Voucher = "OTHER-ISSUE", InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var first = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        var second = await service.RecalculateItemAsync("OTHER", new DateTime(2026, 6, 30));
        Assert.Null(first.Error);
        Assert.Null(second.Error);
        Assert.Equal(10m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 11)).CostAmountAdjustment);

        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 3));

        Assert.Null(close.Error);
        Assert.Equal(NoYes.Yes, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == first.Result!.ClosingRecId)).Cancellation);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == second.Result!.ClosingRecId)).Cancellation);
        Assert.Equal(10m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 11)).CostAmountAdjustment);
        Assert.Equal(50m, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "OTHER")).PostedValue);
    }

    [Fact]
    public async Task Item_close_does_not_reverse_a_company_wide_recalculation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateAsync(new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 3));
        Assert.Contains("cannot automatically reverse a company-wide recalculation", close.Error);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == recalculation.Result!.ClosingRecId)).Cancellation);
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
    }

    [Fact]
    public async Task Failed_close_rolls_back_automatic_recalculation_reversal()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 1, 31));
        Assert.NotNull(close.Error);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == recalculation.Result!.ClosingRecId)).Cancellation);
        Assert.Empty(await db.Set<InventClosing>().Where(x => x.CancelClosing == NoYes.Yes).ToListAsync());
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(1700m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    [Fact]
    public async Task Legacy_recalculation_settlement_must_be_reversed_before_close()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var cutoff = new DateTime(2026, 6, 30);
        var recalculation = await service.RecalculateItemAsync("ITEM", cutoff);
        Assert.Null(recalculation.Error);

        // Reproduce the records written by the earlier recalculation implementation.
        var settlement = await db.Set<InventSettlement>().SingleAsync();
        settlement.ReceiptTransRecId = 1;
        settlement.QtySettled = -50m;
        settlement.CostAmountSettled = -500m;
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1);
        issue.QtySettled = -50m;
        issue.CostAmountSettled = -500m;
        receipt.QtySettled = 50m;
        await db.SaveChangesAsync();

        var refused = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Contains("earlier inventory recalculation contains settled", refused.Error);
        Assert.Equal(-50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Null((await service.CancelAsync(recalculation.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
        Assert.Null((await service.CloseItemAsync("ITEM", cutoff)).Error);
    }

    [Fact]
    public async Task Cost_run_numbers_remain_unique_after_a_reversal_and_reclose()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var cutoff = new DateTime(2026, 6, 30);

        var first = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Null(first.Error);
        var reversal = await service.CancelAsync(first.Result!.ClosingRecId);
        Assert.Null(reversal.Error);
        Assert.Null(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", cutoff, default));
        var second = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Null(second.Error);

        Assert.Equal(1, first.Result.RunNum);
        Assert.Equal(2, reversal.Result!.RunNum);
        Assert.Equal(3, second.Result!.RunNum);
        Assert.Equal(new[] { 1, 2, 3 }, (await db.Set<InventClosing>().ToListAsync()).Select(x => x.RunNum).OrderBy(x => x));
        Assert.NotNull(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", cutoff, default));
    }

    [Fact]
    public async Task Cancelling_recalculation_reverses_its_value_without_settling_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        Assert.Null((await service.CancelAsync(result.Result!.ClosingRecId)).Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
        Assert.Equal(1650m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    [Fact]
    public async Task Recalculation_can_repeat_and_be_reversed_before_final_close()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var cutoff = new DateTime(2026, 6, 30);

        var first = await service.RecalculateItemAsync("ITEM", cutoff);
        Assert.Null(first.Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, receipt.QtySettled);
        Assert.Equal(InventTransOpen.Yes, issue.ValueOpen);
        Assert.Equal(1700m, (await db.Set<InventSum>().SingleAsync()).PostedValue);

        var second = await service.RecalculateItemAsync("ITEM", cutoff);
        Assert.Null(second.Error);
        Assert.Equal(0m, second.Result!.TotalAdjustment);
        Assert.Null((await service.CancelAsync(second.Result.ClosingRecId)).Error);
        Assert.Equal(50m, issue.CostAmountAdjustment);

        var close = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Null(close.Error);
        Assert.Equal(0m, close.Result!.TotalAdjustment);
        Assert.Equal(-50m, issue.QtySettled);
        Assert.Equal(50m, receipt.QtySettled);
        Assert.Equal(InventTransOpen.No, issue.ValueOpen);
        Assert.Equal(1700m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Recalculation_leaves_a_partially_coverable_issue_unchanged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -150m;
        issue.CostAmountPosted = -1650m;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).DateFinancial = new DateTime(2026, 6, 20);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var run = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 10));
        Assert.Null(run.Error);
        Assert.Equal(150m, Assert.Single(run.Result!.UnsettledIssues).RemainingQuantity);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
    }

    [Fact]
    public async Task Close_preserves_unsettled_quantity_for_a_later_receipt_and_reverses_both_runs()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -150m;
        issue.CostAmountPosted = -1650m;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).DateFinancial = new DateTime(2026, 6, 20);
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var first = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 10));
        Assert.Null(first.Error);
        Assert.Equal(50m, Assert.Single(first.Result!.UnsettledIssues).RemainingQuantity);
        Assert.Equal(-100m, issue.QtySettled);
        Assert.Equal(-1000m, issue.CostAmountSettled);
        Assert.Equal(100m, issue.CostAmountAdjustment);
        Assert.Equal(InventTransOpen.Yes, issue.ValueOpen);
        var retry = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 10));
        Assert.Equal(50m, Assert.Single(retry.Result!.UnsettledIssues).RemainingQuantity);

        var second = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(second.Error);
        Assert.Empty(second.Result!.UnsettledIssues);
        Assert.Equal(-150m, issue.QtySettled);
        Assert.Equal(-1600m, issue.CostAmountSettled);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(InventTransOpen.No, issue.ValueOpen);
        Assert.Null((await service.CancelAsync(second.Result.ClosingRecId)).Error);
        Assert.Equal(-100m, issue.QtySettled);
        Assert.Null((await service.CancelAsync(first.Result.ClosingRecId)).Error);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
    }

    [Fact]
    public async Task Inventory_summary_key_prevents_duplicate_on_hand_rows()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventSum>().Add(new InventSum { DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM" });
        await db.SaveChangesAsync();
        db.Set<InventSum>().Add(new InventSum { DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Single(await db.Set<InventSum>().ToListAsync());
        db.Set<InventSum>().Add(new InventSum { DataAreaId = "usmf", ItemId = "ITEM", InventDimId = "DIM" });
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.Set<InventSum>().CountAsync());
    }

    /// <summary>Two receipts at different costs and one issue, so marking changes the outcome.</summary>
    private static void SeedMarkablePool(CycleContext db)
    {
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        db.Set<InventTransOrigin>().AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "R1", ItemId = "ITEM", ReferenceId = "IJ1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "R2", ItemId = "ITEM", ReferenceId = "IJ2", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 3, DataAreaId = "dat", InventTransId = "I1", ItemId = "ITEM", ReferenceId = "IJ3", ReferenceCategory = InventRefType.InventoryJournal });
        db.Set<InventTrans>().AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 2, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1200, DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3, Qty = -50, StatusIssue = StatusIssue.Sold, CostAmountPosted = -550, Voucher = "ISSUE", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 150, PostedValue = 1650 });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", Voucher = "ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue,
            PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
    }

    [Theory]
    [InlineData(0, "1.5", false)]
    [InlineData(0, "2", true)]
    [InlineData(2, "1.23", true)]
    [InlineData(2, "1.234", false)]
    [InlineData(6, "1.00001", false)]
    [InlineData(6, "1.0001", true)]
    public async Task Count_confirmation_respects_inventory_unit_and_storage_precision(int precision, string quantityText, bool accepted)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedCountableItem(db);
        db.Set<UnitOfMeasure>().Local.Single().DecimalPrecision = precision;
        db.Add(new InventJournalTable { DataAreaId = "dat", JournalId = "CNT-1", JournalType = InventJournalType.Counting, CountingFrozen = true });
        db.Add(new InventJournalTrans { RecId = 1, DataAreaId = "dat", JournalId = "CNT-1", JournalType = InventJournalType.Counting,
            ItemId = "ITEM", InventDimId = "DIM", Unit = "EA" });
        await db.SaveChangesAsync();
        var service = new InventJournalPostingService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence(), new TestInventNumbers());
        var quantity = decimal.Parse(quantityText, System.Globalization.CultureInfo.InvariantCulture);

        var error = await service.ConfirmCountAsync("CNT-1", new Dictionary<long, decimal> { [1] = quantity }, default);

        Assert.Equal(accepted, error is null);
        Assert.Equal(accepted, (await db.Set<InventJournalTable>().SingleAsync()).CountingConfirmed);
        if (accepted) Assert.Equal(quantity, (await db.Set<InventJournalTrans>().SingleAsync()).Counted);
    }

    // ---- Counting journal line generation -----------------------------------------------------

    [Fact]
    public async Task Counting_journal_generates_lines_from_on_hand_and_skips_what_it_cannot_count()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedCountableItem(db);
        db.Add(new InventJournalName { DataAreaId = "dat", JournalNameId = "CNT", JournalType = InventJournalType.Counting });
        db.Add(new InventJournalTable
        {
            DataAreaId = "dat", JournalId = "CNT-1", JournalNameId = "CNT",
            JournalType = InventJournalType.Counting, Posted = NoYes.No
        });
        // Countable stock, batch-dimension stock the basic journal cannot count, and an empty bin.
        db.Set<InventDim>().AddRange(
            new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM1", InventSiteId = "S1", InventLocationId = "WH1" },
            new InventDim { RecId = 2, DataAreaId = "dat", InventDimId = "DIM2", InventSiteId = "S1", InventLocationId = "WH1", InventBatchId = "B1" },
            new InventDim { RecId = 3, DataAreaId = "dat", InventDimId = "DIM3", InventSiteId = "S1", InventLocationId = "WH1" });
        db.Set<InventSum>().AddRange(
            new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", PhysicalInvent = 10m },
            new InventSum { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM2", PhysicalInvent = 4m },
            new InventSum { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM3", PhysicalInvent = 0m });
        await db.SaveChangesAsync();

        var work = new TestUnitOfWork(db);
        var service = new InventJournalService(work, new TestCompany(), Sequence(),
            new InventDimensionResolver(work, new TestInventNumbers()));

        var (result, error) = await service.CreateCountingLinesAsync("CNT-1",
            new InventCountingLinesInputDto { InventSiteId = "S1", InventLocationId = "WH1" }, default);

        Assert.Null(error);
        Assert.Equal(1, result!.LinesCreated);           // only the countable basic dimension
        Assert.Equal(1, result.Skipped);                 // the batch dimension
        Assert.Contains(result.Problems, x => x.Contains("batch", StringComparison.OrdinalIgnoreCase));

        var line = await db.Set<InventJournalTrans>().SingleAsync();
        Assert.Equal("ITEM", line.ItemId);
        Assert.Equal("DIM1", line.InventDimId);
        Assert.Equal(0m, line.Counted);                  // the counter still enters the count
        Assert.Equal(InventRefType.PhysicalinventoryCount, line.InventRefType);
        Assert.Equal(1, (await db.Set<InventJournalTable>().SingleAsync()).NumOfLines);

        // A second run must not duplicate a line that is already in the journal.
        var (second, secondError) = await service.CreateCountingLinesAsync("CNT-1",
            new InventCountingLinesInputDto { InventSiteId = "S1", InventLocationId = "WH1" }, default);
        Assert.Null(secondError);
        Assert.Equal(0, second!.LinesCreated);
        Assert.Single(await db.Set<InventJournalTrans>().ToListAsync());
    }

    [Fact]
    public async Task Counting_line_generation_is_refused_for_the_wrong_journal_state()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Add(new InventJournalName { DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment });
        db.Add(new InventJournalTable
        {
            DataAreaId = "dat", JournalId = "ADJ-1", JournalNameId = "ADJ",
            JournalType = InventJournalType.Adjustment, Posted = NoYes.No
        });
        db.Add(new InventJournalTable
        {
            DataAreaId = "dat", JournalId = "CNT-2", JournalNameId = "CNT",
            JournalType = InventJournalType.Counting, Posted = NoYes.Yes
        });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var service = new InventJournalService(work, new TestCompany(), Sequence(),
            new InventDimensionResolver(work, new TestInventNumbers()));

        var wrongType = await service.CreateCountingLinesAsync("ADJ-1", new InventCountingLinesInputDto(), default);
        Assert.Contains("counting journal", wrongType.Error);
        var posted = await service.CreateCountingLinesAsync("CNT-2", new InventCountingLinesInputDto(), default);
        Assert.Contains("already been posted", posted.Error);
        Assert.Empty(await db.Set<InventJournalTrans>().ToListAsync());
    }

    // ---- Inventory value report ---------------------------------------------------------------

    [Fact]
    public async Task Inventory_value_report_values_stock_from_transactions_and_shows_summary_drift()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", Name = "Example item" });
        db.Set<InventTableModule>().Add(new InventTableModule
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM",
            ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA"
        });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem
        {
            RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE"
        });
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM1", InventSiteId = "S1", InventLocationId = "WH1" });
        db.Set<InventTrans>().AddRange(
            new InventTrans
            {
                RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = 100m,
                CostAmountPosted = 1000m, StatusReceipt = StatusReceipt.Purchased,
                DatePhysical = new DateTime(2026, 6, 1), DateFinancial = new DateTime(2026, 6, 1)
            },
            new InventTrans
            {
                RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = -40m,
                CostAmountPosted = -400m, StatusIssue = StatusIssue.Sold,
                DatePhysical = new DateTime(2026, 6, 20), DateFinancial = new DateTime(2026, 6, 20)
            });
        // The summary carries 50 more than the transactions, which the report must surface.
        db.Set<InventSum>().Add(new InventSum
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1",
            PhysicalInvent = 60m, PostedValue = 650m
        });
        await db.SaveChangesAsync();

        var service = new InventValueReportService(new TestUnitOfWork(db), new TestCompany());

        var report = await service.RunAsync(new DateOnly(2026, 6, 30), null, null, null, null, false, 1, 50, default);
        Assert.NotNull(report);
        var row = Assert.Single(report!.Rows);
        Assert.Equal("ITEM", row.ItemId);
        Assert.Equal("TRADE", row.ItemGroupId);
        Assert.Equal("S1", row.InventSiteId);
        Assert.Equal("EA", row.InventoryUnit);
        Assert.Equal(60m, row.PhysicalQuantity);
        Assert.Equal(60m, row.FinancialQuantity);
        Assert.Equal(600m, row.FinancialValue);
        Assert.Equal(10m, row.AverageUnitCost);
        Assert.Equal(50m, row.FinancialValueDifference); // summary minus transactions
        Assert.Equal(0m, row.QuantityDifference);
        Assert.Equal(600m, report.TotalFinancialValue);

        // Before the issue, only the receipt counts.
        var earlier = await service.RunAsync(new DateOnly(2026, 6, 10), null, null, null, null, false, 1, 50, default);
        var earlierRow = Assert.Single(earlier!.Rows);
        Assert.Equal(100m, earlierRow.FinancialQuantity);
        Assert.Equal(1000m, earlierRow.FinancialValue);

        // An item group that nothing belongs to returns no rows.
        var filtered = await service.RunAsync(new DateOnly(2026, 6, 30), null, "OTHER", null, null, false, 1, 50, default);
        Assert.Empty(filtered!.Rows);
    }

    [Fact]
    public async Task Inventory_value_report_values_received_and_delivered_stock_on_the_physical_side()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", Name = "Example item" });
        db.Set<InventTableModule>().Add(new InventTableModule
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM",
            ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA"
        });
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM1", InventSiteId = "S1", InventLocationId = "WH1" });
        db.Set<InventTrans>().AddRange(
            // Invoiced: counts on the financial side only.
            new InventTrans
            {
                RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = 100m,
                CostAmountPosted = 1000m, CostAmountPhysical = 0m, StatusReceipt = StatusReceipt.Purchased,
                DatePhysical = new DateTime(2026, 6, 1), DateFinancial = new DateTime(2026, 6, 1)
            },
            // Received, not invoiced: counts on the physical side only.
            new InventTrans
            {
                RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = 30m,
                CostAmountPhysical = 330m, StatusReceipt = StatusReceipt.Received,
                DatePhysical = new DateTime(2026, 6, 10)
            },
            // Delivered on a packing slip, not invoiced: physical side, issue direction.
            new InventTrans
            {
                RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = -20m,
                CostAmountPhysical = -200m, StatusIssue = StatusIssue.Deducted,
                DatePhysical = new DateTime(2026, 6, 15)
            });
        db.Set<InventSum>().Add(new InventSum
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1",
            PhysicalInvent = 110m, PostedValue = 1000m, PhysicalValue = 130m
        });
        await db.SaveChangesAsync();

        var service = new InventValueReportService(new TestUnitOfWork(db), new TestCompany());
        var report = await service.RunAsync(new DateOnly(2026, 6, 30), null, null, null, null, false, 1, 50, default);
        var row = Assert.Single(report!.Rows);

        // The financial side carries only the invoiced receipt; the physical side carries the
        // receipt and the delivery that have no invoice yet.
        Assert.Equal(100m, row.FinancialQuantity);
        Assert.Equal(1000m, row.FinancialValue);
        Assert.Equal(110m, row.PhysicalQuantity);
        Assert.Equal(130m, row.PhysicalValue);
        Assert.Equal(130m, report.TotalPhysicalValue);

        // Each side reconciles against its own on-hand bucket, so drift is attributable.
        Assert.Equal(1000m, row.SummaryPostedValue);
        Assert.Equal(130m, row.SummaryPhysicalValue);
        Assert.Equal(1130m, row.SummaryValue);
        Assert.Equal(0m, row.FinancialValueDifference);
        Assert.Equal(0m, row.PhysicalValueDifference);

        // Before the delivery, only the receipt's physical value counts.
        var earlier = await service.RunAsync(new DateOnly(2026, 6, 12), null, null, null, null, false, 1, 50, default);
        Assert.Equal(330m, Assert.Single(earlier!.Rows).PhysicalValue);
    }

    /// <summary>The minimum released-item setup an inventory journal line needs to validate.</summary>
    private static void SeedCountableItem(CycleContext db)
    {
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", Name = "Example item" });
        db.Set<InventTableModule>().Add(new InventTableModule
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM",
            ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA"
        });
        db.Set<UnitOfMeasure>().Add(new UnitOfMeasure { RecId = 1, Symbol = "EA", DecimalPrecision = 2 });
        db.Set<InventModelGroup>().Add(new InventModelGroup
        {
            RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO",
            InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes
        });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem
        {
            RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM"
        });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem
        {
            RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE"
        });
        db.Set<InventSite>().Add(new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S1" });
        db.Set<InventLocation>().Add(new InventLocation
        {
            RecId = 1, DataAreaId = "dat", InventLocationId = "WH1", InventSiteId = "S1"
        });
        // Tracking is assigned but no dimension is active, so a basic site/warehouse line is valid.
        db.Set<IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupItem>().Add(
            new IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupItem
        {
            RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", TrackingDimensionGroup = 1
        });
    }

    // ---- Harness ----------------------------------------------------------------------------

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public CycleContext Db { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture._connection.OpenAsync();
            fixture.Db = new CycleContext(new DbContextOptionsBuilder<CycleContext>().UseSqlite(fixture._connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class CycleContext(DbContextOptions<CycleContext> options) : DbContext(options)
    {
        private static readonly Type[] Included =
        [
            typeof(InventDim), typeof(InventTrans), typeof(InventTransOrigin), typeof(InventSum),
            typeof(IAX.IXApi.Modules.Finance.Entities.InventTransPosting),
            typeof(InventModelGroup), typeof(InventModelGroupItem), typeof(InventItemGroupItem),
            typeof(InventPosting), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy), typeof(MainAccount),
            typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
            typeof(InventSettlement), typeof(InventClosing), typeof(InventJournalTrans), typeof(InventJournalTable), typeof(InventJournalName),
            typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry), typeof(SubledgerVoucherGeneralJournalEntry),
            typeof(SourceDocumentHeader), typeof(SourceDocumentLine), typeof(AccountingEvent), typeof(AccountingDistribution),
            typeof(SubledgerJournalEntry), typeof(SubledgerJournalAccountEntry), typeof(SubledgerJournalAccountEntryDistribution),
            typeof(PurchParameters), typeof(VendInvoiceInfoTable), typeof(VendInvoiceInfoLine), typeof(VendInvoiceInfoSubLine),
            typeof(PurchTable), typeof(PurchLine), typeof(PurchPrepayTable), typeof(EcoResCategory), typeof(VendPurchOrderJour), typeof(VendPurchOrderTrans),
            typeof(VendPackingSlipJour), typeof(VendPackingSlipTrans), typeof(VendPackingSlipVersion), typeof(VendPackingSlipTransHistory),
            typeof(VendInvoiceJour), typeof(VendInvoiceTrans), typeof(VendInvoicePackingSlipQuantityMatch),
            typeof(PaymTerm), typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(PurchReceiptAccrual), typeof(VendLedgerAccounts), typeof(InventTransOriginPurchLine), typeof(VendTable), typeof(VendParameters), typeof(VendPaymModeTable),
            typeof(InventSite), typeof(InventLocation), typeof(InventTable), typeof(InventTableModule), typeof(UnitOfMeasure), typeof(UnitOfMeasureConversion),
            typeof(SalesTable), typeof(SalesLine), typeof(CustTable), typeof(CustParameters), typeof(CustPackingSlipJour), typeof(CustPackingSlipTrans),
            typeof(CustPackingSlipVersion), typeof(CustPackingSlipTransHistory),
            typeof(CustInvoiceTable), typeof(CustInvoiceLine), typeof(CustInvoiceJour), typeof(CustInvoiceTrans), typeof(CustRelatedInvoice), typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustLedgerAccounts),
            typeof(Currency), typeof(ExchangeRate), typeof(ExchangeRateType), typeof(ExchangeRateCurrencyPair),
            typeof(DirPartyTable), typeof(CustPaymModeTable), typeof(TaxGroupHeading), typeof(TaxItemGroupHeading),
            typeof(TaxGroupData), typeof(TaxOnItem), typeof(TaxData), typeof(TaxTable), typeof(TaxLedgerAccountGroup), typeof(TaxTrans),
            typeof(InventTransOriginSalesLine), typeof(BankAccountTable), typeof(BankAccountTrans), typeof(LedgerJournalTable), typeof(LedgerJournalTrans),
            typeof(ReturnDispositionCode), typeof(ReturnReasonCode),
            // Purchase trade agreements are resolved when a purchase line opts in, and published
            // from the agreement journal.
            typeof(PriceDiscTable), typeof(PriceDiscAdmTable), typeof(PriceDiscAdmTrans),
            typeof(PriceDiscAdmName), typeof(PriceDiscGroup),
            // Invoicing posts the order's charges.
            typeof(MarkupTable), typeof(MarkupTrans),
            // Journal line validation reads the item's dimension groups and count-group setup.
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResStorageDimensionGroupItem),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResStorageDimensionGroupFldSetup),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupItem),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupFldSetup),
            typeof(InventItemLocation)
        ];

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var included = new HashSet<Type>(Included);
            foreach (var type in typeof(InventTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity<long>).IsAssignableFrom(t) && !included.Contains(t)))
                builder.Ignore(type);
            foreach (var type in Included) Configure(builder, type);
            builder.Entity<CustPackingSlipVersion>().Ignore(x => x.DataAreaId);
            builder.Entity<VendPackingSlipVersion>().Ignore(x => x.DataAreaId);
            builder.Entity<VendPackingSlipTransHistory>().HasOne(x => x.PackingSlipLine)
                .WithMany().HasForeignKey(x => x.VendPackingSlipTrans);
            builder.Entity<CustPackingSlipTransHistory>().HasOne(x => x.PackingSlipLine)
                .WithMany().HasForeignKey(x => x.CustPackingSlipTrans).OnDelete(DeleteBehavior.Restrict);
        }

        private static void Configure(ModelBuilder builder, Type entity)
        {
            var e = builder.Entity(entity);
            e.HasKey(nameof(Entity<long>.RecId));
            e.Property(nameof(Entity<long>.RecId)).ValueGeneratedOnAdd();
            e.Ignore(nameof(Entity<long>.RowVersion));
            foreach (var property in entity.GetProperties())
            {
                var t = property.PropertyType;
                if (t != typeof(string) && t != typeof(byte[]) && !t.IsValueType)
                    e.Ignore(property.Name);
            }
        }
    }

    public class TestSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name == nameof(ISysNumberSequenceService.NextAsync))
                return Task.FromResult(new NextSequenceResultDto { EntityName = "X", Value = ++_n, Code = $"X-{_n:D6}" });
            throw new NotSupportedException(method?.Name);
        }
    }

    private sealed class TestUser : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        private IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, CycleRepositoryProxy<T>>();
            ((CycleRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) await _tx.CommitAsync(ct); }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) await _tx.RollbackAsync(ct); }
        public void Dispose() { }
    }

    public class CycleRepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "GetQueryable": return Context.Set<T>().AsQueryable();
                case "AddAsync": Context.Add((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "UpdateAsync": return Task.FromResult((T)args![0]!);
                case "RemoveAsync": Context.Remove((T)args![0]!); return Task.CompletedTask;
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }
}
