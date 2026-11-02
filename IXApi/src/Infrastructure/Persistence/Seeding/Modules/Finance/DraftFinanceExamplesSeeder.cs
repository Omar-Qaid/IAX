using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Unposted charge/tax and settlement-selection examples, linked to the seeded documents.</summary>
public sealed class DraftFinanceExamplesSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.DraftExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.CommercialScenarios" };
    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        var userId = await db.Set<AspNetUser>().IgnoreQueryFilters().Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct);
        var actor = new SalesScenarioSeeder.ExampleActor(userId);
        var work = new UnitOfWork(db);
        var order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == "DEMO-SO-OPEN", ct);
        var line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
        if (!await db.Set<MarkupTrans>().AnyAsync(x => x.DataAreaId == "dat" && x.TransTableId == FinanceTableNumbers.SalesTable && x.TransRecId == order.RecId && x.MarkupCode == "Freight", ct))
            await new MarkupTransCommandService(work, actor, new MarkupChargeCalculator(work, actor)).CreateAsync(MarkupModuleType.Customer,
                FinanceTableNumbers.SalesTable, order.RecId, new MarkupTransDto { ModuleType = MarkupModuleType.Customer,
                    TransTableId = FinanceTableNumbers.SalesTable, TransRecId = order.RecId, MarkupCode = "Freight", LineNum = 1,
                    MarkupCategory = MarkupCategory.Fixed, Value = 10, CurrencyCode = "SAR", TransDate = DateTime.UtcNow.Date,
                    Txt = "Example freight charge", TaxGroup = "DOM", TaxItemGroup = line.TaxItemGroup }, ct);
        var invoicedOrder = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == "DEMO-SO-INVOICED", ct);
        var invoice = await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == invoicedOrder.SalesId, ct);
        foreach (var tax in await db.Set<TaxTrans>().Where(x => x.DataAreaId == "dat" && x.Voucher == invoice.LedgerVoucher).ToListAsync(ct))
            if (!await db.Set<TaxJournalTrans>().AnyAsync(x => x.DataAreaId == "dat" && x.TransTableId == FinanceTableNumbers.SalesLine && x.TransRecId == line.RecId && x.TaxCode == tax.TaxCode, ct))
                db.Add(new TaxJournalTrans { TransTableId = FinanceTableNumbers.SalesLine, TransRecId = line.RecId, TransDate = DateTime.UtcNow.Date,
                    TaxCode = tax.TaxCode, TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, TaxValue = tax.TaxValue,
                    TaxBaseAmount = line.LineAmount, SourceBaseAmountCur = line.LineAmount, SourceCurrencyCode = "SAR",
                    TaxAmount = decimal.Round(line.LineAmount * tax.TaxValue / 100m, 2),
                    SourceTaxAmountCur = decimal.Round(line.LineAmount * tax.TaxValue / 100m, 2), TaxDirection = tax.TaxDirection, TaxOrigin = tax.TaxOrigin });
        const string journalId = "DEMO-PAY-DRAFT";
        var journal = await db.Set<LedgerJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.JournalNum == journalId, ct);
        if (journal == null)
        {
            journal = new LedgerJournalTable { JournalNum = journalId, JournalName = "DEMO-AR", Name = "Example unposted customer payment",
                JournalType = LedgerJournalType.CustPayment, CurrencyCode = "SAR", NumOfLines = 1, JournalTotalCredit = 100,
                JournalTotalDebit = 100, JournalBalance = 0, Posted = NoYes.No };
            db.Add(journal);
            await db.SaveChangesAsync(ct);
        }
        var template = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.PaymReference == "DEMO-PAYMENT-001", ct);
        var payment = await db.Set<LedgerJournalTrans>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.JournalNum == journalId && x.LineNum == 1, ct);
        if (payment == null)
        {
            payment = new LedgerJournalTrans { JournalNum = journalId, LineNum = 1, CurrencyCode = "SAR", AmountCurCredit = 100,
                AccountType = template.AccountType, LedgerDimension = template.LedgerDimension,
                OffsetAccountType = template.OffsetAccountType, OffsetLedgerDimension = template.OffsetLedgerDimension,
                Company = "dat", OffsetCompany = "dat", TransDate = DateTime.UtcNow.Date, Txt = "Example pending invoice settlement",
                PostingProfile = template.PostingProfile, PaymMode = template.PaymMode, Invoice = invoice.InvoiceId };
            db.Add(payment);
            await db.SaveChangesAsync(ct);
        }
        var transaction = await db.Set<CustTrans>().SingleAsync(x => x.DataAreaId == "dat" && x.Invoice == invoice.InvoiceId && x.AmountCur > 0, ct);
        var open = await db.Set<CustTransOpen>().SingleAsync(x => x.DataAreaId == "dat" && x.RefRecId == transaction.RecId, ct);
        if (!await db.Set<SpecTrans>().AnyAsync(x => x.DataAreaId == "dat" && x.Code == "DM-MARK1", ct))
            db.Add(new SpecTrans { Code = "DM-MARK1", RefCompany = "dat", RefTableId = FinanceTableNumbers.CustTransOpen, RefRecId = open.RecId,
                SpecCompany = "dat", SpecTableId = FinanceTableNumbers.LedgerJournalTrans, SpecRecId = payment.RecId,
                Balance01 = Math.Min(100, open.AmountCur), CrossRate = 1, Payment = NoYes.Yes });
        await db.SaveChangesAsync(ct);
    }
}
