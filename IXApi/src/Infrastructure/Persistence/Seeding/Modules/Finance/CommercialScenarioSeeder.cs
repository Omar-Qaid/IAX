using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed class CommercialScenarioSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.CommercialScenarios";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.SalesScenarios" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var userId = await context.Db.Set<AspNetUser>().IgnoreQueryFilters().Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct);
        var actor = new SalesScenarioSeeder.ExampleActor(userId);
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(context.Db.Database.GetConnectionString()).Options, companyContext: actor);
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var date = DateTime.UtcNow.Date;
        var order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == "DEMO-SO-INVOICED", ct);
        var invoice = await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
        var orderLine = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
        // Explicit invoice selection prevents settlement of unrelated customer transactions.
        var paid = await new CustPaymentService(work, actor, actor, numbers).PostAsync(
            new CustPaymentPostDto(order.CustAccount, "DEMO-SAR", 200m, date, "Transfer", invoice.InvoiceId, "DEMO-PAYMENT-001"), ct);
        if (paid.StatusCode is < 200 or >= 300) throw new InvalidOperationException(paid.Message);

        var agreement = await Ensure(db, x => x.DataAreaId == "dat" && x.JournalNum == "DEMO-PRICE-001",
            new PriceDiscAdmTable { JournalNum = "DEMO-PRICE-001", JournalName = "SALEPRICE", Name = "Example quantity-based chair prices",
                DefaultRelation = PriceType.PriceSales }, ct);
        if (agreement.Posted != NoYes.Yes)
        {
            foreach (var (line, from, to, price) in new[] { (1m, 1m, 9m, 150m), (2m, 10m, 100m, 140m) })
                await Ensure(db, x => x.DataAreaId == "dat" && x.JournalNum == agreement.JournalNum && x.LineNum == line,
                    new PriceDiscAdmTrans { JournalNum = agreement.JournalNum, LineNum = line, Relation = PriceType.PriceSales,
                        Module = ModuleInventCustVend.Cust, AccountCode = PriceDiscPartyCodeType.Table, AccountRelation = "CUST-100",
                        ItemCode = PriceDiscProductCodeType.Table, ItemRelation = "ITEM-001", Currency = "SAR", UnitId = "PCS",
                        InventDimId = orderLine.InventDimId, PriceUnit = 1, Amount = price, QuantityAmountFrom = from, QuantityAmountTo = to,
                        FromDate = date, ToDate = date.AddYears(1) }, ct);
            var posted = await new PriceDiscJournalPostingService(work, actor).PostAsync(agreement.RecId, ct);
            if (posted.Status != PriceDiscJournalPostStatus.Posted && posted.Status != PriceDiscJournalPostStatus.AlreadyPosted)
                throw new InvalidOperationException("Example price agreement validation failed: " + posted);
        }
        var customer = await db.Set<CustTable>().SingleAsync(x => x.DataAreaId == "dat" && x.AccountNum == "CUST-100", ct);
        var salesModule = await db.Set<InventTableModule>().SingleAsync(x => x.DataAreaId == "dat" && x.ItemId == "ITEM-001" && x.ModuleType == ModuleInventPurchSales.Sales, ct);
        var revenue = await db.Set<DimensionAttributeValueCombination>().FirstAsync(x => x.DataAreaId == "dat" && x.MainAccountValue == "40010000", ct);
        var draft = await Ensure(db, x => x.DataAreaId == "dat" && x.InvoiceId == "DEMO-FTI-001",
            new CustInvoiceTable { InvoiceId = "DEMO-FTI-001", Name = "Example unposted consulting invoice", OrderAccount = customer.AccountNum,
                OrderAccountRefRecId = customer.RecId, InvoiceAccount = customer.AccountNum, CustGroup = customer.CustGroupId,
                CurrencyCode = "SAR", InvoiceDate = date, DocumentDate = date, DueDate = date.AddDays(30), Payment = "Monthly",
                PostingProfile = "GEN", TaxGroup = "DOM", TaxItemGroup = salesModule.TaxItemGroupId, Posted = NoYes.No }, ct);
        await Ensure(db, x => x.DataAreaId == "dat" && x.ParentRecId == draft.RecId && x.LineNum == 1,
            new CustInvoiceLine { ParentRecId = draft.RecId, LineNum = 1, Description = "Example consulting: two hours",
                Quantity = 2, UnitPrice = 100, AmountCur = 200, LedgerDimension = revenue.RecId,
                TaxGroup = "DOM", TaxItemGroup = salesModule.TaxItemGroupId }, ct);

        var quotationTax = decimal.Round(750m * (invoice.SalesBalance == 0 ? 0 : invoice.SumTax / invoice.SalesBalance), 2);
        foreach (var (code, status) in new[] { ("DEMO-Q-DRAFT", QuotationStatus.InProcess), ("DEMO-Q-SENT", QuotationStatus.Sent) })
        {
            await Ensure(db, x => x.DataAreaId == "dat" && x.QuotationId == code,
                new SalesQuotationTable { QuotationId = code, QuotationName = "Example office chair quotation", QuotationStatus = status,
                    QuotationType = QuotationType.Sales, CustAccount = customer.AccountNum, InvoiceAccount = customer.AccountNum,
                    CurrencyCode = "SAR", QuotationExpiryDate = date.AddDays(30), ReceiptDateRequested = date.AddDays(7) }, ct);
            await Ensure(db, x => x.DataAreaId == "dat" && x.QuotationId == code && x.LineNum == 1,
                new SalesQuotationLine { QuotationId = code, LineNum = 1, ItemId = "ITEM-001", Name = "Example office chair",
                    CustAccount = customer.AccountNum, CurrencyCode = "SAR", QuotationType = QuotationType.Sales, QuotationStatus = status,
                    SalesQty = 5, QtyOrdered = 5, SalesUnit = "PCS", PriceUnit = 1, SalesPrice = 150, LineAmount = 750,
                    RemainSalesFinancial = 5, RemainSalesPhysical = 5, TaxGroup = "DOM", TaxItemGroup = salesModule.TaxItemGroupId }, ct);
            if (status != QuotationStatus.Sent) continue;
            await Ensure(db, x => x.DataAreaId == "dat" && x.QuotationDocNum == code,
                new CustQuotationJour { QuotationId = code, QuotationDocNum = code, QuotationDate = date, QuotationType = QuotationType.Sales,
                    OrderAccount = customer.AccountNum, InvoiceAccount = customer.AccountNum, CustGroup = customer.CustGroupId,
                    CurrencyCode = "SAR", Qty = 5, SalesBalance = 750, SumTax = quotationTax, QuotationAmount = 750m + quotationTax,
                    SalesBalanceMst = 750, SumTaxMst = quotationTax, QuotationAmountMst = 750m + quotationTax, ExchRate = 100, RespiteDate = date.AddDays(30) }, ct);
            await Ensure(db, x => x.DataAreaId == "dat" && x.QuotationId == code && x.LineNum == 1,
                new CustQuotationTrans { QuotationId = code, OrigQuotationId = code, QuotationDate = date, LineNum = 1,
                    ItemId = "ITEM-001", Name = "Example office chair", CurrencyCode = "SAR", Qty = 5, InventQty = 5,
                    SalesUnit = "PCS", SalesPrice = 150, PriceUnit = 1, LineAmount = 750, LineAmountMst = 750,
                    TaxAmount = quotationTax, TaxAmountMst = quotationTax, TaxGroup = "DOM", TaxItemGroup = salesModule.TaxItemGroupId }, ct);
        }
    }

    private static async Task<T> Ensure<T>(DbContext db, Expression<Func<T, bool>> key, T row, CancellationToken ct) where T : class
    {
        var existing = await db.Set<T>().IgnoreQueryFilters().SingleOrDefaultAsync(key, ct);
        if (existing != null) return existing;
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }
}
