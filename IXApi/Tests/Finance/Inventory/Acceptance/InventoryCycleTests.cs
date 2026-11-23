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
    // ---- Reservation engine (Phase 3 Ãƒâ€šÃ‚Â§7ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Å“8) --------------------------------------------------

    // ---- FIFO inventory close (Phase 7 worked example: 1,600 COGS / 600 remaining) -----------

    // ---- Purchase order Milestone 1 (Purchorder.md Ãƒâ€šÃ‚Â§12.2): PO 100 @ 50, receive/invoice 60 + 40 ----

    // ---- Two-step receipt: arrival registration then product receipt (Logic_Readonly.md §10) ----

    // ---- Purchase order cancellation: only the unreceived remainder goes away ----

    // ---- Line-level cancellation and open-quantity reduction ----

    // ---- Sales order cycle (CycleSalesorder.md / ARTables.md Ãƒâ€šÃ‚Â§14 state table) -------------------

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

    // ---- Returns (CycleSalesorder.md 10.3, Purchorder.md 8) ---------------------------------------

    // ---- Sales order cancellation of a partly delivered order ----

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

    // ---- Tax origin (D365 TaxBaseType) and the pricing date (D365 PriceDateType) ----

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

    // ---- The inventory reference cycle (CycleErpExamples/Inventory/Logic_Readonly.md) --------------

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

    // ---- Counting journal line generation -----------------------------------------------------

    // ---- Inventory value report ---------------------------------------------------------------

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
            typeof(InventPosting), typeof(DimensionAttributeValueCombination), typeof(DimensionAttributeValueSet),
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
            // A stated batch or serial is checked against those defined for the item.
            typeof(InventBatch), typeof(InventSerial),
            // The module-level period guard is wrapped in a FindEntityType check, so without this
            // registration it is skipped silently rather than enforced.
            typeof(LedgerPeriodModuleAccessControl),
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
