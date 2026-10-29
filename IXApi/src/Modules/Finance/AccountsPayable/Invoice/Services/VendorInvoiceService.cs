using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Vendor invoice (financial update). Received receipt portions become Purchased at the invoiced
/// cost, the vendor liability is recorded in VendTrans, and the voucher debits the purchase
/// inventory-receipt account and credits the vendor summary account from posting profiles.
/// Receipt-before-invoice is enforced with receipt-line quantity links and input-tax snapshots.
/// Charges and receipt-accrual reversal are separate posting concerns.
/// </summary>
public sealed class VendorInvoiceService : BaseService<VendInvoiceJour>, IVendorInvoiceService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;
    private readonly IValidator<VendorInvoicePostDto>? _validator;

    private DbContext Db => _unitOfWork.Context;

    public VendorInvoiceService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IValidator<VendorInvoicePostDto>? validator = null)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
        _validator = validator;
    }

    public VendorInvoiceService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers,
        IValidator<VendorInvoicePostDto>? validator = null)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers, validator)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<PurchResult> PostAsync(long recId, VendorInvoicePostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(recId, input, ct));

    private async Task<PurchResult> PostCoreAsync(long recId, VendorInvoicePostDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var validation = await (_validator ?? new VendorInvoicePostDtoValidator()).ValidateAsync(input, ct);
        if (!validation.IsValid) return new(400, null, validation.Errors[0].ErrorMessage);
        var date = (input.InvoiceDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Purchase order was not found in the selected company.", ct);
            if (order.PurchaseType == PurchaseType.ReturnItem)
                return await Fail(422, "Vendor return orders are credited through the return order credit note.", ct);
            await support.LockAsync($"PurchDocument:{area}:{order.PurchId}", ct);
            var vendorInvoiceId = input.InvoiceId.Trim();
            await support.LockAsync($"VendInvoice:{area}:{order.InvoiceAccount}:{vendorInvoiceId}".ToUpperInvariant(), ct);
            if (await Db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area
                && x.InvoiceAccount == order.InvoiceAccount && x.InvoiceId == vendorInvoiceId, ct))
                return await Fail(422, $"Vendor invoice '{vendorInvoiceId}' has already been posted for this vendor.", ct);

            var vendor = await Db.Set<VendTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount, ct);
            if (vendor == null) return await Fail(422, "The invoice vendor was not found.", ct);
            if (vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Invoicing)
                return await Fail(422, "The vendor is blocked for invoicing.", ct);

            var dueDate = date;
            if (!string.IsNullOrWhiteSpace(order.Payment))
            {
                var term = await Db.Set<PaymTerm>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.PaymTermId == order.Payment, ct);
                if (term == null) return await Fail(422, "The purchase order payment terms were not found in this company.", ct);
                try { dueDate = IAX.IXApi.Modules.Finance.Foundation.PaymentTerms.PaymentDueDateCalculator.Calculate(date, term); }
                catch (InvalidOperationException ex) { return await Fail(422, ex.Message, ct); }
            }
            var context = await support.LoadLedgerContextAsync(area, date, ct);
            if (order.CurrencyCode != context.Ledger.AccountingCurrency || (!string.IsNullOrEmpty(context.Ledger.ReportingCurrency) && context.Ledger.ReportingCurrency != context.Ledger.AccountingCurrency))
                return await Fail(422, "Purchase invoice currency conversion requires supported accounting/reporting exchange rates.", ct);
            var vendorAccount = await support.ResolveVendorSummaryAsync(area, vendor, ct);
            var internalId = (await _numbers.NextAsync("VendInvoiceJour", area, ct)).Code;
            var entry = support.NewEntry(area, internalId, internalId, date, context);
            entry.DocumentNumber = vendorInvoiceId;
            Db.Add(entry);
            await _unitOfWork.CompleteAsync(ct);

            decimal total = 0m, totalQty = 0m, totalTax = 0m;
            foreach (var request in input.Lines)
            {
                var line = await Db.Set<PurchLine>().FirstOrDefaultAsync(x => x.RecId == request.PurchLineRecId
                    && x.DataAreaId == area && x.PurchId == order.PurchId, ct);
                if (line == null) return await Fail(404, $"Purchase line {request.PurchLineRecId} was not found.", ct);
                var boundaryError = await IAX.IXApi.Modules.Finance.Inventory.InventoryFinancialBoundary.CheckAsync(Db, area, line.ItemId, date, ct);
                if (boundaryError != null) return await Fail(422, boundaryError, ct);
                if (request.Qty <= 0) return await Fail(422, "The invoice quantity must be greater than zero.", ct);
                var receivedNotInvoiced = line.RemainPurchFinancial - line.RemainPurchPhysical;
                if (request.Qty > receivedNotInvoiced)
                    return await Fail(422, $"Invoice quantity for item {line.ItemId} exceeds the received, uninvoiced quantity {receivedNotInvoiced}.", ct);
                var unitPrice = request.UnitPrice ?? line.PurchPrice;
                if (unitPrice < 0) return await Fail(422, "The invoice price cannot be negative.", ct);
                var inventQty = request.Qty == line.RemainPurchFinancial
                    ? line.RemainInventFinancial
                    : decimal.Round(request.Qty * line.QtyOrdered / line.PurchQty, 6);
                var amount = decimal.Round(request.Qty * unitPrice / (line.PriceUnit > 0 ? line.PriceUnit : 1m), 2, MidpointRounding.AwayFromZero);
                var unitCost = inventQty == 0 ? 0m : amount / inventQty;

                var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.InventTransId == line.InventTransId && x.ReferenceId == order.PurchId
                    && x.ReferenceCategory == InventRefType.PurchaseOrder, ct);
                if (origin == null) return await Fail(422, $"Inventory origin is missing for item {line.ItemId}.", ct);
                var received = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
                    && x.StatusReceipt == StatusReceipt.Received).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
                if (received.Sum(x => x.Qty) < inventQty)
                    return await Fail(422, $"Received inventory does not cover the invoice for item {line.ItemId}.", ct);

                var left = inventQty;
                decimal physicalReleased = 0m;
                var receiptMatches = new Dictionary<long, decimal>();
                foreach (var source in received)
                {
                    if (left <= 0) break;
                    var take = Math.Min(source.Qty, left);
                    var receipt = await Db.Set<VendPackingSlipTrans>().SingleOrDefaultAsync(x => x.DataAreaId == area
                        && x.InternalPackingSlipId == source.PackingSlipId && x.InventTransId == line.InventTransId
                        && x.OrigPurchId == order.PurchId && x.InventDimId == source.InventDimId, ct);
                    if (receipt == null) return await Fail(422, "The received inventory has no matching product-receipt line.", ct);
                    receiptMatches[receipt.RecId] = receiptMatches.GetValueOrDefault(receipt.RecId) + take;
                    var physicalPerUnit = source.Qty == 0 ? 0m : source.CostAmountPhysical / source.Qty;
                    if (take < source.Qty)
                    {
                        var remainder = source.Qty - take;
                        Db.Set<InventTrans>().Add(new InventTrans
                        {
                            DataAreaId = source.DataAreaId, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                            InventDimId = source.InventDimId, Qty = remainder, StatusReceipt = StatusReceipt.Received,
                            StatusIssue = StatusIssue.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                            DateExpected = source.DateExpected, DatePhysical = source.DatePhysical, DateStatus = source.DateStatus,
                            DateInvent = source.DateInvent, PackingSlipId = source.PackingSlipId, VoucherPhysical = source.VoucherPhysical,
                            CostAmountPhysical = remainder * physicalPerUnit
                        });
                    }
                    physicalReleased += take * physicalPerUnit;
                    source.Qty = take;
                    source.CostAmountPhysical = take * physicalPerUnit;
                    source.StatusReceipt = StatusReceipt.Purchased;
                    source.DateFinancial = date;
                    source.DateStatus = date;
                    source.InvoiceId = internalId;
                    source.Voucher = internalId;
                    source.CostAmountPosted = take * unitCost;
                    left -= take;
                }

                var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                if (summary == null) return await Fail(422, $"Inventory summary is missing for item {line.ItemId}.", ct);
                summary.Received -= inventQty;
                summary.PostedQty += inventQty;
                summary.PhysicalValue -= physicalReleased;
                summary.PostedValue += amount;

                line.RemainPurchFinancial -= request.Qty;
                line.RemainInventFinancial = Math.Max(0m, line.RemainInventFinancial - inventQty);
                if (line.RemainPurchFinancial <= 0 && line.RemainPurchPhysical <= 0) line.PurchStatus = PurchStatus.Invoiced;

                var inventoryAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.PurchReceipt, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, inventoryAccount, LedgerPostingType.PurchReceipt, amount,
                    $"Purchase {order.PurchId} item {line.ItemId}", inventQty, context.Ledger.AccountingCurrency));

                var invoiceLine = new VendInvoiceTrans
                {
                    DataAreaId = area, InvoiceId = vendorInvoiceId, InternalInvoiceId = internalId, InvoiceDate = date,
                    PurchId = order.PurchId, LineNum = line.LineNumber, ItemId = line.ItemId, Qty = request.Qty,
                    InventQty = inventQty, PurchUnit = line.PurchUnit, PurchPrice = unitPrice, PriceUnit = line.PriceUnit,
                    LineAmount = amount, CurrencyCode = order.CurrencyCode, TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup,
                    InventTransId = line.InventTransId, InventDimId = line.InventDimId
                };
                Db.Add(invoiceLine);
                await _unitOfWork.CompleteAsync(ct);
                foreach (var (receiptId, matchedInventQty) in receiptMatches)
                {
                    var match = new VendInvoicePackingSlipQuantityMatch { DataAreaId = area, InvoiceTransRecId = invoiceLine.RecId,
                        PackingSlipTransRecId = receiptId, InventQuantity = matchedInventQty, Quantity = matchedInventQty * request.Qty / inventQty };
                    await new PurchaseReceiptAccrualPosting(Db).ReverseAsync(area, match, entry.RecId, context.Ledger.AccountingCurrency, ct);
                    Db.Add(match);
                }
                totalTax += await new PurchaseTaxPosting(Db).PostAsync(area, invoiceLine, entry.RecId, context.Ledger.AccountingCurrency, ct);
                total += amount;
                totalQty += request.Qty;
            }

            Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, vendorAccount, LedgerPostingType.VendBalance, -(total + totalTax),
                $"Vendor invoice {vendorInvoiceId}", 0m, context.Ledger.AccountingCurrency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = internalId,
                VoucherDataAreaId = area, AccountingDate = date
            });
            var vendorTransaction = new VendTrans
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, TransDate = date, Voucher = internalId,
                Invoice = vendorInvoiceId, Txt = $"Purchase {order.PurchId}", CurrencyCode = order.CurrencyCode,
                AmountCur = -(total + totalTax), AmountMst = -(total + totalTax), DueDate = dueDate, TransType = LedgerTransType.Purch
            };
            Db.Set<VendTrans>().Add(vendorTransaction);
            await _unitOfWork.CompleteAsync(ct);
            VendorOpenBalance.Add(Db, vendorTransaction);
            Db.Set<VendInvoiceJour>().Add(new VendInvoiceJour
            {
                DataAreaId = area, InvoiceId = vendorInvoiceId, InternalInvoiceId = internalId, InvoiceDate = date,
                PurchId = order.PurchId, OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount,
                CurrencyCode = order.CurrencyCode, LedgerVoucher = internalId, Qty = totalQty,
                SalesBalance = total, SumTax = totalTax, InvoiceAmount = total + totalTax, DueDate = dueDate
            });

            var lines = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
                && x.PurchStatus != PurchStatus.Canceled).ToListAsync(ct);
            if (lines.All(x => x.RemainPurchFinancial <= 0 && x.RemainPurchPhysical <= 0))
                order.PurchStatus = PurchStatus.Invoiced;
            order.DocumentStatus = DocumentStatus.Invoice;
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { invoiceId = vendorInvoiceId, voucher = internalId, amount = total + totalTax, tax = totalTax, qty = totalQty },
                "Vendor invoice posted.");
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

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
