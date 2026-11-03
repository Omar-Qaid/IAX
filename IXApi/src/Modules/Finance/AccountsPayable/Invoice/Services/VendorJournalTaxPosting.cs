using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>Inclusive input tax on a vendor invoice journal; the shared journal engine owns posting.</summary>
internal sealed class VendorJournalTaxPosting(DbContext db)
{
    internal async Task<IReadOnlyList<GeneralJournalAccountEntry>> BuildAsync(string area,
        LedgerJournalTable header, LedgerJournalTrans line, long entryId,
        DimensionAttributeValueCombination expense, CancellationToken ct)
    {
        if (line.AccountType != LedgerJournalACType.Vend || string.IsNullOrEmpty(line.TaxGroup)) return [];
        if (header.LedgerJournalInclTax != NoYes.Yes || string.IsNullOrEmpty(line.TaxItemGroup))
            throw new SubledgerPostingException("Vendor invoice journal tax requires inclusive amounts and both tax groups.");
        var support = new SubledgerPostingSupport(db);
        var ledger = await db.Set<Ledger>().AsNoTracking().SingleAsync(x => x.DataAreaId == area, ct);
        var translation = await SubledgerCurrencyTranslation.LoadAsync(db, area, ledger, line.CurrencyCode, line.TransDate, ct);
        var codes = await support.ApplicableTaxCodesAsync(area, line.TaxGroup, line.TaxItemGroup, line.TransDate, ct);
        var rate = codes.Sum(x => x.Rate);
        if (rate < 0m || rate <= -100m) throw new SubledgerPostingException("The vendor journal tax rate is invalid.");
        var gross = line.AmountCurCredit - line.AmountCurDebit;
        var net = decimal.Round(gross / (1m + rate / 100m), 2, MidpointRounding.AwayFromZero);
        var allocations = SubledgerPostingSupport.AllocateTax(codes, gross - net);
        var entries = new List<GeneralJournalAccountEntry>();
        foreach (var (code, amount) in allocations)
        {
            var tax = await db.Set<TaxTable>().AsNoTracking().SingleAsync(x => x.DataAreaId == area && x.TaxCode == code, ct);
            if (amount != 0m)
            {
                var group = await db.Set<TaxLedgerAccountGroup>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxAccountGroup == tax.TaxAccountGroup, ct);
                if (group?.TaxIncomingLedgerDimension is not > 0)
                    throw new SubledgerPostingException($"Purchase input tax account is missing for tax code '{code}'.");
                var account = await support.LoadDimensionAsync(area, group.TaxIncomingLedgerDimension.Value, "vendor journal input tax", ct);
                entries.Add(translation.AccountEntry(area, entryId, account, LedgerPostingType.Tax, amount, line.Txt, 0m));
                entries.Add(translation.AccountEntry(area, entryId, expense, LedgerPostingType.LedgerJournal, -amount, "Inclusive vendor tax reclassification", 0m));
            }
            db.Add(new TaxTrans { DataAreaId = area, SourceTableId = FinanceTableNumbers.LedgerJournalTrans,
                SourceRecId = line.RecId, SourceDocumentLine = line.RecId, Voucher = line.Voucher, TransDate = line.TransDate,
                TaxCode = code, TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup,
                TaxDirection = TaxDirection.Incoming, TaxValue = codes.Single(x => x.Code == code).Rate,
                SourceCurrencyCode = line.CurrencyCode, CurrencyCode = line.CurrencyCode,
                SourceBaseAmountCur = net, SourceTaxAmountCur = amount, TaxBaseAmountCur = net, TaxAmountCur = amount,
                TaxBaseAmount = translation.ToAccounting(net), TaxAmount = translation.ToAccounting(amount),
                TaxBaseAmountRep = translation.ToReporting(net), TaxAmountRep = translation.ToReporting(amount) });
        }
        return entries;
    }
}
