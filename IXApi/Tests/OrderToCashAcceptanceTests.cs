using System.Text.Json;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public sealed partial class AccountsReceivableCycleStageTests
{
    [SqlServerPhaseOneFact]
    public Task Order_to_cash_attachment_exact_amounts_and_weighted_average_close_SQLServer() => RunOrderToCashAttachmentAsync(false);

    [SqlServerPhaseOneFact]
    public Task Order_to_cash_attachment_returns_credit_only_and_corrections_SQLServer() => RunOrderToCashAttachmentAsync(true);

    private async Task RunOrderToCashAttachmentAsync(bool corrections)
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = "IXApi_OTCAcceptance_" + Guid.NewGuid().ToString("N"),
            IntegratedSecurity = true, TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var db = new ApplicationDbContext(options);
        var audit = new OtcAudit();
        try
        {
            await db.Database.EnsureCreatedAsync();
            using var provider = CycleServices(db);
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            const string serviceItem = "APV2-SVC";
            long orderId = 0, stockLineId = 0, serviceLineId = 0;
            string inventId = "", salesId = "", firstInvoice = "", secondInvoice = "";
            string firstRequest = "OTC-I1";
            var responses = new List<object>();
            decimal openAfterPayment = 0;

            async Task<object> Evidence()
            {
                db.ChangeTracker.Clear();
                var stock = await db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == Company && x.ItemId == Item)
                    ?? new InventSum();
                var customer = await db.Set<CustTrans>().Where(x => x.DataAreaId == Company && x.AccountNum == Customer).ToListAsync();
                var open = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company && x.AccountNum == Customer).ToListAsync();
                return new
                {
                    orderId, salesId, stockLineId, serviceLineId, firstInvoice, secondInvoice,responses,
                    stock.PhysicalInvent, stock.ReservPhysical, stock.AvailPhysical, stock.PostedValue,
                    customerBalance = customer.Sum(x => x.AmountCur),
                    openDebit = open.Where(x => x.AmountCur > 0).Sum(x => x.AmountCur),
                    openCredit = open.Where(x => x.AmountCur < 0).Sum(x => x.AmountCur),
                    customerControl = await LedgerAsync(db,GeneralLedgerSeedExample.CustomerControl),
                    revenue = await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue),
                    cogs = await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold),
                    inventoryLedger = await LedgerAsync(db,GeneralLedgerSeedExample.Inventory),
                    outputTax = await LedgerAsync(db,GeneralLedgerSeedExample.PayableSalesTax),
                    bank = await LedgerAsync(db,GeneralLedgerSeedExample.Bank),
                    invoices = await db.Set<CustInvoiceJour>().Where(x => x.DataAreaId == Company)
                        .Select(x => new { x.RecId,x.InvoiceId,x.SalesId,x.InvoiceAccount,x.InvoiceAmount,x.LedgerVoucher,x.ParmId }).ToListAsync(),
                    journals = await db.Set<LedgerJournalTable>().Where(x => x.DataAreaId == Company)
                        .Select(x => new {x.RecId,x.JournalNum,x.JournalName,x.Posted}).ToListAsync(),
                    customerTransactions = customer.Select(x => new { x.RecId,x.AccountNum,x.Invoice,x.Voucher,x.AmountCur,x.AmountMst,x.SettleAmountCur }),
                    settlements = await db.Set<CustSettlement>().Where(x => x.DataAreaId == Company)
                        .Select(x => new { x.RecId,x.TransRecId,x.OffsetRecId,x.SettleAmountCur,x.SettlementVoucher }).ToListAsync()
                };
            }
            Task Stage(string[] ids,string name,Func<Task> action) => audit.Stage(ids,name,async () => { await action(); return await Evidence(); });
            async Task AssertOpen(decimal debit,decimal credit)
            {
                var rows = await db.Set<CustTransOpen>().Where(x => x.AccountNum == Customer && x.DataAreaId == Company).ToListAsync();
                Assert.Equal(debit,rows.Where(x => x.AmountCur > 0).Sum(x => x.AmountCur));
                Assert.Equal(credit,rows.Where(x => x.AmountCur < 0).Sum(x => x.AmountCur));
            }
            async Task Invoice(decimal quantity,bool includeService,string request)
            {
                var result = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,
                    new(Date,request,includeService ? [new(stockLineId,quantity),new(serviceLineId,1m)] : [new(stockLineId,quantity)]),default);
                Assert.True(result.StatusCode == 200,result.Message);
            }

            await Stage(["CFG-03","CFG-07","MD-05","MD-06"],"Configuration and master-data policies",async () =>
            {
                await services.GetRequiredService<AccountsReceivableCycleSeeder>().PrepareAsync(new(2026,"otc-acceptance"));
                var policy = await db.Set<InventModelGroup>().SingleAsync(x => x.ModelGroupId == "APV2");
                policy.InventModel = InventModel.WeightedAvg;
                policy.PostPhysicalRevenue = NoYes.Yes;
                policy.NegativePhysical = NoYes.No;
                policy.NegativeFinancial = NoYes.No;
                await db.SaveChangesAsync();
                Assert.Equal(NoYes.No,(await db.Set<InventModelGroup>().SingleAsync(x => x.ModelGroupId == "APV2-SVC")).StockedProduct);
                Assert.True(await db.Set<InventTableModule>().AnyAsync(x => x.ItemId == serviceItem));
            });
            await Stage([],"Opening stock: 100 units at 60",async () =>
            {
                await BuyStockAsync(services,db,100m,60m,"OTC-OPEN",new DateTime(2026,4,1));
                var stock = await OnHandAsync(db);
                Assert.Equal(100m,stock.PhysicalInvent);
                Assert.Equal(6000m,stock.PostedValue);
                Assert.Equal(6000m,await LedgerAsync(db,GeneralLedgerSeedExample.Inventory));
                await AssertOpen(0,0);
            });
            await Stage(["SO-01","SO-02"],"Mixed sales order: 10 at 100 plus service 200",async () =>
            {
                var before = await CountsAsync(db);
                var created = await services.GetRequiredService<ISalesOrderQuickCreateService>().CreateAsync(new SalesOrderQuickCreateDto
                {
                    CustomerAccount = Customer,CurrencyCode = "SAR",SalesName = "OTC source acceptance",
                    InventSiteId = "APV2-SITE",InventLocationId = "APV2-WH",
                    RequestedShipDate = Date,RequestedReceiptDate = Date
                },default);
                Assert.True(created.Error == null && created.Order != null,created.Error);
                orderId = created.Order!.RecId;
                salesId = created.Order.SalesId;
                var lines = services.GetRequiredService<ISalesOrderLineService>();
                foreach(var input in new[] {
                    new SalesLineInputDto { ItemNumber = Item,Quantity = 10,UnitPrice = 100,Unit = "EA",PriceUnit = 1,
                        InventSiteId = "APV2-SITE",InventLocationId = "APV2-WH",DeliveryDate = Date,TaxGroup = "ARV2",TaxItemGroup = "APV2" },
                    new SalesLineInputDto { ItemNumber = serviceItem,Quantity = 1,UnitPrice = 200,Unit = "EA",PriceUnit = 1,
                        DeliveryDate = Date,TaxGroup = "ARV2",TaxItemGroup = "APV2" } })
                {
                    var result = await lines.AddAsync(orderId,input,default);
                    Assert.True(result.Status == SalesLineWriteStatus.Success,result.Error);
                }
                var saved = await db.Set<SalesLine>().Where(x => x.SalesId == salesId).ToListAsync();
                var stock = saved.Single(x => x.ItemId == Item);
                stockLineId = stock.RecId;
                inventId = stock.InventTransId;
                serviceLineId = saved.Single(x => x.ItemId == serviceItem).RecId;
                Assert.Equal(1200m,saved.Sum(x => x.LineAmount));
                Assert.Equal(NoYes.No,saved.Single(x => x.RecId == serviceLineId).StockedProduct);
                Assert.False(await db.Set<InventTrans>().AnyAsync(x => x.ItemId == serviceItem));
                Assert.Equal(100m,(await OnHandAsync(db)).PhysicalInvent);
                Assert.Empty(await db.Set<CustTrans>().ToListAsync());
                Assert.Equal(0m,await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue));
            });
            await Stage(["SO-05"],"Confirm without customer or revenue posting",async () =>
            {
                var before = await CountsAsync(db);
                var result = await services.GetRequiredService<ISalesOrderConfirmationService>().PostAsync(orderId,new(Date,true,"OTC-CONF"),default);
                Assert.Equal(200,result.StatusCode);
                Assert.Equal(before,await CountsAsync(db));
                Assert.Equal(100m,(await OnHandAsync(db)).PhysicalInvent);
            });
            var reservation = services.GetRequiredService<IInventoryReservationService>();
            await Stage(["SYS-06"],"Another company's context cannot invoice this order",async () =>
            {
                var before = await CountsAsync(db);
                await using var foreignDb = new ApplicationDbContext(options,companyContext:new ArCycleIdentity("otc-other-company","oth"));
                using var foreignServices = CycleServices(foreignDb,"otc-other-company","oth");
                var rejected = await foreignServices.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,
                    new(Date,"OTC-FOREIGN",[new(stockLineId,1)]),default);
                responses.Add(new {id="SYS-06",rejected.StatusCode,rejected.Message});
                Assert.Equal(404,rejected.StatusCode);
                Assert.Equal(before,await CountsAsync(db));
            });
            await Stage(["INV-01"],"Reserve 10 without physical or financial issue",async () =>
            {
                var result = await reservation.ReserveAsync(inventId,10,false,default);
                Assert.Null(result.Error);
                var stock = await OnHandAsync(db);
                Assert.Equal(100m,stock.PhysicalInvent); Assert.Equal(10m,stock.ReservPhysical); Assert.Equal(90m,stock.AvailPhysical);
            });
            await Stage(["INV-02"],"Release reservation and reserve again",async () =>
            {
                Assert.Null((await reservation.ReleaseAsync(inventId,10,default)).Error);
                Assert.Equal(100m,(await OnHandAsync(db)).AvailPhysical);
                Assert.Null((await reservation.ReserveAsync(inventId,10,false,default)).Error);
            });
            await Stage(["INV-03"],"Pick 6 without physical issue",async () =>
            {
                var before = await db.Set<GeneralJournalEntry>().CountAsync();
                var result = await reservation.SetPickedAsync(inventId,6,default);
                Assert.Null(result.Error); Assert.Equal(6m,result.Result!.Picked);
                Assert.Equal(100m,(await OnHandAsync(db)).PhysicalInvent);
                Assert.Equal(before,await db.Set<GeneralJournalEntry>().CountAsync());
                Assert.Empty(await db.Set<CustInvoiceJour>().ToListAsync());
                Assert.Empty(await db.Set<CustTrans>().ToListAsync());
            });
            async Task Deliver(decimal quantity,string request)
            {
                var result = await services.GetRequiredService<IPackingSlipJournalService>().PostAsync(orderId,
                    new(Date,"all",true,request,[new(stockLineId.ToString(System.Globalization.CultureInfo.InvariantCulture),quantity)]),default);
                Assert.True(result.StatusCode == 200,result.Message);
            }
            await Stage(["INV-04"],"Deliver 6: on hand 94 and physical cost 360",async () =>
            {
                var stock = await OnHandAsync(db);
                Assert.Equal(6m, stock.Picked);
                db.ChangeTracker.Clear();
                await db.Set<InventSum>().Where(x => x.RecId == stock.RecId)
                    .ExecuteUpdateAsync(rows => rows.SetProperty(x => x.Picked, 0m));

                var rejected = await services.GetRequiredService<IPackingSlipJournalService>().PostAsync(orderId,
                    new(Date,"all",true,"OTC-PS1",[new(stockLineId.ToString(System.Globalization.CultureInfo.InvariantCulture),6m)]),default);
                Assert.Equal(422, rejected.StatusCode);
                Assert.Contains("Inventory summary does not match open demand", rejected.Message);
                Assert.Empty(db.ChangeTracker.Entries<CustPackingSlipJour>());
                Assert.False(await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Company && x.ParmId == "OTC-PS1"));

                stock = await OnHandAsync(db);
                Assert.Equal(100m, stock.PhysicalInvent);
                await db.Set<InventSum>().Where(x => x.RecId == stock.RecId)
                    .ExecuteUpdateAsync(rows => rows.SetProperty(x => x.Picked, 6m));
                await Deliver(6,"OTC-PS1");
                await Deliver(6,"OTC-PS1");
                var conflictingRetry = await services.GetRequiredService<IPackingSlipJournalService>().PostAsync(orderId,
                    new(Date,"all",true,"OTC-PS1",[new(stockLineId.ToString(System.Globalization.CultureInfo.InvariantCulture),5m)]),default);
                Assert.Equal(409, conflictingRetry.StatusCode);
                var conflictingDate = await services.GetRequiredService<IPackingSlipJournalService>().PostAsync(orderId,
                    new(Date.AddDays(1),"all",true,"OTC-PS1",[new(stockLineId.ToString(System.Globalization.CultureInfo.InvariantCulture),6m)]),default);
                Assert.Equal(409, conflictingDate.StatusCode);
                var slip = await db.Set<CustPackingSlipJour>().AsNoTracking()
                    .SingleAsync(x => x.DataAreaId == Company && x.ParmId == "OTC-PS1");
                Assert.Equal(slip.PackingSlipId + ".1", slip.InternalPackingSlipId);
                Assert.Equal(6m, slip.Qty);
                var version = await db.Set<CustPackingSlipVersion>().AsNoTracking()
                    .SingleAsync(x => x.CustPackingSlipJour == slip.RecId);
                Assert.Equal(slip.InternalPackingSlipId, version.InternalPackingSlipId);
                Assert.Equal(slip.LedgerVoucher, version.LedgerVoucher);
                Assert.Equal(slip.Qty, version.Qty);
                Assert.Equal(slip.ParmId, version.ParmId);
                Assert.Equal(Date, version.AccountingDate);
                var slipLine = await db.Set<CustPackingSlipTrans>().AsNoTracking()
                    .SingleAsync(x => x.DataAreaId == Company && x.ParentRecId == slip.RecId);
                var history = await db.Set<CustPackingSlipTransHistory>().AsNoTracking()
                    .SingleAsync(x => x.DataAreaId == Company && x.CustPackingSlipTrans == slipLine.RecId);
                Assert.Equal(await db.Set<SalesLine>().Where(x => x.RecId == stockLineId)
                    .Select(x => x.PriceUnit).SingleAsync(), slipLine.PriceUnit);
                Assert.Equal(6m, history.Qty);
                Assert.Equal(6m, history.InventQty);
                Assert.Equal(4m, history.Remain);
                Assert.Equal(slipLine.ValueMst, history.ValueMst);
                Assert.Equal(version.VersionDateTime, history.ValidFrom);
                Assert.Equal(new DateTime(2154, 12, 31, 23, 59, 59), history.ValidTo);
                Assert.False(string.IsNullOrWhiteSpace(slip.LedgerVoucher));
                Assert.True(await db.Set<SubledgerVoucherGeneralJournalEntry>().AnyAsync(x =>
                    x.VoucherDataAreaId == Company && x.Voucher == slip.LedgerVoucher && x.AccountingDate == Date));
                Assert.Equal(94m,(await OnHandAsync(db)).PhysicalInvent);
                var line = await db.Set<SalesLine>().SingleAsync(x => x.RecId == stockLineId);
                Assert.Equal(4m,line.RemainSalesPhysical); Assert.Equal(10m,line.RemainSalesFinancial);
                Assert.Equal(-360m,await LedgerAsync(db,GeneralLedgerSeedExample.DeliveredNotInvoicedInterim));
                Assert.Equal(360m,await LedgerAsync(db,GeneralLedgerSeedExample.DeliveredNotInvoiced));
                Assert.Empty(await db.Set<CustTrans>().ToListAsync());
                var origin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == inventId && x.DataAreaId == Company);
                var delivered = await db.Set<InventTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == Company && x.InventTransOrigin == origin.RecId && x.StatusIssue == StatusIssue.Deducted)
                    .ToListAsync();
                Assert.Equal(-6m, delivered.Sum(x => x.Qty));
                Assert.All(delivered, x => Assert.Equal(slip.LedgerVoucher, x.VoucherPhysical));
            });
            await Stage(["INV-07"],"Cancel an uninvoiced delivery, reverse its cost, and deliver again",async () =>
            {
                var journal = services.GetRequiredService<IPackingSlipJournalService>();
                var slip = await db.Set<CustPackingSlipJour>().AsNoTracking()
                    .SingleAsync(x => x.DataAreaId == Company && x.ParmId == "OTC-PS1");
                Assert.Equal(200, (await journal.CancelAsync(orderId, slip.RecId, Date, default)).StatusCode);
                Assert.Equal(200, (await journal.CancelAsync(orderId, slip.RecId, Date, default)).StatusCode);
                var canceledRetry = await journal.PostAsync(orderId,
                    new(Date, "all", true, "OTC-PS1", [new(stockLineId.ToString(System.Globalization.CultureInfo.InvariantCulture), 6m)]), default);
                Assert.Equal(409, canceledRetry.StatusCode);
                Assert.Equal(100m, (await OnHandAsync(db)).PhysicalInvent);
                Assert.Equal(0m, await LedgerAsync(db, GeneralLedgerSeedExample.DeliveredNotInvoicedInterim));
                Assert.Equal(0m, await LedgerAsync(db, GeneralLedgerSeedExample.DeliveredNotInvoiced));
                Assert.Equal(10m, await db.Set<SalesLine>().Where(x => x.RecId == stockLineId)
                    .Select(x => x.RemainSalesPhysical).SingleAsync());
                var versions = await db.Set<CustPackingSlipVersion>().AsNoTracking()
                    .Where(x => x.CustPackingSlipJour == slip.RecId).OrderBy(x => x.RecId).ToListAsync();
                Assert.Equal(new[] { 6m, 0m }, versions.Select(x => x.Qty));
                var lineId = await db.Set<CustPackingSlipTrans>().Where(x => x.ParentRecId == slip.RecId)
                    .Select(x => x.RecId).SingleAsync();
                Assert.Equal(versions[1].VersionDateTime, await db.Set<CustPackingSlipTransHistory>()
                    .Where(x => x.CustPackingSlipTrans == lineId).Select(x => x.ValidTo).SingleAsync());
                Assert.Null((await reservation.ReserveAsync(inventId, 10, false, default)).Error);
                Assert.Null((await reservation.SetPickedAsync(inventId, 6, default)).Error);
                await Deliver(6, "OTC-PS1-REPOST");
                Assert.Equal(94m, (await OnHandAsync(db)).PhysicalInvent);
            });
            await Stage(["BILL-06"],"Reject a repeated sales line in one invoice request",async () =>
            {
                var before = await CountsAsync(db);
                foreach(var (lineId,quantity) in new[] { (stockLineId,6m),(serviceLineId,1m) })
                {
                    var result = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,
                        new(Date,"OTC-DUP-" + lineId,[new(lineId,quantity),new(lineId,quantity)]),default);
                    responses.Add(new { id="BILL-06",lineId,result.StatusCode,result.Message });
                    Assert.Equal(422,result.StatusCode);
                    Assert.Equal(before,await CountsAsync(db));
                }
                Assert.Equal(94m,(await OnHandAsync(db)).PhysicalInvent);
            });
            await Stage(["MD-04"],"An invoicing-blocked customer cannot be billed",async () =>
            {
                var customer = await db.Set<CustTable>().SingleAsync(x => x.AccountNum == Customer);
                customer.Blocked = CustVendorBlocked.Invoicing;
                await db.SaveChangesAsync();
                var before = await CountsAsync(db);
                var result = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,new(Date,"OTC-BLOCKED",[new(stockLineId,6)]),default);
                responses.Add(new { id="MD-04",result.StatusCode,result.Message });
                Assert.Equal(422,result.StatusCode);
                Assert.Equal(before,await CountsAsync(db));
                db.ChangeTracker.Clear();
                customer = await db.Set<CustTable>().SingleAsync(x => x.AccountNum == Customer);
                Assert.Equal(CustVendorBlocked.Invoicing,customer.Blocked);
                customer.Blocked = CustVendorBlocked.No;
                await db.SaveChangesAsync();
            });
            await Stage(["SYS-02"],"Late tax-account failure rolls back allocated number, stock and voucher",async () =>
            {
                var taxAccount = await db.Set<TaxLedgerAccountGroup>().SingleAsync(x => x.TaxAccountGroup == "ARV2");
                var original = taxAccount.TaxOutgoingLedgerDimension;
                taxAccount.TaxOutgoingLedgerDimension = null;
                await db.SaveChangesAsync();
                var before = await CountsAsync(db);
                var counter = await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().Where(x => x.NumberSequence == "CustInvoiceJour").Select(x => x.NextRec).SingleAsync();
                var postingCount = await db.Set<InventTransPosting>().CountAsync();
                var result = await services.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,new(Date,"OTC-LATE-FAIL",[new(stockLineId,6)]),default);
                responses.Add(new { id="SYS-02",result.StatusCode,result.Message });
                Assert.Equal(422,result.StatusCode);
                Assert.Equal(before,await CountsAsync(db));
                Assert.Equal(postingCount,await db.Set<InventTransPosting>().CountAsync());
                Assert.Equal(counter,await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().Where(x => x.NumberSequence == "CustInvoiceJour").Select(x => x.NextRec).SingleAsync());
                Assert.Equal(10m,(await db.Set<SalesLine>().SingleAsync(x => x.RecId == stockLineId)).RemainSalesFinancial);
                db.ChangeTracker.Clear();
                taxAccount = await db.Set<TaxLedgerAccountGroup>().SingleAsync(x => x.TaxAccountGroup == "ARV2");
                taxAccount.TaxOutgoingLedgerDimension = original;
                await db.SaveChangesAsync();
            });
            await Stage(["BILL-01","SYS-03"],"Two users invoice the same 6: exactly one 690 invoice, cost 360",async () =>
            {
                await using var firstDb = new ApplicationDbContext(options);
                await using var secondDb = new ApplicationDbContext(options);
                using var firstServices = CycleServices(firstDb,"otc-user-a");
                using var secondServices = CycleServices(secondDb,"otc-user-b");
                var results = await Task.WhenAll(
                    firstServices.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,new(Date,"OTC-I1A",[new(stockLineId,6)]),default),
                    secondServices.GetRequiredService<ISalesInvoiceService>().PostAsync(orderId,new(Date,"OTC-I1B",[new(stockLineId,6)]),default));
                Assert.Equal(new[] {200,422},results.Select(x => x.StatusCode).OrderBy(x => x).ToArray());
                firstRequest = results[0].StatusCode == 200 ? "OTC-I1A" : "OTC-I1B";
                db.ChangeTracker.Clear();
                var invoice = await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == firstRequest);
                firstInvoice = invoice.InvoiceId;
                Assert.Equal(690m,invoice.InvoiceAmount); Assert.Equal(90m,invoice.SumTax);
                Assert.Equal(360m,await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold));
                Assert.Equal(0m,await LedgerAsync(db,GeneralLedgerSeedExample.DeliveredNotInvoicedInterim));
                Assert.Equal(94m,(await OnHandAsync(db)).PhysicalInvent);
                await AssertOpen(690,0);
            });
            await Stage(["SYS-01"],"Repeat the same invoice request without duplicate posting",async () =>
            {
                var before = await CountsAsync(db);
                await Invoice(6,false,firstRequest);
                Assert.Equal(before,await CountsAsync(db));
            });
            await Stage(["INV-06"],"Reject delivery of 5 when only 4 remain",async () =>
            {
                var before = await CountsAsync(db);
                var result = await services.GetRequiredService<IPackingSlipJournalService>().PostAsync(orderId,
                    new(Date,"all",true,"OTC-EXCESS-DELIVERY",[new(stockLineId.ToString(System.Globalization.CultureInfo.InvariantCulture),5)]),default);
                responses.Add(new { id="INV-06",result.StatusCode,result.Message });
                Assert.Equal(422,result.StatusCode);
                Assert.Equal(before,await CountsAsync(db));
                Assert.Equal(94m,(await OnHandAsync(db)).PhysicalInvent);
            });
            await Stage(["INV-05"],"Pick and deliver the remaining 4",async () =>
            {
                Assert.Null((await reservation.SetPickedAsync(inventId,4,default)).Error);
                await Deliver(4,"OTC-PS2");
                Assert.Equal(90m,(await OnHandAsync(db)).PhysicalInvent);
                Assert.Equal(0m,(await db.Set<SalesLine>().SingleAsync(x => x.RecId == stockLineId)).RemainSalesPhysical);
            });
            await Stage(["BILL-02","BILL-05","CFG-04"],"Invoice 4 plus service: 690; order invoiced but debt open",async () =>
            {
                await Invoice(4,true,"OTC-I2");
                var invoice = await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "OTC-I2");
                secondInvoice = invoice.InvoiceId;
                Assert.NotEqual(firstInvoice,secondInvoice);
                Assert.Equal(690m,invoice.InvoiceAmount);
                Assert.Equal(SalesStatus.Invoiced,(await db.Set<SalesTable>().SingleAsync(x => x.RecId == orderId)).SalesStatus);
                Assert.Equal(90m,(await OnHandAsync(db)).PhysicalInvent);
                Assert.Equal(5400m,(await OnHandAsync(db)).PostedValue);
                Assert.Equal(600m,await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold));
                Assert.Equal(-1200m,await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue));
                Assert.Equal(-180m,await LedgerAsync(db,GeneralLedgerSeedExample.PayableSalesTax));
                Assert.False(await db.Set<InventTrans>().AnyAsync(x => x.ItemId == serviceItem));
                await AssertOpen(1380,0);
            });
            var payment = services.GetRequiredService<ICustPaymentService>();
            await Stage(["PAY-01"],"Save an unposted customer payment journal without bank or customer effects",async () =>
            {
                var before = await CountsAsync(db);
                var work = services.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.Repositories.IUnitOfWork>();
                var headers = new IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.LedgerJournalTableService(
                    work,new ArCycleIdentity(),services.GetRequiredService<IAX.IXApi.Modules.Administration.NumberSequences.ISysNumberSequenceService>());
                var details = new IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.LedgerJournalTransService(work,new ArCycleIdentity());
                var header = await headers.AddAsync(new() {JournalName="ARV2-PAY",Name="OTC draft receipt",CurrencyCode="SAR"});
                var bank = await db.Set<BankAccountTable>().SingleAsync(x => x.AccountId == Bank);
                await details.AddAsync(new() {JournalNum=header.JournalNum,TransDate=Date.AddDays(1),AccountType=LedgerJournalACType.Cust,
                    LedgerDimensionName=Customer,AmountCurCredit=700,CurrencyCode="SAR",OffsetAccountType=LedgerJournalACType.Bank,
                    OffsetLedgerDimension=bank.LedgerDimension,PaymMode="ARV2",PaymReference="OTC-DRAFT"});
                Assert.Equal(NoYes.No,(await db.Set<LedgerJournalTable>().SingleAsync(x => x.RecId == header.RecId)).Posted);
                Assert.Equal(before,await CountsAsync(db));
                Assert.Equal(0m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                await AssertOpen(1380,0);
            });
            var statements = new CustTransService(
                services.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.Repositories.IUnitOfWork>(),
                new ArCycleIdentity());
            CustStatementDto? beforeSettlement = null;
            await Stage(["PAY-02","PAY-03"],"Post 700 unallocated: invoices 1380, payment -700",async () =>
            {
                var result = await payment.PostAsync(new(Customer,Bank,700,Date.AddDays(1),"ARV2",null,"OTC-P1",false),default);
                Assert.True(result.StatusCode == 200,result.Message);
                await AssertOpen(1380,-700);
                Assert.Equal(680m,await LedgerAsync(db,GeneralLedgerSeedExample.CustomerControl));
                Assert.Equal(700m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                openAfterPayment = 680;
                beforeSettlement = await statements.GetStatementAsync(Customer, Date, Date.AddDays(1), default);
            });
            async Task SettleOpen()
            {
                var ids = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company && x.AccountNum == Customer).Select(x => x.RefRecId).ToListAsync();
                var result = await services.GetRequiredService<ICustSettleOpenTransactionsService>().SettleAsync(new(Customer,Date.AddDays(2),ids),default);
                Assert.True(result.StatusCode == 200,result.Message);
            }
            if (!corrections)
            {
                await Stage(["SYS-04"],"A payment cannot bypass the manual settlement lock",async () =>
                {
                    var before = await CountsAsync(db);
                    await using var lockingDb = new ApplicationDbContext(options);
                    await using var held = await lockingDb.Database.BeginTransactionAsync();
                    await new SubledgerPostingSupport(lockingDb).LockAsync(
                        $"CustSettleOpen:{Company}:{Customer}".ToUpperInvariant(), default);
                    var blocked = await payment.PostAsync(new(Customer,Bank,50,Date.AddDays(2),"ARV2",null,"OTC-BLOCKED-LOCK",true),default);
                    Assert.Equal(422, blocked.StatusCode);
                    Assert.Contains("concurrency lock", blocked.Message);
                    Assert.Equal(before, await CountsAsync(db));
                    await held.RollbackAsync();
                });
            }
            await Stage(["SET-01","SET-03","SYS-04"],"Concurrent settlement: allocate 700 once as 690 plus 10",async () =>
            {
                var ledgerCount = await db.Set<GeneralJournalEntry>().CountAsync();
                var ids = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Company && x.AccountNum == Customer).Select(x => x.RefRecId).ToListAsync();
                await using var firstDb = new ApplicationDbContext(options);
                await using var secondDb = new ApplicationDbContext(options);
                using var firstServices = CycleServices(firstDb,"otc-user-a");
                using var secondServices = CycleServices(secondDb,"otc-user-b");
                var results = await Task.WhenAll(
                    firstServices.GetRequiredService<ICustSettleOpenTransactionsService>().SettleAsync(new(Customer,Date.AddDays(2),ids),default),
                    secondServices.GetRequiredService<ICustSettleOpenTransactionsService>().SettleAsync(new(Customer,Date.AddDays(2),ids),default));
                Assert.Equal(new[] {200,422},results.Select(x => x.StatusCode).OrderBy(x => x).ToArray());
                db.ChangeTracker.Clear();
                await AssertOpen(680,0);
                var remaining = await db.Set<CustTransOpen>().SingleAsync(x => x.AccountNum == Customer);
                var trans = await db.Set<CustTrans>().SingleAsync(x => x.RecId == remaining.RefRecId);
                Assert.Equal(secondInvoice,trans.Invoice);
                Assert.Equal(openAfterPayment,await LedgerAsync(db,GeneralLedgerSeedExample.CustomerControl));
                Assert.Equal(ledgerCount,await db.Set<GeneralJournalEntry>().CountAsync());
            });
            await Stage(["SET-02","SET-04","PAY-07","CFG-05"],"Post and allocate 680; preserve transaction and settlement history",async () =>
            {
                var result = await payment.PostAsync(new(Customer,Bank,680,Date.AddDays(3),"ARV2",null,"OTC-P2",false),default);
                Assert.Equal(200,result.StatusCode);
                await SettleOpen();
                await AssertOpen(0,0);
                Assert.Equal(0m,await LedgerAsync(db,GeneralLedgerSeedExample.CustomerControl));
                Assert.Equal(1380m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                Assert.Equal(4,await db.Set<CustTrans>().CountAsync(x => x.AccountNum == Customer));
                Assert.NotEmpty(await db.Set<CustSettlement>().ToListAsync());
                Assert.Equal(2,await db.Set<LedgerJournalTrans>().Where(x => x.PaymReference == "OTC-P1" || x.PaymReference == "OTC-P2").Select(x => x.Voucher).Distinct().CountAsync());
            });
            await audit.Stage(["COL-02"],"Customer statement retains invoices and payments after settlement",async () =>
            {
                var historical = await statements.GetStatementAsync(Customer, Date, Date.AddDays(1), default);
                Assert.Equal(680m, beforeSettlement!.ClosingBalanceMst);
                Assert.Equal(beforeSettlement.ClosingBalanceMst, historical.ClosingBalanceMst);
                Assert.Equal(beforeSettlement.Lines.ToArray(), historical.Lines.ToArray());
                Assert.Equal(new[] { 690m, 690m, -700m }, historical.Lines.Select(x => x.AmountMst).ToArray());
                Assert.All(historical.Lines, line => Assert.False(string.IsNullOrWhiteSpace(line.Voucher)));

                var payments = await statements.GetStatementAsync(Customer, Date.AddDays(1), Date.AddDays(3), default);
                Assert.Equal(1380m, payments.OpeningBalanceMst);
                Assert.Equal(new[] { 680m, 0m }, payments.Lines.Select(x => x.RunningBalanceMst).ToArray());
                Assert.Equal(0m, payments.ClosingBalanceMst);
                return new { beforeSettlement, historical, payments };
            });
            var invoiceDueDate = await db.Set<CustTrans>().Where(x => x.AccountNum == Customer && x.Invoice == firstInvoice)
                .Select(x => x.DueDate).SingleAsync();
            await audit.Stage(["COL-01"],"Customer aging reconstructs due-date buckets at earlier cutoffs",async () =>
            {
                var original = await statements.GetAgingAsync(Customer, Date, invoiceDueDate.AddDays(1), default);
                var partial = await statements.GetAgingAsync(Customer, Date.AddDays(2), invoiceDueDate.AddDays(31), default);
                var closed = await statements.GetAgingAsync(Customer, Date.AddDays(3), invoiceDueDate.AddDays(91), default);
                Assert.Equal(new[] { 0m, 1380m, 0m, 0m, 0m }, original.Buckets.Select(x => x.BalanceMst).ToArray());
                Assert.Equal(new[] { 0m, 0m, 680m, 0m, 0m }, partial.Buckets.Select(x => x.BalanceMst).ToArray());
                Assert.All(closed.Buckets, bucket => Assert.Equal(0m, bucket.BalanceMst));
                return new { original, partial, closed };
            });
            await Stage(["REC-01","REC-03"],"Reconcile customer, tax, inventory, source vouchers and balanced GL",async () =>
            {
                var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
                foreach(var voucher in entries.GroupBy(x => x.GeneralJournalEntry)) Assert.Equal(0m,voucher.Sum(x => x.AccountingCurrencyAmount));
                Assert.Equal(5400m,await LedgerAsync(db,GeneralLedgerSeedExample.Inventory));
                var invoices = await db.Set<CustInvoiceJour>().Where(x => x.SalesId == salesId).ToListAsync();
                foreach(var invoice in invoices)
                {
                    Assert.True(await db.Set<SubledgerVoucherGeneralJournalEntry>().AnyAsync(x => x.Voucher == invoice.LedgerVoucher && x.VoucherDataAreaId == Company));
                    Assert.True(await db.Set<CustTrans>().AnyAsync(x => x.Invoice == invoice.InvoiceId && x.AccountNum == Customer));
                }
                Assert.Equal(-180m,await db.Set<TaxTrans>().Where(x => x.TaxCode == "ARV2-15").SumAsync(x => x.TaxAmount));
            });
            if (!corrections)
            {
                await audit.Stage(["REC-02"],"Reconcile both customer receipts to the bank statement exactly once",async () =>
                {
                    var retryOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlServer(connection.ConnectionString, sql => sql.EnableRetryOnFailure()).Options;
                    await using var reconciliationDb = new ApplicationDbContext(retryOptions);
                    using var reconciliationWork = new IAX.IXApi.Infrastructure.Persistence.Repositories.UnitOfWork(reconciliationDb);
                    var service = new IAX.IXApi.Modules.Finance.GeneralLedger.Bank.BankAccountStatementService(
                        reconciliationWork, new ArCycleIdentity());
                    var movements = await db.Set<BankAccountTrans>().AsNoTracking()
                        .Where(x => x.DataAreaId == Company && x.AccountId == Bank
                            && (x.PaymReference == "OTC-P1" || x.PaymReference == "OTC-P2"))
                        .OrderBy(x => x.TransDate).ToListAsync();
                    Assert.Equal(new[] { 700m, 680m }, movements.Select(x => x.BankTransAmountCur).ToArray());
                    var ids = movements.Select(x => x.RecId).ToArray();
                    var ledgerCount = await db.Set<GeneralJournalEntry>().CountAsync();
                    var date = Date.AddDays(3);

                    var missing = await service.ReconcileAsync(Bank, "OTC-STMT", date, 1380m, [ids[0]], default);
                    Assert.Null(missing.Statement);
                    Assert.Contains("differs", missing.Error);
                    var duplicated = await service.ReconcileAsync(Bank, "OTC-STMT", date, 1380m, [ids[0], ids[0]], default);
                    Assert.Null(duplicated.Statement);
                    Assert.Contains("distinct", duplicated.Error);
                    Assert.False(await db.Set<BankAccountStatement>().AnyAsync(x => x.AccountStatementNum == "OTC-STMT"));
                    Assert.False(await db.Set<BankAccountTrans>().AnyAsync(x => ids.Contains(x.RecId) && x.Reconciled == NoYes.Yes));

                    var matched = await service.ReconcileAsync(Bank, "OTC-STMT", date, 1380m, ids, default);
                    Assert.True(matched.Statement != null, matched.Error);
                    var detail = await service.GetReconciliationAsync(matched.Statement!.RecId, default);
                    Assert.NotNull(detail);
                    Assert.Equal(ids, detail.Movements.Select(x => x.RecId).ToArray());
                    Assert.Equal(1380m, detail.Movements.Sum(x => x.Amount));
                    var repeat = await service.ReconcileAsync(Bank, "OTC-STMT", date, 1380m, ids, default);
                    Assert.Null(repeat.Statement);
                    var reuse = await service.ReconcileAsync(Bank, "OTC-STMT-2", date.AddDays(1), 2760m, ids, default);
                    Assert.Null(reuse.Statement);
                    Assert.Contains("already reconciled", reuse.Error);
                    Assert.Equal(ledgerCount, await db.Set<GeneralJournalEntry>().CountAsync());
                    Assert.Equal(1380m, await LedgerAsync(db, GeneralLedgerSeedExample.Bank));
                    return detail;
                });
            }
            if(corrections)
            {
                await audit.Stage(["PAY-09"],"Refund an unallocated 165 credit and settle the bank disbursement",async () =>
                {
                    var bankBefore = await LedgerAsync(db, GeneralLedgerSeedExample.Bank);
                    var receipt = await payment.PostAsync(new(Customer, Bank, 165, Date.AddDays(4), "ARV2",
                        null, "OTC-REFUND-CREDIT", false), default);
                    Assert.True(receipt.StatusCode == 200, receipt.Message);
                    await AssertOpen(0, -165);
                    var credit = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.AccountNum == Customer);
                    var request = new CustRefundPostDto(Customer, Bank, "ARV2-PAY", credit.RefRecId, Date.AddDays(4), "OTC-REFUND");
                    var before = await CountsAsync(db);
                    var refused = await payment.RefundAsync(request with { BankAccountId = "NO-BANK" }, default);
                    Assert.Equal(422, refused.StatusCode);
                    Assert.Equal(before, await CountsAsync(db));
                    var posted = await payment.RefundAsync(request, default);
                    Assert.True(posted.StatusCode == 200, posted.Message);
                    await AssertOpen(0, 0);
                    Assert.Equal(bankBefore, await LedgerAsync(db, GeneralLedgerSeedExample.Bank));
                    Assert.Equal(0m, await LedgerAsync(db, GeneralLedgerSeedExample.CustomerControl));
                    var line = await db.Set<LedgerJournalTrans>().AsNoTracking().SingleAsync(x => x.PaymReference == request.RequestId);
                    var bank = await db.Set<BankAccountTrans>().AsNoTracking().SingleAsync(x => x.Voucher == line.Voucher);
                    Assert.Equal(-165m, bank.AmountMst);
                    var settlements = await db.Set<CustSettlement>().AsNoTracking()
                        .Where(x => x.TransRecId == line.CustTransId && x.OffsetRecId == credit.RefRecId).ToListAsync();
                    Assert.Equal(165m, Assert.Single(settlements).SettleAmountMst);
                    var after = await CountsAsync(db);
                    Assert.Equal(200, (await payment.RefundAsync(request, default)).StatusCode);
                    Assert.Equal(422, (await payment.RefundAsync(request with { RequestId = "OTC-REFUND-AGAIN" }, default)).StatusCode);
                    Assert.Equal(after, await CountsAsync(db));
                    return new { line.JournalNum, line.Voucher, bank.AmountMst, credit.RefRecId, settlements };
                });
                await Stage(["SET-07"],"Undo the 680 settlement without reversing bank",async () =>
                {
                    var paymentTrans = await db.Set<CustTrans>().SingleAsync(x => x.AccountNum == Customer && x.PaymReference == "OTC-P2");
                    var voucher = await db.Set<CustSettlement>().Where(x => x.TransRecId == paymentTrans.RecId)
                        .Select(x => x.SettlementVoucher).Distinct().SingleAsync();
                    var result = await services.GetRequiredService<ICustSettlementReversalService>().ReverseAsync(voucher,Date.AddDays(4),default);
                    Assert.True(result.StatusCode == 200,result.Message);
                    await AssertOpen(680,-680);
                    Assert.Equal(1380m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                    var beforeUndo = await statements.GetAgingAsync(Customer, Date.AddDays(3), invoiceDueDate, default);
                    var afterUndo = await statements.GetAgingAsync(Customer, Date.AddDays(4), invoiceDueDate, default);
                    Assert.All(beforeUndo.Buckets, bucket => Assert.Equal(0m, bucket.BalanceMst));
                    Assert.Equal(680m, afterUndo.Buckets[0].BalanceMst);
                    Assert.Equal(-680m, afterUndo.Buckets.Skip(1).Sum(bucket => bucket.BalanceMst));
                });
                await Stage(["PAY-08"],"Reverse the second payment: bank 700 and invoice debt 680",async () =>
                {
                    var journal = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.PaymReference == "OTC-P2");
                    var result = await services.GetRequiredService<ICustPaymentReversalService>().ReverseAsync(journal.JournalNum,Date.AddDays(5),default);
                    Assert.True(result.StatusCode == 200,result.Message);
                    await AssertOpen(680,0);
                    Assert.Equal(700m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                    Assert.Equal(680m,await LedgerAsync(db,GeneralLedgerSeedExample.CustomerControl));
                });
                var returns = services.GetRequiredService<ICustReturnService>();
                var sold = await db.Set<CustInvoiceTrans>().SingleAsync(x => x.InvoiceId == firstInvoice && x.ItemId == Item);
                await Stage(["RET-01","RET-02"],"Return 2 at original cost 60: credit 230 and stock value +120",async () =>
                {
                    var created = await returns.CreateAsync(new(firstInvoice,"ARV2-WRONG",[new(sold.RecId,2,"ARV2-RET")]),default);
                    Assert.True(created.StatusCode == 200,created.Message);
                    var order = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
                    var line = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == order.SalesId);
                    Assert.Equal(sold.InventTransId,line.InventTransIdReturn);
                    var received = await returns.ReceiveAsync(order.RecId,new("OTC-RR",Date.AddDays(5),[new(line.RecId,2)]),default);
                    Assert.True(received.StatusCode == 200,received.Message);
                    var credited = await returns.CreditAsync(order.RecId,new(Date.AddDays(6),"OTC-CR"),default);
                    Assert.True(credited.StatusCode == 200,credited.Message);
                    Assert.Equal(-230m,(await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "OTC-CR")).InvoiceAmount);
                    Assert.Equal(92m,(await OnHandAsync(db)).PhysicalInvent);
                    Assert.Equal(5520m,(await OnHandAsync(db)).PostedValue);
                    Assert.Equal(480m,await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold));
                    await AssertOpen(680,-230);
                });
                await Stage(["RET-04"],"Apply the physical-return credit to the open invoice",async () =>
                {
                    await SettleOpen();
                    await AssertOpen(450,0);
                    Assert.Equal(700m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                });
                await Stage(["RET-03"],"Credit only 100 plus tax 15 without stock or cost movement",async () =>
                {
                    db.Add(new ReturnDispositionCode { DataAreaId = Company,DispositionCodeId = "OTC-CREDIT",Description = "Credit without return",DispositionAction = DispositionAction.CreditOnly });
                    await db.SaveChangesAsync();
                    var beforeQty = (await OnHandAsync(db)).PhysicalInvent;
                    var beforeValue = (await OnHandAsync(db)).PostedValue;
                    var transCount = await db.Set<InventTrans>().CountAsync();
                    var created = await returns.CreateAsync(new(firstInvoice,null,[new(sold.RecId,1,"OTC-CREDIT")]),default);
                    Assert.True(created.StatusCode == 200,created.Message);
                    var line = await db.Set<SalesLine>().SingleAsync(x => x.ReturnDispositionCodeId == "OTC-CREDIT");
                    var order = await db.Set<SalesTable>().SingleAsync(x => x.SalesId == line.SalesId);
                    var credited = await returns.CreditAsync(order.RecId,new(Date.AddDays(7),"OTC-CREDIT-ONLY"),default);
                    Assert.True(credited.StatusCode == 200,credited.Message);
                    Assert.Equal(-115m,(await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "OTC-CREDIT-ONLY")).InvoiceAmount);
                    Assert.Equal(beforeQty,(await OnHandAsync(db)).PhysicalInvent);
                    Assert.Equal(beforeValue,(await OnHandAsync(db)).PostedValue);
                    Assert.Equal(transCount,await db.Set<InventTrans>().CountAsync());
                    Assert.Equal(480m,await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold));
                });
                await Stage(["BILL-08","MD-02"],"Free text 300 plus 45: debt goes to a different invoice account, no inventory",async () =>
                {
                    var inventoryCount = await db.Set<InventTrans>().CountAsync();
                    var revenue = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.MainAccountValue == GeneralLedgerSeedExample.SalesRevenue);
                    var freeText = services.GetRequiredService<IFreeTextInvoiceService>();
                    var created = await freeText.CreateAsync(new() {CustomerAccount=Customer,InvoiceAccount="ARV2-dat-USD",InvoiceDate=Date.AddDays(8),CurrencyCode="SAR",TaxGroup="ARV2",
                        Description="OTC consulting",Lines=[new() {Description="Consulting",Quantity=1,UnitPrice=300,LedgerDimension=revenue.RecId,TaxGroup="ARV2",TaxItemGroup="APV2"}]},default);
                    Assert.True(created.StatusCode is 200 or 201,created.Message);
                    var posted = await freeText.PostAsync(created.Invoice!.RecId,default);
                    Assert.True(posted.StatusCode == 200,posted.Message);
                    Assert.Equal(345m,posted.Invoice!.InvoiceAmount);
                    var debt = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == posted.Invoice.InvoiceId);
                    Assert.Equal("ARV2-dat-USD",debt.AccountNum);
                    Assert.Equal(345m,debt.AmountCur);
                    Assert.Equal(inventoryCount,await db.Set<InventTrans>().CountAsync());
                });
                await Stage(["PAY-06","SET-06"],"Overpayment with automatic allocation leaves the correct customer credit",async () =>
                {
                    // After the reversed 680 payment, return 230 and credit-only 115, net debt is 335.
                    var posted = await payment.PostAsync(new(Customer,Bank,500,Date.AddDays(9),"ARV2",null,"OTC-OVERPAY",true),default);
                    Assert.True(posted.StatusCode == 200,posted.Message);
                    await AssertOpen(0,-165);
                    Assert.Equal(1200m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
                });
                await Stage(["SYS-07"],"Fractional-price rounding agrees across free-text line, header and receivable",async () =>
                {
                    var revenue = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.MainAccountValue == GeneralLedgerSeedExample.SalesRevenue);
                    var freeText = services.GetRequiredService<IFreeTextInvoiceService>();
                    var created = await freeText.CreateAsync(new() {CustomerAccount="ARV2-dat-USD",InvoiceDate=Date.AddDays(9),CurrencyCode="SAR",TaxGroup="ARV2",
                        Description="OTC rounding",Lines=[new() {Description="Fractional service",Quantity=3,UnitPrice=0.335m,LedgerDimension=revenue.RecId,TaxGroup="ARV2",TaxItemGroup="APV2"}]},default);
                    Assert.True(created.StatusCode is 200 or 201,created.Message);
                    var posted = await freeText.PostAsync(created.Invoice!.RecId,default);
                    Assert.True(posted.StatusCode == 200,posted.Message);
                    Assert.Equal(1.01m,posted.Invoice!.NetAmount);
                    Assert.Equal(0.15m,posted.Invoice.TaxAmount);
                    Assert.Equal(1.16m,posted.Invoice.InvoiceAmount);
                    var debt = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == posted.Invoice.InvoiceId);
                    Assert.Equal(1.16m,debt.AmountCur);
                    var voucher = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry ==
                        db.Set<SubledgerVoucherGeneralJournalEntry>().Where(y => y.Voucher == debt.Voucher && y.VoucherDataAreaId == Company).Select(y => y.GeneralJournalEntry).Single()).ToListAsync();
                    Assert.Equal(0m,voucher.Sum(x => x.AccountingCurrencyAmount));
                });
            }
            else
            await Stage(["CLOSE-01"],"Receive 100 at 80 and close weighted average: COGS 700, inventory 13300",async () =>
            {
                await BuyStockAsync(services,db,100,80,"OTC-LATER",new DateTime(2026,4,20));
                var close = new InventCloseService(services.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.Repositories.IUnitOfWork>(),new ArCycleIdentity(),new ArCycleIdentity(),services.GetRequiredService<IAX.IXApi.Modules.Administration.NumberSequences.ISysNumberSequenceService>());
                var result = await close.CloseAsync(new DateTime(2026,4,30),default);
                Assert.Null(result.Error); Assert.NotNull(result.Result);
                Assert.Equal(-100m,result.Result.TotalAdjustment); // signed inventory adjustment; COGS debit is +100
                Assert.Equal(190m,(await OnHandAsync(db)).PhysicalInvent);
                Assert.Equal(13300m,(await OnHandAsync(db)).PostedValue);
                Assert.Equal(700m,await LedgerAsync(db,GeneralLedgerSeedExample.CostOfGoodsSold));
                Assert.Equal(13300m,await LedgerAsync(db,GeneralLedgerSeedExample.Inventory));
                Assert.Equal(-1200m,await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue));
                await AssertOpen(0,0);
                var again = await close.CloseAsync(new DateTime(2026,4,30),default);
                Assert.Null(again.Error); Assert.Equal(0m,again.Result!.TotalAdjustment);
            });
            if(!corrections)
            await Stage(["CFG-01","CLOSE-02"],"Close the ledger period and reject a payment dated inside it",async () =>
            {
                var period = await db.Set<FiscalCalendarPeriod>().SingleAsync(x => x.StartDate <= Date && x.EndDate >= Date);
                var ledgerPeriod = await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync(x => x.FiscalCalendarPeriod == period.RecId);
                var periodService = new IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.LedgerFiscalCalendarPeriodService(
                    services.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.Repositories.IUnitOfWork>(),new ArCycleIdentity());
                var closed = await periodService.SetStatusAsync(ledgerPeriod.RecId,FiscalPeriodStatus.Closed,default);
                Assert.Null(closed.Error);
                var before = await CountsAsync(db);
                var rejected = await payment.PostAsync(new(Customer,Bank,1,Date,"ARV2",null,"OTC-CLOSED",false),default);
                responses.Add(new { id="CLOSE-02",rejected.StatusCode,rejected.Message });
                Assert.Equal(422,rejected.StatusCode);
                Assert.Equal(before,await CountsAsync(db));
                Assert.Equal(1380m,await LedgerAsync(db,GeneralLedgerSeedExample.Bank));
            });
            audit.Complete();
        }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private sealed class OtcAudit
    {
        public Func<object?>? FailureEvidence { get; set; }
        private readonly Dictionary<string,int> attempts = new();
        private readonly string runId = Guid.NewGuid().ToString("N");
        private readonly string path = Environment.GetEnvironmentVariable("IXAPI_OTC_REPORT") ?? Path.Combine(AppContext.BaseDirectory,"otc-acceptance.jsonl");
        public OtcAudit(string? sourceRunId = null)
        {
            if(sourceRunId != null) runId = sourceRunId;
            if(File.Exists(path))
            foreach(var line in File.ReadLines(path))
            {
                using var previous = JsonDocument.Parse(line);
                if(previous.RootElement.GetProperty("status").GetString() == "Running")
                {
                    var id = previous.RootElement.GetProperty("id").GetString()!;
                    attempts[id] = attempts.GetValueOrDefault(id) + 1;
                }
            }
            using var stream = typeof(OtcAudit).Assembly.GetManifestResourceStream("IXApi.Tests.OrderToCashAcceptancePlan.json")!;
            var plan = JsonSerializer.Deserialize<JsonElement[]>(stream)!;
            foreach(var test in plan) Write(test.GetProperty("id").GetString()!,"Not run",test);
        }
        private void Write(string id,string status,object? evidence)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.AppendAllText(path,JsonSerializer.Serialize(new {runId,id,status,attempt=attempts.GetValueOrDefault(id),company=Company,actor="ar-cycle-test",atUtc=DateTime.UtcNow,evidence}) + Environment.NewLine);
        }
        public async Task Stage(string[] ids,string name,Func<Task<object>> action)
        {
            foreach(var id in ids) { attempts[id] = attempts.GetValueOrDefault(id) + 1; Write(id,"Running",new {name}); }
            try
            {
                var evidence = await action();
                foreach(var id in ids) Write(id,"Passed",new {name,actual=evidence});
                if(ids.Length == 0) Write("PREPARATION","Passed",new {name,actual=evidence});
            }
            catch(Exception error)
            {
                foreach(var id in ids) Write(id,"Failed",new {name,error=error.ToString(),actual=FailureEvidence?.Invoke()});
                throw;
            }
        }
        public void RelatedCyclePassed(string[] ids,object evidence)
        {
            foreach(var id in ids)
            {
                attempts[id] = attempts.GetValueOrDefault(id) + 1;
                Write(id,"Running",new {fixture="Existing AR service regression with independent arithmetic"});
                Write(id,"Passed",evidence);
            }
        }
        public void Complete() => Write("RUN","Passed",new {scope="Only the executed stages in this run passed; inspect each test ID and its evidence. Unexecuted plan IDs remain Not run."});
    }
}
