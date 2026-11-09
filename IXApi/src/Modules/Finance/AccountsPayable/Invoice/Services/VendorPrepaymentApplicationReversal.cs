using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed partial class VendorInvoiceService
{
    public Task<PurchResult> ReversePrepaymentApplicationAsync(long orderRecId, long applicationRecId, DateTime accountingDate,
        CancellationToken ct) => Db.Database.CreateExecutionStrategy().ExecuteAsync(
            () => ReversePrepaymentApplicationCoreAsync(orderRecId, applicationRecId, accountingDate.Date, ct));

    private async Task<PurchResult> ReversePrepaymentApplicationCoreAsync(long orderRecId, long applicationRecId,
        DateTime date, CancellationToken ct)
    {
        if (applicationRecId <= 0 || date == default)
            return new(400, null, "An application and an accounting date are required.");
        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == orderRecId, ct);
            if (order == null) return await Fail(404, "Purchase order was not found.", ct);
            var support = new SubledgerPostingSupport(Db);
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            await support.LockAsync($"VendPayment:{area}:{order.InvoiceAccount}", ct);
            await support.LockAsync($"VendSettleOpen:{area}:{order.InvoiceAccount}".ToUpperInvariant(), ct);

            var matches = Db.Set<VendSettlement>().Where(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount);
            var application = await matches.AsNoTracking().SingleOrDefaultAsync(x => x.RecId == applicationRecId && x.AmountCur > 0, ct);
            if (application == null) return await Fail(404, "Prepayment application was not found.", ct);
            var transactions = Db.Set<VendTrans>().Where(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount);
            var offset = await transactions.AsNoTracking().SingleOrDefaultAsync(x => x.RecId == application.PaymentTransRecId
                && x.TransType == LedgerTransType.PurchAdvanceApplication && x.Voucher == application.Voucher, ct);
            var final = await transactions.SingleOrDefaultAsync(x => x.RecId == application.InvoiceTransRecId, ct);
            if (offset == null || final == null || !await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area
                && x.PurchId == order.PurchId && x.InvoiceAccount == order.InvoiceAccount && x.Prepayment == NoYes.No
                && x.InvoiceId == final.Invoice && x.LedgerVoucher == final.Voucher, ct)
                || !await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area && x.PurchId == order.PurchId
                    && x.InvoiceAccount == order.InvoiceAccount && x.Prepayment == NoYes.Yes && x.InvoiceId == offset.Invoice, ct))
                return await Fail(422, "The application does not belong to this purchase order.", ct);
            var prior = await matches.AsNoTracking().SingleOrDefaultAsync(x => x.PaymentTransRecId == offset.RecId
                && x.InvoiceTransRecId == final.RecId && x.AmountCur < 0, ct);
            if (prior != null)
            {
                if (prior.TransDate.Date != date)
                    return await Fail(422, "The application was already reversed on a different accounting date.", ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return new(200, prior, "Prepayment application already reversed.");
            }
            if (date < application.TransDate.Date || application.AmountCur != application.AmountMst
                || final.SettleAmountCur > -application.AmountCur || final.SettleAmountMst > -application.AmountMst)
                return await Fail(422, "The application date or invoice settlement balance prevents reversal.", ct);

            var postedLines = await (from originalEntry in Db.Set<GeneralJournalEntry>()
                join line in Db.Set<GeneralJournalAccountEntry>() on originalEntry.RecId equals line.GeneralJournalEntry
                where originalEntry.DataAreaId == area && line.DataAreaId == area && originalEntry.SubledgerVoucher == application.Voucher
                select line).ToListAsync(ct);
            var originalLiability = postedLines.SingleOrDefault(x => x.PostingType == LedgerPostingType.VendBalance
                && x.AccountingCurrencyAmount == application.AmountMst);
            var originalAsset = postedLines.SingleOrDefault(x => x.PostingType == LedgerPostingType.LedgerJournal
                && x.AccountingCurrencyAmount == -application.AmountMst);
            if (postedLines.Count != 2 || originalLiability == null || originalAsset == null)
                return await Fail(422, "The application ledger entry could not be reconciled.", ct);
            var liability = await support.LoadDimensionAsync(area, originalLiability.LedgerDimension, "original application liability", ct);
            var asset = await support.LoadDimensionAsync(area, originalAsset.LedgerDimension, "original application asset", ct);
            var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
            if (offset.CurrencyCode != ledger.Ledger.AccountingCurrency)
                return await Fail(422, "The reversal requires the original accounting currency.", ct);
            var definition = await Db.Set<PurchPrepayTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.PurchTable == order.PurchId, ct);
            if (definition == null) return await Fail(422, "The purchase order prepayment definition was not found.", ct);

            var voucher = (await _numbers.NextAsync("Voucher", area, ct)).Code;
            var entry = support.NewEntry(area, voucher, voucher, date, ledger);
            entry.DocumentNumber = final.Invoice;
            var reversal = new VendSettlement
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, InvoiceTransRecId = final.RecId,
                PaymentTransRecId = offset.RecId, TransDate = date, Voucher = voucher, AmountCur = -application.AmountCur,
                AmountMst = -application.AmountMst, CanBeReversed = NoYes.No
            };
            Db.Add(entry);
            Db.Add(reversal);
            Db.Add(new VendTrans
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, Invoice = offset.Invoice, Voucher = voucher,
                TransDate = date, DueDate = date, Closed = date, CurrencyCode = offset.CurrencyCode,
                Txt = $"Reverse prepayment application {application.Voucher}", TransType = LedgerTransType.PurchAdvanceApplication,
                AmountCur = reversal.AmountCur, AmountMst = reversal.AmountMst,
                SettleAmountCur = reversal.AmountCur, SettleAmountMst = reversal.AmountMst
            });
            final.SettleAmountCur += application.AmountCur;
            final.SettleAmountMst += application.AmountMst;
            await VendSettlementRestore.RestoreOpenAsync(Db, area, final, ct);
            definition.PrepayApplicationRemaining += application.AmountCur;
            await _unitOfWork.CompleteAsync(ct);
            var text = $"Reverse prepayment application {application.Voucher}";
            Db.AddRange(
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, liability, originalLiability.PostingType,
                    -originalLiability.TransactionCurrencyAmount, -originalLiability.AccountingCurrencyAmount, -originalLiability.ReportingCurrencyAmount,
                    text, 0m, originalLiability.TransactionCurrencyCode),
                SubledgerPostingSupport.AccountEntry(area, entry.RecId, asset, originalAsset.PostingType,
                    -originalAsset.TransactionCurrencyAmount, -originalAsset.AccountingCurrencyAmount, -originalAsset.ReportingCurrencyAmount,
                    text, 0m, originalAsset.TransactionCurrencyCode));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
            });
            await _unitOfWork.CompleteAsync(ct);
            await new PurchaseSourceAccounting(Db).VerifyBalancedAsync(entry, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, reversal, "Prepayment application reversed.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }
}
