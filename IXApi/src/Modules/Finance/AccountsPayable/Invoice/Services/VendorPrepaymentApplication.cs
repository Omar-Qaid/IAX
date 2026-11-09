using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed partial class VendorInvoiceService
{
    public Task<PurchResult> ApplyPrepaymentAsync(long orderRecId, long prepaymentInvoiceRecId, long finalInvoiceRecId,
        DateTime accountingDate, decimal amount, CancellationToken ct) => Db.Database.CreateExecutionStrategy().ExecuteAsync(
            () => ApplyPrepaymentCoreAsync(orderRecId, prepaymentInvoiceRecId, finalInvoiceRecId, accountingDate.Date, amount, ct));

    private async Task<PurchResult> ApplyPrepaymentCoreAsync(long orderRecId, long prepaymentInvoiceRecId, long finalInvoiceRecId,
        DateTime date, decimal amount, CancellationToken ct)
    {
        if (date == default || amount <= 0 || decimal.Round(amount, 2) != amount)
            return new(400, null, "A positive prepayment application amount and accounting date are required.");
        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var support = new SubledgerPostingSupport(Db);
            var order = await Db.Set<PurchTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == orderRecId, ct);
            if (order == null) return await Fail(404, "Purchase order was not found.", ct);
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            await support.LockAsync($"VendPayment:{area}:{order.InvoiceAccount}", ct);
            await support.LockAsync($"VendSettleOpen:{area}:{order.InvoiceAccount}".ToUpperInvariant(), ct);
            var invoices = await Db.Set<VendInvoiceJour>().AsNoTracking().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
                && x.InvoiceAccount == order.InvoiceAccount && (x.RecId == prepaymentInvoiceRecId || x.RecId == finalInvoiceRecId)).ToListAsync(ct);
            var advance = invoices.SingleOrDefault(x => x.RecId == prepaymentInvoiceRecId && x.Prepayment == NoYes.Yes);
            var final = invoices.SingleOrDefault(x => x.RecId == finalInvoiceRecId && x.Prepayment == NoYes.No);
            if (advance == null || final == null)
                return await Fail(422, "Select a prepayment invoice and a standard invoice from this purchase order.", ct);
            if (advance.CurrencyCode != final.CurrencyCode || date < advance.InvoiceDate.Date || date < final.InvoiceDate.Date)
                return await Fail(422, "Invoices must share a currency and the application cannot precede either invoice.", ct);

            var transactions = Db.Set<VendTrans>().Where(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount);
            var advanceTrans = await transactions.SingleOrDefaultAsync(x => x.Voucher == advance.LedgerVoucher && x.Invoice == advance.InvoiceId
                && x.TransType == LedgerTransType.Purch, ct);
            var finalTrans = await transactions.SingleOrDefaultAsync(x => x.Voucher == final.LedgerVoucher && x.Invoice == final.InvoiceId
                && x.TransType == LedgerTransType.Purch, ct);
            if (advanceTrans == null || finalTrans == null)
                return await Fail(422, "The original vendor transactions could not be identified.", ct);

            var applications = await (from debit in transactions
                join settlement in Db.Set<VendSettlement>() on debit.RecId equals settlement.PaymentTransRecId
                where settlement.DataAreaId == area && settlement.AccountNum == order.InvoiceAccount
                    && debit.TransType == LedgerTransType.PurchAdvanceApplication && debit.Invoice == advance.InvoiceId
                    && settlement.AmountCur > 0 && !Db.Set<VendSettlement>().Any(reversal => reversal.DataAreaId == area
                        && reversal.PaymentTransRecId == debit.RecId && reversal.InvoiceTransRecId == settlement.InvoiceTransRecId
                        && reversal.AmountCur < 0)
                select settlement).ToListAsync(ct);
            var prior = applications.SingleOrDefault(x => x.InvoiceTransRecId == finalTrans.RecId);
            if (prior != null)
            {
                if (prior.AmountCur != amount || prior.TransDate.Date != date)
                    return await Fail(422, "A different application already exists for these invoices.", ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return new(200, prior, "Purchase order prepayment already applied.");
            }

            var payments = await (from settlement in Db.Set<VendSettlement>()
                join debit in transactions on settlement.PaymentTransRecId equals debit.RecId
                join line in Db.Set<LedgerJournalTrans>() on debit.RecId equals line.VendTransId
                where settlement.DataAreaId == area && settlement.InvoiceTransRecId == advanceTrans.RecId
                    && settlement.CanBeReversed == NoYes.Yes && settlement.TransDate <= date
                    && debit.TransType == LedgerTransType.Payment && debit.AmountCur > 0
                    && line.DataAreaId == area && line.MarkedInvoice == advance.InvoiceId
                    && line.Voucher == settlement.Voucher
                select settlement.AmountCur).ToListAsync(ct);
            var available = payments.Sum() - applications.Sum(x => x.AmountCur);
            if (amount > available)
                return await Fail(422, "The application exceeds the paid prepayment amount available.", ct);
            var open = await Db.Set<VendTransOpen>().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.AccountNum == order.InvoiceAccount && x.RefRecId == finalTrans.RecId, ct);
            if (open == null || amount > -open.AmountCur || open.AmountCur != finalTrans.AmountCur - finalTrans.SettleAmountCur
                || open.AmountMst != finalTrans.AmountMst - finalTrans.SettleAmountMst || open.AmountMst != open.AmountCur)
                return await Fail(422, "The final invoice has insufficient or unreconciled open balance.", ct);

            var context = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
            if (advance.CurrencyCode != context.Ledger.AccountingCurrency)
                return await Fail(422, "This prepayment application path requires the accounting currency.", ct);
            var translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, context.Ledger, advance.CurrencyCode, date, ct);
            var originalLines = await (from journal in Db.Set<GeneralJournalEntry>()
                join line in Db.Set<GeneralJournalAccountEntry>() on journal.RecId equals line.GeneralJournalEntry
                where journal.DataAreaId == area && line.DataAreaId == area
                    && (journal.SubledgerVoucher == advance.LedgerVoucher || journal.SubledgerVoucher == final.LedgerVoucher)
                select new { journal.SubledgerVoucher, Line = line }).ToListAsync(ct);
            var assets = originalLines.Where(x => x.SubledgerVoucher == advance.LedgerVoucher && x.Line.AccountingCurrencyAmount > 0
                && x.Line.PostingType == LedgerPostingType.LedgerJournal).ToList();
            var liabilities = originalLines.Where(x => x.SubledgerVoucher == final.LedgerVoucher && x.Line.AccountingCurrencyAmount < 0
                && x.Line.PostingType == LedgerPostingType.VendBalance).ToList();
            if (assets.Count != 1 || liabilities.Count != 1 || assets[0].Line.LedgerDimension == liabilities[0].Line.LedgerDimension)
                return await Fail(422, "The original advance asset and standard invoice liability could not be identified.", ct);
            var asset = await support.LoadDimensionAsync(area, assets[0].Line.LedgerDimension, "original prepayment asset", ct);
            var liability = await support.LoadDimensionAsync(area, liabilities[0].Line.LedgerDimension, "original invoice liability", ct);
            var definition = await Db.Set<PurchPrepayTable>().SingleAsync(x => x.DataAreaId == area && x.PurchTable == order.PurchId, ct);
            if (definition.PrepayApplicationRemaining < amount)
                return await Fail(422, "The purchase order prepayment balance requires reconciliation.", ct);

            var voucher = (await _numbers.NextAsync("Voucher", area, ct)).Code;
            var entry = support.NewEntry(area, voucher, voucher, date, context);
            entry.DocumentNumber = final.InvoiceId;
            var offset = new VendTrans
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, Invoice = advance.InvoiceId, Voucher = voucher,
                TransDate = date, DueDate = date, Closed = date, Txt = $"Apply prepayment to {final.InvoiceId}",
                CurrencyCode = advance.CurrencyCode, AmountCur = amount, AmountMst = amount,
                SettleAmountCur = amount, SettleAmountMst = amount, TransType = LedgerTransType.PurchAdvanceApplication
            };
            Db.Add(entry);
            Db.Add(offset);
            await _unitOfWork.CompleteAsync(ct);
            Db.Add(translation.AccountEntry(area, entry.RecId, liability, LedgerPostingType.VendBalance, amount, offset.Txt, 0m));
            Db.Add(translation.AccountEntry(area, entry.RecId, asset, LedgerPostingType.LedgerJournal, -amount, offset.Txt, 0m));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
            });
            var settlementRow = new VendSettlement
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, InvoiceTransRecId = finalTrans.RecId,
                PaymentTransRecId = offset.RecId, TransDate = date, Voucher = voucher, AmountCur = amount, AmountMst = amount,
                CanBeReversed = NoYes.No
            };
            Db.Add(settlementRow);
            finalTrans.SettleAmountCur -= amount;
            finalTrans.SettleAmountMst -= amount;
            open.AmountCur += amount;
            open.AmountMst += amount;
            if (open.AmountCur == 0)
            {
                finalTrans.Closed = date;
                Db.Remove(open);
            }
            definition.PrepayApplicationRemaining -= amount;
            await _unitOfWork.CompleteAsync(ct);
            await new PurchaseSourceAccounting(Db).VerifyBalancedAsync(entry, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, settlementRow, "Purchase order prepayment applied.");
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
