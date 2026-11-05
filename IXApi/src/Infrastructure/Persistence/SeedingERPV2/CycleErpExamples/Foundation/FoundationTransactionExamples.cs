using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
/// <summary>Exercise Foundation tax and charges through the existing AP business-service examples.</summary>
public sealed class FoundationTransactionExamples(ApplicationDbContext db, FoundationCycleSeeder preparation, AccountsPayableTransactionExamples transactions)
{
    public async Task<AccountsPayableTransactionReport> RunAsync(FoundationExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        await preparation.PrepareAsync(options, ct);
        db.ChangeTracker.Clear();
        var report = await transactions.RunAsync(new AccountsPayableTransactionOptions(options.FiscalYear, options.Actor, ["04", "07"]), ct);
        var area = db.GetDataAreaId();
        var key = $"APV2-{options.FiscalYear}-04-DOC";
        var journal = await db.Set<LedgerJournalTable>().SingleAsync(x => x.DataAreaId == area && x.Name == key, ct);
        var sources = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == area && x.JournalNum == journal.JournalNum && x.TaxGroup != "").ToListAsync(ct);
        foreach (var source in sources)
        {
            var posted = await db.Set<TaxTrans>().Where(x => x.DataAreaId == area && x.SourceTableId == IAX.IXApi.Modules.Finance.Common.FinanceTableNumbers.LedgerJournalTrans && x.SourceRecId == source.RecId).ToListAsync(ct);
            if (posted.Count == 0) throw new InvalidOperationException("Foundation taxed journal has no posted tax.");
            foreach (var tax in posted)
            {
                var detail = await db.Set<TaxJournalTrans>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TransTableId == tax.SourceTableId && x.TransRecId == source.RecId && x.TaxCode == tax.TaxCode, ct);
                if (detail == null || detail.TaxGroup != source.TaxGroup || detail.TaxItemGroup != source.TaxItemGroup
                    || detail.SourceCurrencyCode != tax.SourceCurrencyCode || detail.TransDate != source.TransDate
                    || detail.SourceBaseAmountCur != tax.SourceBaseAmountCur || detail.SourceTaxAmountCur != tax.SourceTaxAmountCur
                    || detail.TaxBaseAmount != tax.TaxBaseAmount || detail.TaxAmount != tax.TaxAmount
                    || detail.OperationLedgerDimension != source.OffsetLedgerDimension)
                    throw new InvalidOperationException("Foundation TaxJournalTrans does not reconcile to its source journal and posted TaxTrans.");
            }
        }
        return report;
    }
}
