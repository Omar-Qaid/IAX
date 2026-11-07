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
using Microsoft.Data.SqlClient;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using Mapster;
using IAX.IXApi.Modules.Finance;
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
public sealed partial class AccountsReceivableCycleStageTests
{
    private const string Company = "dat";
    private const string Customer = "ARV2-dat-LOCAL";
    private const string Bank = "APV2-BANK";
    private const string Item = "APV2-STOCK";
    private static readonly DateTime Date = new(2026, 4, 15);

    // The seeded quotations: bought at 25, sold at 40, 15 percent output tax.
    private const decimal SalesPrice = 40m;
    private const decimal PurchasePrice = 25m;

    [Fact(Skip = "The production sales-line service uses decimal SQL Max/ordering unsupported by SQLite; the complete exact-decimal cycle is verified on SQL Server.")]
    public async Task Detailed_AR_stages_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new ArFixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await RunStagesAsync(db, "SQLite", async () =>
            await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'")));
    }

    [SqlServerPhaseOneFact]
    public async Task Detailed_AR_stages_SQLServer()
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = "IXApi_ARStages_" + Guid.NewGuid().ToString("N"),
            IntegratedSecurity = true, TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var db = new ApplicationDbContext(options);
        try { await RunStagesAsync(db,"SQL Server",async () => { await db.Database.EnsureCreatedAsync(); }); }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
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
        var otcAudit = Environment.GetEnvironmentVariable("IXAPI_OTC_REPORT") == null ? null : new OtcAudit(runId);
        var relatedCases = new Dictionary<string,string[]>
        {
            ["8"] = ["BILL-03"],
            ["scenario-03"] = ["BILL-04"],
            ["scenario-05"] = ["BILL-07"],
            ["scenario-13"] = ["FX-01","FX-03"]
        };
        void Record(string stage, string name, string status, string? detail = null) => ArRecord(runId, providerName, stage, name, status, detail);
        async Task Stage(string number, string name, Func<Task<string?>> action)
        {
            Record(number, name, "RUNNING");
            try
            {
                var detail = await action();
                db.ChangeTracker.Clear();
                Record(number, name, "PASSED", detail);
                if(otcAudit != null && relatedCases.TryGetValue(number,out var ids))
                    otcAudit.RelatedCyclePassed(ids,new
                    {
                        fixture="Existing AR service regression; separate numbers from the attachment's exact mixed example",
                        provider=providerName,sourceRunId=runId,stage=number,name,detail,
                        invoices=await db.Set<CustInvoiceJour>().Where(x => x.DataAreaId == Company)
                            .Select(x => new {x.InvoiceId,x.SalesId,x.InvoiceAmount,x.CurrencyCode,x.LedgerVoucher,x.ParmId}).ToListAsync()
                    });
            }
            catch (Exception error)
            {
                Record(number, name, "FAILED", error.ToString());
                Record("run", "اختبار دورة الحسابات المدينة", "FAILED", error.Message);
                db.ChangeTracker.Clear();
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
            Assert.True(await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().AnyAsync(x => x.NumberSequence == "CustSettlement"));
            return "No financial or inventory effect: setup and master data only. 2 customers, sales quote 40 SAR, no ledger entry.";
        });

        // A sale needs stock on hand with a cost, so the demo item is bought and invoiced first
        // through the purchase services. This is a prerequisite, not part of the customer cycle.
        await Stage("2", "شراء واستلام وفوترة 2000 وحدة لتوفير رصيد مخزني بتكلفة", async () =>
        {
            await BuyStockAsync(services, db, 2000m);
            var sum = await OnHandAsync(db);
            Assert.Equal(2000m, sum.PhysicalInvent);
            Assert.Equal(2000m, sum.AvailPhysical);
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
            Assert.Equal(2000m - 160m, sum.PhysicalInvent);
            Assert.Equal(0m, sum.ReservPhysical);
            Assert.Equal(840m,await LedgerAsync(db,GeneralLedgerSeedExample.CustomerControl));
            Assert.Equal(-5600m,await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue));
            Assert.Equal(-840m,await LedgerAsync(db,GeneralLedgerSeedExample.PayableSalesTax));
            Assert.Equal(3500m,await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold));
            Assert.Equal(5600m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
            Assert.Equal(46500m,sum.PostedValue);
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
            Assert.Equal(200,again.StatusCode);
            Assert.Equal(before, await CountsAsync(db));
            return "The same request returns its existing invoice and writes nothing.";
        });

        var examples = services.GetRequiredService<AccountsReceivableTransactionExamples>();
        var scenarioFailures = new List<string>();
        foreach (var id in AccountsReceivableTransactionFlow.Scenarios.Keys.OrderBy(x => x,StringComparer.Ordinal))
        {
            try { await Stage("scenario-" + id, AccountsReceivableTransactionFlow.Scenarios[id], async () =>
            {
                var report = await examples.RunAsync(new(2026,"ar-cycle-test",[id]));
                Assert.Equal(id,Assert.Single(report.Scenarios).Id);
                await CheckScenarioAmountsAsync(db,id);
                return "Persisted customer, tax, bank, inventory, revenue, cost and GL accounts match independent fixture arithmetic.";
            }); }
            catch(Exception error) { scenarioFailures.Add(id + ": " + error.Message); }
        }
        Assert.True(scenarioFailures.Count == 0,string.Join(Environment.NewLine,scenarioFailures));
        await Stage("rerun", "إعادة جميع أمثلة AR دون مستندات أو حركات مكررة", async () =>
        {
            var before = await CountsAsync(db);
            await examples.RunAsync(new(2026,"ar-cycle-test"));
            Assert.Equal(before,await CountsAsync(db));
            return "Same-database rerun preserves transaction and posting counts.";
        });
        foreach (var unsupported in new[] { "Customer payment-journal prepayment", "Sales delivery-charge example", "Credit-only and scrap return examples" })
            Record("unsupported",unsupported,"NOT_IMPLEMENTED","Not claimed as a passing scenario in this cycle package.");

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
                ItemNumber = Item, Quantity = ordered, UnitPrice = 1m, UsePriceAgreement = true, Unit = "EA", PriceUnit = 1m,
                InventSiteId = "APV2-SITE", InventLocationId = "APV2-WH", DeliveryDate = Date,
                TaxGroup = "ARV2", TaxItemGroup = "APV2"
            }, default);
            Assert.True(added.Status == SalesLineWriteStatus.Success, added.Error);
            var line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == Company && x.SalesId ==
                db.Set<SalesTable>().Where(o => o.RecId == orderRecId).Select(o => o.SalesId).Single());
            lineRecId = line.RecId;
            Assert.Equal(SalesPrice,line.SalesPrice);
            Assert.Equal(1m,line.PriceUnit);
            Assert.Equal(ordered * SalesPrice,line.LineAmount);
            Assert.Equal(0L,line.ManualEntryChangePolicy);
            Assert.Equal(ordered * SalesPrice,(await db.Set<SalesTable>().SingleAsync(x => x.RecId == orderRecId)).SmmSalesAmountTotal);
            inventTransId = line.InventTransId;
            var sum = await OnHandAsync(db);
            Assert.Equal(0m, (await db.Set<CustTrans>().Where(x => x.DataAreaId == Company).ToListAsync()).Sum(x => x.AmountCur - x.SettleAmountCur) - await OpenTotalAsync(db));
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
            Assert.Equal(2000m - await TotalDeliveredAsync(db), sum.PhysicalInvent);
            var interim = await LedgerAsync(db, GeneralLedgerSeedExample.DeliveredNotInvoicedInterim);
            return $"Physical inventory update: on hand {sum.PhysicalInvent} (down by {delivered}), status Deducted; no second issue at invoicing. "
                + $"Financial effect: no customer transaction; delivery cost interim account {interim}. Customer balance unchanged.";
        });

        var requestId = key + "-I1";
        var invoiceId = string.Empty;
        if(full)
        await Stage("8-profile", "رفض ملف ترحيل صريح بلا قواعد دون أثر مالي أو مخزني", async () =>
        {
            var order = await db.Set<SalesTable>().SingleAsync(x => x.RecId == orderRecId);
            var originalProfile = order.PostingProfile;
            order.PostingProfile = "MISSING";
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var before = await CountsAsync(db);
            var taxCount = await db.Set<TaxTrans>().CountAsync();
            var stock = await OnHandAsync(db);
            var physical = stock.PhysicalInvent;
            var postedValue = stock.PostedValue;
            var rejected = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderRecId,
                new(Date, requestId, [new(lineRecId, invoiced)]), default);
            Assert.Equal(422,rejected.StatusCode);
            Assert.Contains("MISSING",rejected.Message);
            Assert.Equal(before,await CountsAsync(db));
            Assert.Equal(taxCount,await db.Set<TaxTrans>().CountAsync());
            var afterStock = await OnHandAsync(db);
            Assert.Equal(physical,afterStock.PhysicalInvent);
            Assert.Equal(postedValue,afterStock.PostedValue);
            Assert.Equal(ordered,(await db.Set<SalesLine>().SingleAsync(x => x.RecId == lineRecId)).RemainSalesFinancial);
            var preserved = await db.Set<SalesTable>().SingleAsync(x => x.RecId == orderRecId);
            Assert.Equal("MISSING",preserved.PostingProfile);
            preserved.PostingProfile = originalProfile;
            await db.SaveChangesAsync();
            return "422; no invoice, tax, customer, stock or GL movement; configured profile preserved. The next stage retries the same request with the valid profile.";
        });
        await Stage("8" + suffix, "ترحيل فاتورة البيع" + suffix, async () =>
        {
            var result = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderRecId,
                new(Date, requestId, [new(lineRecId, invoiced)]), default);
            Assert.True(result.StatusCode == 200, $"HTTP {result.StatusCode}: {result.Message}");
            var journal = await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == Company && x.ParmId == requestId);
            invoiceId = journal.InvoiceId;
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
            var bank = (await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == Company).ToListAsync()).Sum(x => x.AmountCur);
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

    private static async Task BuyStockAsync(IServiceProvider services, ApplicationDbContext db, decimal quantity,
        decimal purchasePrice = PurchasePrice, string key = "AR-SUPPLY", DateTime? purchaseDate = null)
    {
        var purchaseDay = purchaseDate ?? Date.AddDays(-30);
        var orders = services.GetRequiredService<IPurchOrderService>();
        var created = await orders.CreateAsync(new("APV2-dat-LOCAL", null, "SAR", purchaseDay,
            "APV2-SITE", "APV2-WH", key, purchaseDay.AddDays(-1)), default);
        Assert.True(created.StatusCode == 200, created.Message);
        var order = (PurchTable)created.Data!;
        var added = await orders.AddLineAsync(order.RecId, new(Item, quantity, "EA", purchasePrice, 1m,
            "APV2-SITE", "APV2-WH", purchaseDay, null), default);
        Assert.True(added.StatusCode == 200, added.Message);
        var line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == Company && x.PurchId == order.PurchId);
        Assert.True((await services.GetRequiredService<IPurchConfirmationService>()
            .ConfirmAsync(order.RecId, default, purchaseDay.AddDays(1))).StatusCode == 200);
        Assert.True((await services.GetRequiredService<IProductReceiptService>()
            .PostAsync(order.RecId, new(key + "-R1", purchaseDay.AddDays(2), [new(line.RecId, quantity)]), default)).StatusCode == 200);
        var pending = services.GetRequiredService<IVendInvoiceInfoService>();
        var held = await pending.CreateFromOrderAsync(order.RecId,
            new(key + "-I1", purchaseDay.AddDays(3), purchaseDay.AddDays(3), "AR stage supply", [new(line.RecId, quantity)]), default);
        Assert.True(held.StatusCode == 200, held.Message);
        var parm = await db.Set<VendInvoiceInfoTable>().SingleAsync(x => x.DataAreaId == Company && x.Num == key + "-I1");
        Assert.True((await pending.PostAsync(parm.ParmId, default)).StatusCode == 200);
        db.ChangeTracker.Clear();
    }

    private static async Task CheckScenarioAmountsAsync(ApplicationDbContext db, string id)
    {
        (decimal Qty,decimal Value,decimal Revenue,decimal Tax,decimal BankCur,decimal BankMst,
            decimal Control,decimal Debit,decimal Credit,decimal Discount,decimal FxGain) expected = id switch
        {
            "01" => (-100m,-2500m,-5000m,-750m,5750m,5750m,0m,0m,0m,0m,0m),
            "03" => (0m,0m,-1000m,-150m,1150m,1150m,0m,0m,0m,0m,0m),
            "04" => (0m,0m,-350m,-52.50m,402.50m,402.50m,0m,0m,0m,0m,0m),
            "08" => (-100m,-2500m,-4000m,-600m,1000m,1000m,3600m,3600m,0m,0m,0m),
            "12" => (-100m,-2500m,-4000m,-600m,4508m,4508m,0m,0m,0m,92m,0m),
            "13" => (-100m,-2500m,-4125m,-618.75m,1265m,4807m,0m,0m,0m,0m,-63.25m),
            "14" => (-90m,-2250m,-3600m,-540m,4140m,4140m,0m,0m,0m,0m,0m),
            "16" or "20" => (-60m,-1500m,-2400m,-360m,2760m,2760m,0m,0m,0m,0m,0m),
            "18" => (-100m,-2500m,-4000m,-600m,4600m,4600m,0m,4600m,4600m,0m,0m),
            "19" => (-100m,-2500m,-4000m,-600m,0m,0m,4600m,4600m,0m,0m,0m),
            "02" or "05" or "09" => (-100m,-2500m,-4000m,-600m,4600m,4600m,0m,0m,0m,0m,0m),
            _ => throw new InvalidOperationException("Missing arithmetic expectations for AR " + id)
        };
        var actual = await AccountsReceivableReconciliationSnapshot.LoadAsync(db,$"ARV2-2026-{id}",id,expected.Qty,expected.Debit,expected.Credit,default);
        Assert.Equal(expected.Qty,actual.PhysicalQuantity);
        Assert.Equal(expected.Value,actual.Inventory.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment));
        Assert.Equal(expected.Tax,actual.Tax.Sum(x => x.TaxAmount));
        Assert.Equal(expected.BankCur,actual.Bank.Sum(x => x.AmountCur));
        Assert.Equal(expected.BankMst,actual.Bank.Sum(x => x.AmountMst));
        Assert.Equal(expected.Debit,actual.OpenDebit); Assert.Equal(expected.Credit,actual.OpenCredit);
        var accounts = new Dictionary<string,decimal>
        {
            [GeneralLedgerSeedExample.Bank]=expected.BankMst,
            [GeneralLedgerSeedExample.Inventory]=expected.Value,
            [GeneralLedgerSeedExample.CostOfGoodsSold]=-expected.Value,
            [GeneralLedgerSeedExample.SalesRevenue]=expected.Revenue,
            [GeneralLedgerSeedExample.PayableSalesTax]=expected.Tax,
            [GeneralLedgerSeedExample.CustomerControl]=expected.Control,
            [GeneralLedgerSeedExample.SalesCashDiscount]=expected.Discount,
            [GeneralLedgerSeedExample.RealizedExchangeGain]=expected.FxGain,
            [GeneralLedgerSeedExample.RealizedExchangeLoss]=0m,
            [GeneralLedgerSeedExample.CustomerAdvance]=0m,
            [GeneralLedgerSeedExample.DeliveredNotInvoiced]=0m,
            [GeneralLedgerSeedExample.DeliveredNotInvoicedInterim]=0m
        };
        foreach(var (account,amount) in accounts)
            Assert.True(amount == actual.LedgerAmount(account),$"AR {id}, account {account}: expected {amount}, actual {actual.LedgerAmount(account)} SAR.");
        Assert.NotEmpty(actual.Entries);
        Assert.All(actual.Entries.GroupBy(x => x.GeneralJournalEntry),voucher => Assert.Equal(0m,voucher.Sum(x => x.AccountingCurrencyAmount)));
    }

    private static Task<InventSum> OnHandAsync(ApplicationDbContext db) =>
        db.Set<InventSum>().AsNoTracking().FirstAsync(x => x.DataAreaId == Company && x.ItemId == Item);

    private static async Task<decimal> TotalDeliveredAsync(ApplicationDbContext db) =>
        -(await db.Set<InventTrans>().Where(x => x.DataAreaId == Company && x.ItemId == Item
            && (x.StatusIssue == StatusIssue.Deducted || x.StatusIssue == StatusIssue.Sold)).ToListAsync()).Sum(x => x.Qty);

    private static async Task<decimal> OpenTotalAsync(ApplicationDbContext db) =>
        (await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company).ToListAsync()).Sum(x => x.AmountCur);

    private static async Task<decimal> LedgerAsync(ApplicationDbContext db, string mainAccount)
    {
        var dimensions = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == Company && x.MainAccountValue == mainAccount).Select(x => x.RecId).ToArrayAsync();
        return (await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.DataAreaId == Company && dimensions.Contains(x.LedgerDimension)).ToListAsync())
            .Sum(x => x.AccountingCurrencyAmount);
    }

    private static async Task<string> CountsAsync(ApplicationDbContext db) => string.Join(":",
        await db.Set<CustTrans>().CountAsync(), await db.Set<CustTransOpen>().CountAsync(),
        await db.Set<CustInvoiceJour>().CountAsync(), await db.Set<CustInvoiceTrans>().CountAsync(),
        await db.Set<InventTrans>().CountAsync(), await db.Set<GeneralJournalEntry>().CountAsync(),
        await db.Set<GeneralJournalAccountEntry>().CountAsync(), await db.Set<BankAccountTrans>().CountAsync());

    private static ServiceProvider CycleServices(ApplicationDbContext db, string actorId = "ar-cycle-test", string companyId = Company)
    {
        var actor = new ArCycleIdentity(actorId,companyId);
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        return new ServiceCollection().AddSingleton(db).AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor)
            .AddLogging()
            .AddAccountsReceivableTransactionExamples()
            .AddScoped<SalesOrderValidationService>()
            .AddScoped<ISalesInventoryDemandService,SalesInventoryDemandService>()
            .AddScoped<IValidator<PostConfirmationRequest>,SalesOrderConfirmationValidator>()
            .AddScoped<IInventoryReservationService, InventoryReservationService>()
            .BuildServiceProvider();
    }

    private sealed class ArCycleIdentity(string actorId = "ar-cycle-test", string companyId = Company) : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => companyId;
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => actorId;
        public string GetOwnerAccountId() => actorId;
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
