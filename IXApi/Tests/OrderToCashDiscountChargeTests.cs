using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public sealed partial class AccountsReceivableCycleStageTests
{
    [SqlServerPhaseOneFact]
    public async Task Order_to_cash_line_total_discounts_and_taxed_charges_reach_partial_invoices_SQLServer()
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER"),
            InitialCatalog = "IXApi_OTCDiscounts_" + Guid.NewGuid().ToString("N"),
            IntegratedSecurity = true,TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var db = new ApplicationDbContext(options);
        var audit = new OtcAudit();
        var documents = new List<object>();
        audit.FailureEvidence = () => new {documents};
        try
        {
            await db.Database.EnsureCreatedAsync();
            using var provider = CycleServices(db);
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            await services.GetRequiredService<AccountsReceivableCycleSeeder>().PrepareAsync(new(2026,"otc-discounts"));
            var orders = services.GetRequiredService<ISalesOrderQuickCreateService>();
            var lines = services.GetRequiredService<ISalesOrderLineService>();
            var invoices = services.GetRequiredService<ISalesInvoiceService>();
            const string item = "APV2-SVC";

            async Task<(long Order,long Line)> Order(decimal quantity,decimal price,decimal linePercent,decimal totalPercent,bool inclusive,string key)
            {
                var created = await orders.CreateAsync(new() {CustomerAccount=Customer,CurrencyCode="SAR",SalesName=key,
                    InventSiteId="APV2-SITE",InventLocationId="APV2-WH",RequestedShipDate=Date,RequestedReceiptDate=Date},default);
                Assert.True(created.Order != null && created.Error == null,created.Error);
                var orderId = created.Order!.RecId;
                var added = await lines.AddAsync(orderId,new() {ItemNumber=item,Quantity=quantity,UnitPrice=price,PriceUnit=1,Unit="EA",
                    LineDiscountPercent=linePercent,DeliveryDate=Date,TaxGroup="ARV2",TaxItemGroup="APV2"},default);
                Assert.True(added.Status == SalesLineWriteStatus.Success,added.Error);
                var updated = await services.GetRequiredService<ISalesOrderHeaderService>().UpdateAsync(orderId,new()
                {
                    InvoiceAccount=Customer,CurrencyCode="SAR",OrderDate=Date,DeliveryDate=Date,
                    InventSiteId="APV2-SITE",InventLocationId="APV2-WH",PaymentTerms="APV2-N30",PaymentMethod="ARV2",
                    TaxGroupId="ARV2",TotalDiscountPercent=totalPercent,PricesIncludeSalesTax=inclusive
                },default);
                Assert.True(updated.Status == SalesHeaderUpdateStatus.Saved,updated.Error);
                var line = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == created.Order.SalesId);
                return (orderId,line.RecId);
            }
            async Task<CustInvoiceJour> Invoice(long orderId,long lineId,decimal quantity,string key)
            {
                var result = await invoices.PostAsync(orderId,new(Date,key,[new(lineId,quantity)]),default);
                Assert.True(result.StatusCode == 200,result.Message);
                db.ChangeTracker.Clear();
                var invoice = await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == key);
                var debt = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == invoice.InvoiceId && x.AccountNum == Customer);
                Assert.Equal(invoice.InvoiceAmount,debt.AmountCur);
                var link = await db.Set<SubledgerVoucherGeneralJournalEntry>().SingleAsync(x => x.Voucher == invoice.LedgerVoucher && x.VoucherDataAreaId == Company);
                var voucher = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == link.GeneralJournalEntry).ToListAsync();
                Assert.Equal(0m,voucher.Sum(x => x.AccountingCurrencyAmount));
                documents.Add(new {invoice.InvoiceId,invoice.SalesId,invoice.LedgerVoucher,invoice.InvoiceAmount,invoice.SalesBalance,
                    invoice.SumLineDisc,invoice.EndDisc,invoice.SumTax,invoice.SumMarkup,debt.RecId,debt.AmountCur,
                    accounts=voucher.Select(x => new {x.LedgerAccount,x.AccountingCurrencyAmount,x.PostingType})});
                return invoice;
            }

            await audit.Stage(["SO-03"],"Line and eligible total discounts agree from confirmation through partial invoicing",async () =>
            {
                foreach(var (inclusive,eligible,key) in new[] {(false,true,"EXCL"),(true,true,"INCL"),(false,false,"NO-END")})
                {
                    var module = await db.Set<InventTableModule>().SingleAsync(x => x.ItemId == item && x.ModuleType == ModuleInventPurchSales.Sales);
                    module.EndDisc = eligible ? NoYes.Yes : NoYes.No;
                    await db.SaveChangesAsync();
                    var revenueBefore = await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue);
                    var taxBefore = await LedgerAsync(db,GeneralLedgerSeedExample.PayableSalesTax);
                    var order = await Order(3,inclusive ? 115 : 100,10,10,inclusive,"OTC-DISC-" + key);
                    var confirm = await services.GetRequiredService<ISalesOrderConfirmationService>().PostAsync(order.Order,new(Date,true,"OTC-CF-" + key),default);
                    Assert.True(confirm.StatusCode == 200,confirm.Message);
                    var first = await Invoice(order.Order,order.Line,1,"OTC-DISC-" + key + "-1");
                    var second = await Invoice(order.Order,order.Line,2,"OTC-DISC-" + key + "-2");
                    var expectedNet = eligible ? 243m : 270m;
                    var expectedTax = eligible ? 36.45m : 40.5m;
                    Assert.Equal(expectedNet / 3,first.SalesBalance);
                    Assert.Equal(expectedTax / 3,first.SumTax);
                    Assert.Equal(expectedNet + expectedTax,first.InvoiceAmount + second.InvoiceAmount);
                    Assert.Equal(eligible ? (inclusive ? 31.05m : 27m) : 0m,first.EndDisc + second.EndDisc);
                    Assert.Equal(inclusive ? 34.5m : 30m,first.SumLineDisc + second.SumLineDisc);
                    var salesId = (await db.Set<SalesTable>().SingleAsync(x => x.RecId == order.Order)).SalesId;
                    var confirmed = await db.Set<CustConfirmJour>().SingleAsync(x => x.SalesId == salesId);
                    Assert.Equal(confirmed.ConfirmAmount,first.InvoiceAmount + second.InvoiceAmount);
                    Assert.Equal(revenueBefore - expectedNet,await LedgerAsync(db,GeneralLedgerSeedExample.SalesRevenue));
                    Assert.Equal(taxBefore - expectedTax,await LedgerAsync(db,GeneralLedgerSeedExample.PayableSalesTax));
                    Assert.False(await db.Set<InventTrans>().AnyAsync(x => x.ItemId == item));
                }
                return new {documents};
            });
            await audit.Stage(["SO-04"],"Taxed fixed freight reaches one partial invoice only",async () =>
            {
                var order = await Order(2,200,0,0,false,"OTC-FREIGHT");
                var revenue = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.MainAccountValue == GeneralLedgerSeedExample.SalesRevenue);
                db.Add(new MarkupTable {DataAreaId=Company,MarkupCode="OTC-SHIP",Txt="Taxed freight",ModuleType=ModuleInventPurchSales.Sales,
                    CustType=1,VendType=0,VendorLedgerDimension=revenue.RecId,TaxItemGroup="APV2"});
                await db.SaveChangesAsync();
                var work = services.GetRequiredService<IUnitOfWork>();
                var command = new MarkupTransCommandService(work,new ArCycleIdentity(),new MarkupChargeCalculator(work,new ArCycleIdentity()));
                var charge = await command.CreateAsync(MarkupModuleType.Customer,FinanceTableNumbers.SalesTable,order.Order,
                    new() {MarkupCode="OTC-SHIP",MarkupCategory=MarkupCategory.Fixed,Value=50,CurrencyCode="SAR",TaxGroup="ARV2",TaxItemGroup="APV2",TransDate=Date},default);
                var first = await Invoice(order.Order,order.Line,1,"OTC-FREIGHT-1");
                var beforeRepeat = await CountsAsync(db);
                await Invoice(order.Order,order.Line,1,"OTC-FREIGHT-1");
                Assert.Equal(beforeRepeat,await CountsAsync(db));
                var second = await Invoice(order.Order,order.Line,1,"OTC-FREIGHT-2");
                Assert.Equal(287.5m,first.InvoiceAmount); Assert.Equal(37.5m,first.SumTax); Assert.Equal(50m,first.SumMarkup);
                Assert.Equal(230m,second.InvoiceAmount); Assert.Equal(30m,second.SumTax); Assert.Equal(0m,second.SumMarkup);
                Assert.Equal(50m,(await db.Set<MarkupTrans>().SingleAsync(x => x.RecId == charge.RecId)).Posted);
                Assert.Equal(first.LedgerVoucher,(await db.Set<MarkupTrans>().SingleAsync(x => x.RecId == charge.RecId)).Voucher);
                Assert.False(await db.Set<InventTrans>().AnyAsync(x => x.ItemId == item));
                return new {documents};
            });
            audit.Complete();
        }
        finally
        {
            await using var cleanup = new ApplicationDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
