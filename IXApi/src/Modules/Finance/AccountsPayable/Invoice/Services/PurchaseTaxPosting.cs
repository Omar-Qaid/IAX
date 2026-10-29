using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

internal sealed class PurchaseTaxPosting(DbContext db)
{
    public async Task<decimal> PostAsync(string area, VendInvoiceTrans line, long entryId, string currency, CancellationToken ct,
        long? originalInvoiceLine = null)
    {
        var support = new SubledgerPostingSupport(db);
        List<(string Code, decimal Rate, decimal Amount)> taxes;
        if (originalInvoiceLine.HasValue)
        {
            var original = await db.Set<VendInvoiceTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == originalInvoiceLine, ct)
                ?? throw new SubledgerPostingException("The original invoice line for purchase tax reversal was not found.");
            var snapshots = await db.Set<TaxTrans>().AsNoTracking().Where(x => x.DataAreaId == area
                && x.SourceTableId == FinanceTableNumbers.VendInvoiceTrans && x.SourceRecId == original.RecId && x.TaxDirection == TaxDirection.Incoming).ToListAsync(ct);
            if (original.Qty <= 0 || (original.TaxAmount != 0 && snapshots.Count == 0))
                throw new SubledgerPostingException("The original invoice has no reliable purchase tax snapshot.");
            var credits = await db.Set<VendInvoiceTrans>().AsNoTracking().Where(x => x.DataAreaId == area
                && x.OriginalInvoiceTransRecId == original.RecId && x.RecId != line.RecId).ToListAsync(ct);
            var remainingQty = original.Qty + credits.Sum(x => x.Qty);
            if (line.Qty >= 0 || -line.Qty > remainingQty)
                throw new SubledgerPostingException("Purchase tax credit exceeds the original invoice quantity.");
            var creditIds = credits.Select(x => x.RecId).ToList();
            var previousTax = await db.Set<TaxTrans>().AsNoTracking().Where(x => x.DataAreaId == area
                && x.SourceTableId == FinanceTableNumbers.VendInvoiceTrans && creditIds.Contains(x.SourceRecId)).ToListAsync(ct);
            taxes = snapshots.Select(x => {
                var remainingTax = x.TaxAmount + previousTax.Where(t => t.TaxCode == x.TaxCode).Sum(t => t.TaxAmount);
                var amount = -line.Qty == remainingQty ? -remainingTax
                    : -Math.Min(remainingTax, decimal.Round(x.TaxAmount * -line.Qty / original.Qty, 2, MidpointRounding.AwayFromZero));
                return (x.TaxCode, x.TaxValue, amount);
            }).ToList();
        }
        else
        {
            var codes = await support.ApplicableTaxCodesAsync(area, line.TaxGroup, line.TaxItemGroup, line.InvoiceDate, ct);
            var total = decimal.Round(line.LineAmount * codes.Sum(x => x.Rate) / 100m, 2, MidpointRounding.AwayFromZero);
            taxes = SubledgerPostingSupport.AllocateTax(codes, total).Select(x => (x.Code, codes.Single(c => c.Code == x.Code).Rate, x.Tax)).ToList();
        }
        foreach (var (code, rate, amount) in taxes)
        {
            var table = await db.Set<TaxTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxCode == code, ct);
            if (table == null) throw new SubledgerPostingException($"Purchase tax code '{code}' was not found.");
            var group = await db.Set<TaxLedgerAccountGroup>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxAccountGroup == table!.TaxAccountGroup, ct);
            if (group?.TaxIncomingLedgerDimension is not > 0)
                throw new SubledgerPostingException($"Purchase input tax account is missing for tax code '{code}'.");
            var account = await support.LoadDimensionAsync(area, group.TaxIncomingLedgerDimension.Value, "purchase input tax", ct);
            db.Add(SubledgerPostingSupport.AccountEntry(area, entryId, account, LedgerPostingType.Tax, amount,
                $"Purchase input tax {code} on {line.InvoiceId}", 0m, currency));
            db.Add(new TaxTrans { DataAreaId = area, SourceTableId = FinanceTableNumbers.VendInvoiceTrans, SourceRecId = line.RecId,
                SourceDocumentLine = line.RecId, TransDate = line.InvoiceDate, Voucher = line.InternalInvoiceId, TaxCode = code,
                TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, TaxValue = rate, TaxDirection = TaxDirection.Incoming,
                SourceCurrencyCode = line.CurrencyCode, CurrencyCode = currency,
                SourceBaseAmountCur = line.LineAmount, SourceTaxAmountCur = amount, TaxBaseAmountCur = line.LineAmount, TaxAmountCur = amount,
                TaxBaseAmount = line.LineAmount, TaxAmount = amount, TaxBaseAmountRep = line.LineAmount, TaxAmountRep = amount });
        }
        line.TaxAmount = taxes.Sum(x => x.Amount);
        return line.TaxAmount;
    }
}
