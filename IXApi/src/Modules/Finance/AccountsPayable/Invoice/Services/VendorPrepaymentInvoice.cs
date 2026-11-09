using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed partial class VendorInvoiceService
{
    public Task<PurchResult> PostPrepaymentAsync(long orderRecId, string invoiceNumber, DateTime invoiceDate, decimal amount, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostPrepaymentCoreAsync(orderRecId, invoiceNumber, invoiceDate.Date, amount, ct));

    private async Task<PurchResult> PostPrepaymentCoreAsync(long orderRecId, string invoiceNumber, DateTime date, decimal amount, CancellationToken ct)
    {
        var invoiceId = invoiceNumber?.Trim();
        if (string.IsNullOrEmpty(invoiceId) || invoiceId.Length > FieldLengths.InvoiceId
            || date == default || amount <= 0 || decimal.Round(amount, 2) != amount)
            return new(400, null, "A vendor invoice number, date and positive prepayment amount are required.");

        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var support = new SubledgerPostingSupport(Db);
            var purchaseId = await Db.Set<PurchTable>().Where(x => x.DataAreaId == area && x.RecId == orderRecId)
                .Select(x => x.PurchId).SingleOrDefaultAsync(ct);
            if (purchaseId == null) return await Fail(404, "Purchase order was not found.", ct);
            await support.LockAsync($"PurchDocument:{area}:{purchaseId}", ct);
            var order = await Db.Set<PurchTable>().AsNoTracking().SingleAsync(x => x.DataAreaId == area && x.RecId == orderRecId, ct);
            await support.LockAsync($"VendInvoice:{area}:{order.InvoiceAccount}:{invoiceId}".ToUpperInvariant(), ct);
            var existing = await Db.Set<VendInvoiceJour>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.InvoiceAccount == order.InvoiceAccount && x.InvoiceId == invoiceId, ct);
            if (existing != null)
            {
                if (existing.Prepayment != NoYes.Yes || existing.PurchId != purchaseId || existing.InvoiceDate.Date != date
                    || existing.InvoiceAmount != amount)
                    return await Fail(422, "The vendor invoice number is already in use.", ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return new(200, existing, "Vendor prepayment invoice was already posted.");
            }

            if (order.PurchaseType != PurchaseType.Purch || order.PurchStatus is PurchStatus.Canceled or PurchStatus.Invoiced)
                return await Fail(422, "An active purchase order is required.", ct);
            var definition = await Db.Set<PurchPrepayTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.PurchTable == purchaseId, ct);
            if (definition == null || definition.Currency != order.CurrencyCode || amount > definition.PrepayAvailable)
                return await Fail(422, "The invoice amount exceeds the available purchase order prepayment.", ct);
            if (await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area && x.PurchId == purchaseId && x.Prepayment != NoYes.Yes, ct))
                return await Fail(422, "A standard invoice has already been posted for this purchase order.", ct);

            var vendor = await Db.Set<VendTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.AccountNum == order.InvoiceAccount, ct);
            if (vendor == null || vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Invoicing)
                return await Fail(422, "The invoice vendor is missing or blocked for invoicing.", ct);
            if (!string.IsNullOrWhiteSpace(order.TaxGroup) || !string.IsNullOrWhiteSpace(vendor.TaxGroup))
                return await Fail(422, "This prepayment invoice path requires a purchase with no sales tax group.", ct);

            var profile = await Db.Set<VendParameters>().AsNoTracking().Where(x => x.DataAreaId == area)
                .Select(x => x.AdvancePostingProfile).SingleOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(profile))
                return await Fail(422, "Configure the vendor prepayment invoice posting profile first.", ct);
            var context = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
            if (order.CurrencyCode != context.Ledger.AccountingCurrency)
                return await Fail(422, "This prepayment invoice path requires the accounting currency.", ct);
            var translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, context.Ledger, order.CurrencyCode, date, ct);
            var liability = await support.ResolveVendorSummaryAsync(area, vendor, ct, postingProfile: profile);
            var asset = await support.ResolveInventPostingAsync(area, string.Empty, InventAccountType.PurchAdvance, ct,
                custVendAccount: vendor.AccountNum, custVendGroup: vendor.VendGroup, categoryRecId: definition.PrepayCategoryId);
            if (asset.RecId == liability.RecId)
                return await Fail(422, "The prepayment asset and liability accounts must be distinct.", ct);

            var voucher = (await _numbers.NextAsync("VendInvoiceJour", area, ct)).Code;
            var entry = support.NewEntry(area, voucher, voucher, date, context);
            entry.DocumentNumber = invoiceId;
            var accounting = new PurchaseSourceAccounting(Db);
            var header = await accounting.HeaderAsync(area, FinanceTableNumbers.VendInvoiceJour, ct);
            var source = await accounting.LineAsync(area, header, FinanceTableNumbers.VendInvoiceJour, 1m, date, ct);
            Db.Add(entry);
            await _unitOfWork.CompleteAsync(ct);
            Db.Add(translation.AccountEntry(area, entry.RecId, asset, LedgerPostingType.LedgerJournal, amount, "Purchase order prepayment invoice", 0m));
            var payable = translation.AccountEntry(area, entry.RecId, liability, LedgerPostingType.VendBalance, -amount, "Vendor prepayment invoice", 0m);
            Db.Add(payable);
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
            });
            var transaction = new VendTrans
            {
                DataAreaId = area, AccountNum = vendor.AccountNum, TransDate = date, DueDate = date, Voucher = voucher, Invoice = invoiceId,
                Txt = definition.Description, CurrencyCode = order.CurrencyCode, AmountCur = -amount,
                AmountMst = payable.AccountingCurrencyAmount, TransType = LedgerTransType.Purch
            };
            var invoice = new VendInvoiceJour
            {
                DataAreaId = area, InvoiceId = invoiceId, InternalInvoiceId = voucher, InvoiceDate = date, DueDate = date,
                PurchId = purchaseId, OrderAccount = order.OrderAccount, InvoiceAccount = vendor.AccountNum, CurrencyCode = order.CurrencyCode,
                LedgerVoucher = voucher, SourceDocumentHeader = header, Prepayment = NoYes.Yes, Qty = 1m, SalesBalance = amount, InvoiceAmount = amount
            };
            Db.Add(transaction);
            Db.Add(invoice);
            Db.Add(new VendInvoiceTrans
            {
                DataAreaId = area, SourceDocumentLine = source, InvoiceId = invoiceId, InternalInvoiceId = voucher, InvoiceDate = date,
                PurchId = purchaseId, LineNum = 1m, Qty = 1m, PurchPrice = amount, PriceUnit = 1m,
                LineAmount = amount, CurrencyCode = order.CurrencyCode
            });
            definition.PrepayAvailable -= amount;
            await _unitOfWork.CompleteAsync(ct);
            VendorOpenBalance.Add(Db, transaction);
            await accounting.CaptureAsync(area, entry, header, source, ct);
            await accounting.VerifyBalancedAsync(entry, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, invoice, "Vendor prepayment invoice posted.");
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
