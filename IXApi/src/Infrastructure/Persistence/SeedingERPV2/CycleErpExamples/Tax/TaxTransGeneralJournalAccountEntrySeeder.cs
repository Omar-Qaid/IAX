using System.Data;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

/// <summary>Opt-in association fixtures for existing, uniquely matched posted input-tax entries.</summary>
/// <remarks>Does not create or modify tax transactions, vouchers, accounts or posting configuration.</remarks>
public sealed class TaxTransGeneralJournalAccountEntrySeeder(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<long>> SeedAsync(IReadOnlyCollection<long> taxIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taxIds);
        if (taxIds.Count == 0) return [];
        var ids = taxIds.Distinct().ToArray();
        if (ids.Any(id => id <= 0)) throw new InvalidOperationException("Tax association seed requires persisted tax record IDs.");
        var area = db.GetDataAreaId();
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                var taxes = await db.Set<TaxTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && ids.Contains(x.RecId)).ToListAsync(ct);
                if (taxes.Count != ids.Length)
                    throw new InvalidOperationException("Tax association seed contains missing or cross-company tax records.");
                var rows = new List<TaxTransGeneralJournalAccountEntry>();
                var usedEntries = new HashSet<long>();
                foreach (var tax in taxes.OrderBy(x => x.RecId))
                {
                    if (tax.TaxDirection != TaxDirection.Incoming || tax.TaxAmount == 0m || string.IsNullOrWhiteSpace(tax.Voucher))
                        throw new InvalidOperationException("Tax association seed supports nonzero posted input tax only.");
                    var sourceExists = tax.SourceTableId switch
                    {
                        FinanceTableNumbers.LedgerJournalTrans => await db.Set<LedgerJournalTrans>().AnyAsync(x => x.DataAreaId == area
                            && x.RecId == tax.SourceRecId && x.Voucher == tax.Voucher && x.TransDate == tax.TransDate, ct),
                        FinanceTableNumbers.VendInvoiceTrans => await db.Set<VendInvoiceTrans>().AnyAsync(x => x.DataAreaId == area
                            && x.RecId == tax.SourceRecId && x.InternalInvoiceId == tax.Voucher && x.InvoiceDate == tax.TransDate, ct),
                        _ => false
                    };
                    if (!sourceExists)
                        throw new InvalidOperationException("Tax association seed requires a valid source vendor invoice or journal line in the same company.");
                    var code = await db.Set<TaxTable>().AsNoTracking()
                        .SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxCode == tax.TaxCode, ct);
                    var taxAccounts = code == null ? null : await db.Set<TaxLedgerAccountGroup>().AsNoTracking()
                        .SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxAccountGroup == code.TaxAccountGroup, ct);
                    if (taxAccounts?.TaxIncomingLedgerDimension is not > 0)
                        throw new InvalidOperationException("Tax association seed requires the configured input-tax ledger dimension.");
                    var candidates = await (
                        from entry in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                        join header in db.Set<GeneralJournalEntry>().AsNoTracking() on entry.GeneralJournalEntry equals header.RecId
                        where entry.DataAreaId == area && header.DataAreaId == area
                            && header.SubledgerVoucherDataAreaId == area && header.SubledgerVoucher == tax.Voucher
                            && header.AccountingDate == tax.TransDate && entry.PostingType == LedgerPostingType.Tax
                            && entry.LedgerDimension == taxAccounts.TaxIncomingLedgerDimension.Value
                            && entry.AccountingCurrencyAmount == tax.TaxAmount
                            && entry.TransactionCurrencyCode == tax.CurrencyCode
                            && entry.TransactionCurrencyAmount == tax.TaxAmountCur
                        select entry).ToListAsync(ct);
                    if (candidates.Count != 1)
                        throw new InvalidOperationException($"Tax record {tax.RecId} has {candidates.Count} matching tax ledger entries; no association was guessed.");
                    var account = candidates[0];
                    if (!usedEntries.Add(account.RecId)
                        || await db.Set<TaxTransGeneralJournalAccountEntry>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == area
                            && x.GeneralJournalAccountEntry == account.RecId && x.TaxTrans != tax.RecId, ct))
                        throw new InvalidOperationException("Tax association seed requires an unambiguous one-to-one match; allocation is not inferred.");
                    var dimension = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
                        .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == account.LedgerDimension, ct);
                    if (dimension == null || dimension.MainAccount != account.MainAccount
                        || !await db.Set<MainAccount>().AnyAsync(x => x.DataAreaId == area && x.RecId == account.MainAccount, ct)
                        || !await db.Set<Currency>().AnyAsync(x => x.DataAreaId == area && x.CurrencyCode == tax.CurrencyCode, ct))
                        throw new InvalidOperationException("Tax association seed has an invalid dimension, main account or currency reference.");
                    var existing = await db.Set<TaxTransGeneralJournalAccountEntry>().IgnoreQueryFilters()
                        .Where(x => x.DataAreaId == area && x.TaxTrans == tax.RecId).ToListAsync(ct);
                    if (existing.Count > 1 || existing.Any(x => x.IsDeleted || x.GeneralJournalAccountEntry != account.RecId
                        || x.LedgerDimension != account.LedgerDimension || x.AccountingCurrencyAmount != tax.TaxAmount
                        || x.TransactionCurrency != tax.CurrencyCode || x.TransactionCurrencyAmount != tax.TaxAmountCur
                        || x.TaxUncommitted != null || x.SubledgerJournalAccountEntry != null
                        || x.TaxTransRelationship != null || x.MonetaryAmountType != null || x.AllocationFactor != null))
                        throw new InvalidOperationException("An existing tax association conflicts with the requested fixture; it was not overwritten.");
                    if (existing.Count == 1) rows.Add(existing[0]);
                    else
                    {
                        // No source exists for uncommitted/subledger references or the enum codes here.
                        // Leave those nullable fields unset rather than manufacturing D365 values.
                        var row = new TaxTransGeneralJournalAccountEntry
                        {
                            DataAreaId = area, TaxTrans = tax.RecId, GeneralJournalAccountEntry = account.RecId,
                            LedgerDimension = account.LedgerDimension, AccountingCurrencyAmount = tax.TaxAmount,
                            TransactionCurrency = tax.CurrencyCode, TransactionCurrencyAmount = tax.TaxAmountCur
                        };
                        db.Add(row);
                        rows.Add(row);
                    }
                }
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return (IReadOnlyList<long>)rows.Select(x => x.RecId).ToArray();
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
