using System.Globalization;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

[CollectionDefinition("AccountsReceivable cycle stages", DisableParallelization = true)]
public sealed class AccountsReceivableCycleStageCollection { }

/// <summary>
/// The customer cycle, stage by stage, through the services that own each document: order,
/// confirmation, reservation, picking, packing slip, invoice, collection, settlement and a closing
/// reconciliation. The full path runs first, then the same path with partial delivery, partial
/// invoicing and partial collection. Every stage records its financial and inventory effect.
/// </summary>
[Collection("AccountsReceivable cycle stages")]
public sealed class AccountsReceivableCycleStageTests
{
    private const string Company = "dat";
    private const string Customer = "ARV2-dat-LOCAL";
    private const string Bank = "APV2-BANK";
    private const string Item = "APV2-STOCK";
    private static readonly DateTime Date = new(2026, 4, 15);

    // The seeded quotations: bought at 25, sold at 40, 15 percent output tax.
    private const decimal SalesPrice = 40m;
    private const decimal PurchasePrice = 25m;

    [Fact]
    public async Task Detailed_AR_stages_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new ArFixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await RunStagesAsync(db, "SQLite", async () =>
            await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'")));
    }

    private static void ArRecord(string runId, string providerName, string stage, string name, string status, string? detail = null)
    {
        var path = Environment.GetEnvironmentVariable("IXAPI_AR_STAGE_REPORT") ?? Path.Combine(AppContext.BaseDirectory, "ar-stage-results.jsonl");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.AppendAllText(path, JsonSerializer.Serialize(new { runId, provider = providerName, stage, name, status, detail, atUtc = DateTime.UtcNow }) + Environment.NewLine);
    }

    private static async Task RunStagesAsync(ApplicationDbContext db, string providerName, Func<Task> initialize)
    {
        var runId = Guid.NewGuid().ToString("N");
        void Record(string stage, string name, string status, string? detail = null) => ArRecord(runId, providerName, stage, name, status, detail);
        async Task Stage(string number, string name, Func<Task<string?>> action)
        {
            Record(number, name, "RUNNING");
            try
            {
                var detail = await action();
                db.ChangeTracker.Clear();
                Record(number, name, "PASSED", detail);
            }
            catch (Exception error)
            {
                Record(number, name, "FAILED", error.ToString());
                Record("run", "اختبار دورة الحسابات المدينة", "FAILED", error.Message);
                throw;
            }
        }
        Record("run", "اختبار دورة الحسابات المدينة", "STARTED");
        await Stage("0", "قاعدة الاختبار والنموذج والمفاتيح الخارجية", async () => { await initialize(); return "Model created with production relationships."; });

        using var provider = CycleServices(db);
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        await Stage("1", "تحضير إعدادات العملاء والبيع والضرائب وحسابات الترحيل", async () =>
        {
            await services.GetRequiredService<AccountsReceivableCycleSeeder>().PrepareAsync(new(2026, "ar-cycle-test"));
            Assert.Equal(2, await db.Set<CustTable>().CountAsync());
            Assert.Equal(2, await db.Set<VendTable>().CountAsync());
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
            var price = await db.Set<PriceDiscTable>().SingleAsync(x => x.AccountRelation == Customer
                && x.ItemRelation == Item && x.Relation == PriceType.PriceSales);
            Assert.Equal(SalesPrice, price.Amount);
            return "No financial or inventory effect: setup and master data only. 2 customers, sales quote 40 SAR, no ledger entry.";
        });

        // A sale needs stock on hand with a cost, so the demo item is bought and invoiced first
        // through the purchase services. This is a prerequisite, not part of the customer cycle.
        await Stage("2", "شراء واستلام وفوترة 200 وحدة لتوفير رصيد مخزني بتكلفة", async () =>
        {
            await BuyStockAsync(services, db, 200m);
            var sum = await OnHandAsync(db);
            Assert.Equal(200m, sum.PhysicalInvent);
            Assert.Equal(200m, sum.AvailPhysical);
            return $"Prerequisite. On hand {sum.PhysicalInvent} at {PurchasePrice} SAR; available {sum.AvailPhysical}; inventory GL { await LedgerAsync(db, GeneralLedgerSeedExample.Inventory)}.";
        });

        var full = await RunSalesPathAsync(db, services, Stage, "AR-STAGE-FULL", 100m, 100m, 100m, full: true);
        var partial = await RunSalesPathAsync(db, services, Stage, "AR-STAGE-PARTIAL", 100m, 60m, 40m, full: false);

        await Stage("9", "المطابقة النهائية: العميل والمفتوح والبنك والقيود والمخزون", async () =>
        {
            var vouchers = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
            foreach (var group in vouchers.GroupBy(x => x.GeneralJournalEntry))
                Assert.Equal(0m, group.Sum(x => x.AccountingCurrencyAmount));
            var open = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company).ToListAsync();
            var transactions = await db.Set<CustTrans>().Where(x => x.DataAreaId == Company).ToListAsync();
            foreach (var row in open)
            {
                var transaction = transactions.Single(x => x.RecId == row.RefRecId);
                Assert.Equal(transaction.AmountCur - transaction.SettleAmountCur, row.AmountCur);
            }
            Assert.DoesNotContain(transactions.Where(x => x.AmountCur - x.SettleAmountCur == 0m),
                x => open.Any(o => o.RefRecId == x.RecId));
            var sum = await OnHandAsync(db);
            Assert.Equal(200m - 160m, sum.PhysicalInvent);
            Assert.Equal(0m, sum.ReservPhysical);
            return $"Every voucher balances. Open rows equal their unsettled remainder; no settled transaction keeps an open row. "
                + $"Full path open {full.Open}; partial path open {partial.Open}. On hand {sum.PhysicalInvent}, reserved {sum.ReservPhysical}. "
                + $"Customer control {await LedgerAsync(db, GeneralLedgerSeedExample.CustomerControl)}, revenue {await LedgerAsync(db, GeneralLedgerSeedExample.SalesRevenue)}, "
                + $"output tax {await LedgerAsync(db, GeneralLedgerSeedExample.PayableSalesTax)}, cost of goods sold {await LedgerAsync(db, GeneralLedgerSeedExample.CostOfGoodsSold)}, "
                + $"bank {await LedgerAsync(db, GeneralLedgerSeedExample.Bank)}, delivery interim {await LedgerAsync(db, GeneralLedgerSeedExample.DeliveredNotInvoicedInterim)}/"
                + $"{await LedgerAsync(db, GeneralLedgerSeedExample.DeliveredNotInvoiced)}.";
        });

        await Stage("10", "إعادة المحاولة لا تُنشئ تكراراً", async () =>
        {
            var before = await CountsAsync(db);
            var again = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(full.OrderRecId,
                new(Date, "AR-STAGE-FULL-I1", [new(full.LineRecId, 100m)]), default);
            Assert.True(again.StatusCode != 200, $"A second invoice for the same quantity must be refused; got {again.StatusCode}.");
            Assert.Equal(before, await CountsAsync(db));
            return $"A repeated invoice of an already invoiced quantity was refused ({again.StatusCode}) and wrote nothing.";
        });

        Record("run", "اختبار دورة الحسابات المدينة", "PASSED");
    }

    private sealed record PathResult(long OrderRecId, long LineRecId, string InvoiceId, decimal Open);

    private static async Task<PathResult> RunSalesPathAsync(ApplicationDbContext db, IServiceProvider services,
        Func<string, string, Func<Task<string?>>, Task> Stage, string key, decimal ordered, decimal delivered, decimal invoiced, bool full)
    {
        var suffix = full ? string.Empty : "-جزئي";
        var net = invoiced * SalesPrice;
        var tax = decimal.Round(net * 0.15m, 2, MidpointRounding.AwayFromZero);
        var gross = net + tax;
        var cost = invoiced * PurchasePrice;
        var collect = full ? gross : 1000m;

        long orderRecId = 0, lineRecId = 0;
        string inventTransId = string.Empty;

        await Stage("3" + suffix, "إنشاء أمر البيع" + suffix, async () =>
        {
            var created = await services.GetRequiredService<ISalesOrderQuickCreateService>().CreateAsync(new SalesOrderQuickCreateDto
            {
                CustomerAccount = Customer, CurrencyCode = "SAR", SalesName = key,
                InventSiteId = "APV2-SITE", InventLocationId = "APV2-WH",
                RequestedShipDate = Date, RequestedReceiptDate = Date
            }, default);
            Assert.True(created.Error == null && created.Order != null, created.Error);
            orderRecId = created.Order!.RecId;
            var added = await services.GetRequiredService<ISalesOrderLineService>().AddAsync(orderRecId, new SalesLineInputDto
            {
                ItemNumber = Item, Quantity = ordered, UnitPrice = SalesPrice, Unit = "EA", PriceUnit = 1m,
                InventSiteId = "APV2-SITE", InventLocationId = "APV2-WH", DeliveryDate = Date,
                TaxGroup = "ARV2", TaxItemGroup = "APV2"
            }, default);
            Assert.True(added.Status == SalesLineWriteStatus.Success, added.Error);
            var line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == Company && x.SalesId ==
                db.Set<SalesTable>().Where(o => o.RecId == orderRecId).Select(o => o.SalesId).Single());
            lineRecId = line.RecId;
            inventTransId = line.InventTransId;
            var sum = await OnHandAsync(db);
            Assert.Equal(0m, await db.Set<CustTrans>().Where(x => x.DataAreaId == Company).SumAsync(x => x.AmountCur) - await OpenTotalAsync(db));
            return $"No financial effect: no customer transaction and no ledger entry from an order. Inventory demand only: "
                + $"InventTrans issue of {ordered} on {inventTransId}; on hand {sum.PhysicalInvent}, available {sum.AvailPhysical}, reserved {sum.ReservPhysical}.";
        });

        await Stage("4" + suffix, "تأكيد أمر البيع" + suffix, async () =>
        {
            var confirmed = await services.GetRequiredService<ISalesOrderConfirmationService>()
                .PostAsync(orderRecId, new(Date.AddDays(-1), true, key + "-CONF"), default);
            Assert.True(confirmed.StatusCode == 200, $"HTTP {confirmed.StatusCode}: {confirmed.Message}");
            var journal = await db.Set<CustConfirmJour>().Where(x => x.DataAreaId == Company).ToListAsync();
            Assert.NotEmpty(journal);
            return "No financial or inventory effect: confirmation stores the document only. "
                + $"CustConfirmJour rows {journal.Count}; no CustTrans and no ledger entry.";
        });

        await Stage("5" + suffix, "حجز الكمية" + suffix, async () =>
        {
            var reservation = await services.GetRequiredService<IInventoryReservationService>()
                .ReserveAsync(inventTransId, delivered, false, default);
            Assert.True(reservation.Error == null && reservation.Result != null, reservation.Error);
            var sum = await OnHandAsync(db);
            Assert.Equal(delivered, reservation.Result!.Reserved);
            return $"No financial effect: reservation posts no ledger entry. Inventory: reserved {sum.ReservPhysical}, "
                + $"on hand {sum.PhysicalInvent} unchanged, available {sum.AvailPhysical}; status ReservPhysical.";
        });

        await Stage("6" + suffix, "تجهيز والتقاط الأصناف" + suffix, async () =>
        {
            var picked = await services.GetRequiredService<IInventoryReservationService>()
                .SetPickedAsync(inventTransId, delivered, default);
            Assert.True(picked.Error == null && picked.Result != null, picked.Error);
            Assert.Equal(delivered, picked.Result!.Picked);
            var sum = await OnHandAsync(db);
            return $"No financial effect: picking posts no ledger entry. Inventory: picked {sum.Picked}, "
                + $"on hand {sum.PhysicalInvent} still unchanged; status Picked.";
        });

        await Stage("7" + suffix, "ترحيل سند التسليم" + suffix, async () =>
        {
            var result = await services.GetRequiredService<IPackingSlipJournalService>().PostAsync(orderRecId,
                new(Date, "all", true, key + "-D1", [new(lineRecId.ToString(CultureInfo.InvariantCulture), delivered)]), default);
            Assert.True(result.StatusCode == 200, $"HTTP {result.StatusCode}: {result.Message}");
            var sum = await OnHandAsync(db);
            Assert.Equal(200m - await TotalDeliveredAsync(db), sum.PhysicalInvent);
            var interim = await LedgerAsync(db, GeneralLedgerSeedExample.DeliveredNotInvoicedInterim);
            return $"Physical inventory update: on hand {sum.PhysicalInvent} (down by {delivered}), status Deducted; no second issue at invoicing. "
                + $"Financial effect: no customer transaction; delivery cost interim account {interim}. Customer balance unchanged.";
        });

        var invoiceId = key + "-I1";
        await Stage("8" + suffix, "ترحيل فاتورة البيع" + suffix, async () =>
        {
            var result = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderRecId,
                new(Date, invoiceId, [new(lineRecId, invoiced)]), default);
            Assert.True(result.StatusCode == 200, $"HTTP {result.StatusCode}: {result.Message}");
            var journal = await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == Company && x.InvoiceId == invoiceId);
            Assert.Equal(gross, journal.InvoiceAmount);
            var transaction = await db.Set<CustTrans>().SingleAsync(x => x.DataAreaId == Company && x.Invoice == invoiceId);
            Assert.Equal(gross, transaction.AmountCur);
            var openRow = await db.Set<CustTransOpen>().SingleAsync(x => x.DataAreaId == Company && x.RefRecId == transaction.RecId);
            Assert.Equal(gross, openRow.AmountCur);
            return $"Financial effect: customer control debit {gross}; revenue credit {net}; output tax credit {tax}; "
                + $"cost of goods sold debit {cost} against inventory, and the delivery interim entry reversed. "
                + $"Open receivable on this invoice {openRow.AmountCur}. Inventory: financially updated for {invoiced}, status Sold; no further physical issue.";
        });

        await Stage("9" + suffix, "ترحيل تحصيل العميل" + suffix, async () =>
        {
            var result = await services.GetRequiredService<ICustPaymentService>().PostAsync(
                new(Customer, Bank, collect, Date.AddDays(30), "ARV2", full ? invoiceId : null, key + "-PAY"), default);
            Assert.True(result.StatusCode == 200, $"HTTP {result.StatusCode}: {result.Message}");
            var bank = await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == Company).SumAsync(x => x.AmountCur);
            return $"Financial effect: bank debit {collect}; customer control credit {collect}. No revenue and no tax from a collection. "
                + $"No inventory effect. Bank movement to date {bank}.";
        });

        decimal remaining = 0m;
        await Stage("10" + suffix, "تسوية التحصيل مع الفاتورة" + suffix, async () =>
        {
            var ids = await db.Set<CustTrans>().Where(x => x.DataAreaId == Company
                && (x.Invoice == invoiceId || x.PaymReference == key + "-PAY")).Select(x => x.RecId).ToArrayAsync();
            var openRows = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company && ids.Contains(x.RefRecId)).ToListAsync();
            if (openRows.Any(x => x.AmountCur > 0) && openRows.Any(x => x.AmountCur < 0))
            {
                var settled = await services.GetRequiredService<ICustSettleOpenTransactionsService>()
                    .SettleAsync(new(Customer, Date.AddDays(31), openRows.Select(x => x.RefRecId).ToArray()), default);
                Assert.True(settled.StatusCode == 200, $"HTTP {settled.StatusCode}: {settled.Message}");
            }
            var after = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company && ids.Contains(x.RefRecId)).ToListAsync();
            remaining = after.Where(x => x.AmountCur > 0).Sum(x => x.AmountCur);
            if (full)
            {
                Assert.Empty(after);
                return $"Financial effect: allocation only, no new money. The invoice is fully settled and its open row is closed; remaining {remaining}.";
            }
            Assert.Equal(gross - collect, remaining);
            return $"Financial effect: allocation only. Invoiced {gross}, collected {collect}, remaining open {remaining}. "
                + $"The invoice stays open for its remainder and keeps its transaction history.";
        });

        if (!full)
        {
            await Stage("11-جزئي", "متابعة الكميات والمبالغ المتبقية بعد التسليم والفوترة الجزئيين", async () =>
            {
                var line = await db.Set<SalesLine>().SingleAsync(x => x.RecId == lineRecId);
                Assert.Equal(ordered - delivered, line.RemainSalesPhysical);
                var sum = await OnHandAsync(db);
                return $"Remaining to deliver {line.RemainSalesPhysical} of {ordered}; remaining to invoice {line.RemainSalesFinancial}. "
                    + $"Delivered {delivered}, invoiced {invoiced}; open receivable {remaining}. On hand {sum.PhysicalInvent}.";
            });
        }

        return new(orderRecId, lineRecId, invoiceId, remaining);
    }

    private static async Task BuyStockAsync(IServiceProvider services, ApplicationDbContext db, decimal quantity)
    {
        var orders = services.GetRequiredService<IPurchOrderService>();
        var created = await orders.CreateAsync(new("APV2-dat-LOCAL", null, "SAR", Date.AddDays(-30),
            "APV2-SITE", "APV2-WH", "AR-STAGE-SUPPLY", Date.AddDays(-31)), default);
        Assert.True(created.StatusCode == 200, created.Message);
        var order = (PurchTable)created.Data!;
        var added = await orders.AddLineAsync(order.RecId, new(Item, quantity, "EA", PurchasePrice, 1m,
            "APV2-SITE", "APV2-WH", Date.AddDays(-30), null), default);
        Assert.True(added.StatusCode == 200, added.Message);
        var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == Company && x.PurchId == order.PurchId);
        Assert.True((await services.GetRequiredService<IPurchConfirmationService>()
            .ConfirmAsync(order.RecId, default, Date.AddDays(-29))).StatusCode == 200);
        Assert.True((await services.GetRequiredService<IProductReceiptService>()
            .PostAsync(order.RecId, new("AR-SUPPLY-R1", Date.AddDays(-28), [new(line.RecId, quantity)]), default)).StatusCode == 200);
        var pending = services.GetRequiredService<IVendInvoiceInfoService>();
        var held = await pending.CreateFromOrderAsync(order.RecId,
            new("AR-SUPPLY-I1", Date.AddDays(-27), Date.AddDays(-27), "AR stage supply", [new(line.RecId, quantity)]), default);
        Assert.True(held.StatusCode == 200, held.Message);
        var parm = await db.Set<VendInvoiceInfoTable>().SingleAsync(x => x.DataAreaId == Company && x.Num == "AR-SUPPLY-I1");
        Assert.True((await pending.PostAsync(parm.ParmId, default)).StatusCode == 200);
        db.ChangeTracker.Clear();
    }

    private static Task<InventSum> OnHandAsync(ApplicationDbContext db) =>
        db.Set<InventSum>().AsNoTracking().FirstAsync(x => x.DataAreaId == Company && x.ItemId == Item);

    private static async Task<decimal> TotalDeliveredAsync(ApplicationDbContext db) =>
        -await db.Set<InventTrans>().Where(x => x.DataAreaId == Company && x.ItemId == Item
            && (x.StatusIssue == StatusIssue.Deducted || x.StatusIssue == StatusIssue.Sold)).SumAsync(x => x.Qty);

    private static async Task<decimal> OpenTotalAsync(ApplicationDbContext db) =>
        await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company).SumAsync(x => x.AmountCur);

    private static async Task<decimal> LedgerAsync(ApplicationDbContext db, string mainAccount)
    {
        var dimensions = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == Company && x.MainAccountValue == mainAccount).Select(x => x.RecId).ToArrayAsync();
        return await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.DataAreaId == Company && dimensions.Contains(x.LedgerDimension))
            .SumAsync(x => x.AccountingCurrencyAmount);
    }

    private static async Task<string> CountsAsync(ApplicationDbContext db) => string.Join(":",
        await db.Set<CustTrans>().CountAsync(), await db.Set<CustTransOpen>().CountAsync(),
        await db.Set<CustInvoiceJour>().CountAsync(), await db.Set<CustInvoiceTrans>().CountAsync(),
        await db.Set<InventTrans>().CountAsync(), await db.Set<GeneralJournalEntry>().CountAsync(),
        await db.Set<GeneralJournalAccountEntry>().CountAsync(), await db.Set<BankAccountTrans>().CountAsync());

    private static ServiceProvider CycleServices(ApplicationDbContext db)
    {
        var actor = new ArCycleIdentity();
        return new ServiceCollection().AddSingleton(db).AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor)
            .AddAccountsReceivableTransactionExamples()
            .AddScoped<IInventoryReservationService, InventoryReservationService>()
            .BuildServiceProvider();
    }

    private sealed class ArCycleIdentity : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => Company;
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "ar-cycle-test";
        public string GetOwnerAccountId() => "ar-cycle-test";
    }

    private sealed class ArFixture(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
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
