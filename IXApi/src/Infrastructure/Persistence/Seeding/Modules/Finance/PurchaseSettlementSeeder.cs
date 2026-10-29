using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Settles the dedicated 60/40 VAT example through the real payment service.</summary>
public sealed class PurchaseSettlementSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.PurchaseSettlements";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.PurchaseVatInvoices", "Finance.OperationalExamples" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var actor = new SalesScenarioSeeder.ExampleActor(await context.Db.Set<AspNetUser>().IgnoreQueryFilters()
            .Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct));
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(context.Db.Database.GetConnectionString()).Options, companyContext: actor);
        var work = new UnitOfWork(db);
        var payments = new VendPaymentService(work, actor, actor, new SysNumberSequenceService(work, actor));
        var order = await db.Set<PurchTable>().SingleAsync(x => x.DataAreaId == "dat"
            && x.PurchName == "DEMO-PO-VAT-6040" && x.ReturnItemNum == "", ct);
        var invoices = await db.Set<VendInvoiceJour>().Where(x => x.DataAreaId == "dat" && x.PurchId == order.PurchId)
            .OrderBy(x => x.RecId).ToListAsync(ct);
        if (invoices.Count != 2 || invoices.Sum(x => x.InvoiceAmount) != 5750m)
            throw new InvalidOperationException("The purchase VAT example was customized; its payment amounts were preserved.");
        for (var i = 0; i < invoices.Count; i++)
        {
            var invoice = invoices[i];
            var request = $"DEMO-VATP-{i + 1}";
            var previous = await db.Set<LedgerJournalTrans>().SingleOrDefaultAsync(x => x.DataAreaId == "dat"
                && x.AccountType == LedgerJournalACType.Vend && x.PaymReference == request, ct);
            string bank;
            if (previous != null)
                bank = await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == "dat"
                    && x.SourceTableId == FinanceTableNumbers.LedgerJournalTrans && x.SourceRecId == previous.RecId)
                    .Select(x => x.AccountId).SingleAsync(ct);
            else
            {
                var transaction = await db.Set<VendTrans>().SingleAsync(x => x.DataAreaId == "dat"
                    && x.AccountNum == invoice.InvoiceAccount && x.Invoice == invoice.InvoiceId, ct);
                var open = await db.Set<VendTransOpen>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.RefRecId == transaction.RecId, ct);
                if (open?.AmountCur != -invoice.InvoiceAmount)
                    throw new InvalidOperationException("An existing purchase example settlement was preserved; no additional payment was posted.");
                bank = await db.Set<BankAccountTable>().Where(x => x.DataAreaId == "dat"
                    && x.BankAccountStatus == BankAccountStatus.Active && x.CurrencyCode == "SAR" && x.LedgerDimension != 0)
                    .OrderBy(x => x.AccountId).Select(x => x.AccountId).FirstAsync(ct);
            }
            var result = await payments.PostAsync(new(invoice.InvoiceAccount, bank, invoice.InvoiceAmount, request,
                previous?.TransDate ?? DateTime.UtcNow.Date, SettleInvoiceId: invoice.InvoiceId), ct);
            if (result.StatusCode != 200) throw new InvalidOperationException($"Purchase example payment failed: {result.Message}");
        }
    }
}
