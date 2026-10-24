using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public sealed record CustReturnLineDto(long CustInvoiceTransRecId, decimal Qty, string DispositionCodeId);
public sealed record CustReturnCreateDto(string InvoiceId, string? ReturnReasonCodeId, List<CustReturnLineDto> Lines);
public sealed record CustReturnReceiptLineDto(long SalesLineRecId, decimal Qty);
public sealed record CustReturnReceiptDto(string RequestId, DateTime? ReceiptDate, List<CustReturnReceiptLineDto> Lines);
public sealed record CustCreditNoteDto(DateTime? InvoiceDate = null, string? RequestId = null);
public sealed record CustReturnResult(int StatusCode, object? Data = null, string? Message = null);

public interface ICustReturnService
{
    Task<CustReturnResult> CreateAsync(CustReturnCreateDto input, CancellationToken ct);
    Task<CustReturnResult> ReceiveAsync(long returnRecId, CustReturnReceiptDto input, CancellationToken ct);
    Task<CustReturnResult> CreditAsync(long returnRecId, CustCreditNoteDto input, CancellationToken ct);
}

/// <summary>
/// Customer return orders (CycleSalesorder.md Phase 10.3). A return order is a SalesTable with
/// SalesType = ReturnItem and an RMA number; each line is linked to the original invoiced line
/// (InventTransIdReturn) and carries a disposition code. Supported dispositions: Credit (goods are
/// received back into stock at the original issue cost, then credited) and Credit only (customer
/// is credited without a physical return). Entitlement is limited to the invoiced quantity not
/// already returned. The credit note reverses revenue, tax and — for received goods — cost of goods
/// sold, and credits the customer through CustTrans/CustTransOpen.
/// </summary>
public sealed class CustReturnService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
    ISysNumberSequenceService numbers, ISalesInventoryNumberService inventoryNumbers) : ICustReturnService
{
    private DbContext Db => unitOfWork.Context;

    // ---------------------------------------------------------------- Create return order

    public Task<CustReturnResult> CreateAsync(CustReturnCreateDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreateCoreAsync(input, ct));

    private async Task<CustReturnResult> CreateCoreAsync(CustReturnCreateDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        if (input.Lines == null || input.Lines.Count == 0) return new(400, null, "At least one return line is required.");
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var invoice = await Db.Set<CustInvoiceJour>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.InvoiceId == input.InvoiceId, ct);
            if (invoice == null) return await Fail(404, $"Invoice '{input.InvoiceId}' was not found in the selected company.", ct);
            var original = await Db.Set<SalesTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.SalesId == invoice.SalesId, ct);
            if (original == null) return await Fail(422, "The invoice's original sales order was not found.", ct);
            if (!string.IsNullOrWhiteSpace(input.ReturnReasonCodeId) && !await Db.Set<ReturnReasonCode>()
                    .AnyAsync(x => x.DataAreaId == area && x.ReasonCodeId == input.ReturnReasonCodeId, ct))
                return await Fail(422, $"Return reason code '{input.ReturnReasonCodeId}' is not configured.", ct);

            var order = new SalesTable
            {
                DataAreaId = area, SalesId = (await numbers.NextAsync("SalesTable", area, ct)).Code,
                ReturnItemNum = (await numbers.NextAsync("ReturnItemNum", area, ct)).Code,
                SalesType = SalesType.ReturnItem, SalesStatus = SalesStatus.Backorder, ReturnStatus = ReturnStatusHeader.Open,
                ReturnReasonCodeId = input.ReturnReasonCodeId ?? string.Empty,
                CustAccount = invoice.OrderAccount, InvoiceAccount = invoice.InvoiceAccount, CustGroup = original.CustGroup,
                CurrencyCode = invoice.CurrencyCode, PostingProfile = original.PostingProfile, PaymTerm = original.PaymTerm,
                PaymMode = original.PaymMode, TaxGroupId = original.TaxGroupId, InclTax = false,
                InventSiteId = original.InventSiteId, InventLocationId = original.InventLocationId,
                OrderDate = DateTime.UtcNow.Date, DefaultDimension = original.DefaultDimension
            };
            Db.Set<SalesTable>().Add(order);
            await unitOfWork.CompleteAsync(ct);

            var lineNum = 0m;
            foreach (var request in input.Lines)
            {
                var sold = await Db.Set<CustInvoiceTrans>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == request.CustInvoiceTransRecId
                    && x.DataAreaId == area && x.InvoiceId == invoice.InvoiceId, ct);
                if (sold == null) return await Fail(404, $"Invoice line {request.CustInvoiceTransRecId} was not found on invoice {invoice.InvoiceId}.", ct);
                if (request.Qty <= 0) return await Fail(422, "The return quantity must be greater than zero.", ct);
                if (sold.Qty <= 0) return await Fail(422, "Only invoiced sales lines can be returned.", ct);
                var disposition = await Db.Set<ReturnDispositionCode>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.DispositionCodeId == request.DispositionCodeId, ct);
                if (disposition == null) return await Fail(422, $"Disposition code '{request.DispositionCodeId}' is not configured.", ct);
                if (disposition.DispositionAction is not (DispositionAction.Credit or DispositionAction.CreditOnly))
                    return await Fail(422, $"Disposition action {disposition.DispositionAction} is outside the basic return scope (Credit and Credit only).", ct);

                // Entitlement: invoiced quantity on the original line minus quantities already on open/posted returns.
                var alreadyReturned = -(await Db.Set<SalesLine>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.InventTransIdReturn == sold.InventTransId
                        && x.SalesType == SalesType.ReturnItem && x.SalesStatus != SalesStatus.Canceled)
                    .Select(x => x.SalesQty).ToListAsync(ct)).Sum();
                var invoicedOnLine = (await Db.Set<CustInvoiceTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.InventTransId == sold.InventTransId && x.Qty > 0)
                    .Select(x => x.Qty).ToListAsync(ct)).Sum();
                if (request.Qty > invoicedOnLine - alreadyReturned)
                    return await Fail(422, $"Return quantity for item {sold.ItemId} exceeds the returnable quantity {invoicedOnLine - alreadyReturned}.", ct);

                var inventQty = decimal.Round(request.Qty * sold.InventQty / sold.Qty, 6);
                var unitRevenue = sold.LineAmount / sold.Qty;
                var line = new SalesLine
                {
                    DataAreaId = area, SalesId = order.SalesId, LineNum = ++lineNum, ItemId = sold.ItemId,
                    SalesType = SalesType.ReturnItem, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes,
                    SalesQty = -request.Qty, QtyOrdered = -inventQty, ExpectedRetQty = -request.Qty,
                    RemainSalesPhysical = -request.Qty, RemainSalesFinancial = -request.Qty,
                    RemainInventPhysical = -inventQty, RemainInventFinancial = -inventQty,
                    SalesUnit = sold.SalesUnit, SalesPrice = sold.SalesPrice, PriceUnit = sold.PriceUnit,
                    LineAmount = -decimal.Round(unitRevenue * request.Qty, 2, MidpointRounding.AwayFromZero),
                    CurrencyCode = sold.CurrencyCode, TaxGroup = sold.TaxGroup, TaxItemGroup = sold.TaxItemGroup,
                    InventDimId = sold.InventDimId, InventTransIdReturn = sold.InventTransId,
                    ReturnDispositionCodeId = disposition.DispositionCodeId, ReturnStatus = ReturnStatusLine.Awaiting,
                    CustAccount = order.CustAccount, CustGroupId = order.CustGroup
                };
                if (disposition.DispositionAction == DispositionAction.Credit)
                    await CreateExpectedReceiptAsync(area, order, line, inventQty, ct);
                Db.Set<SalesLine>().Add(line);
            }

            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { recId = order.RecId, salesId = order.SalesId, returnItemNum = order.ReturnItemNum }, "Return order created.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // Return line for a Credit disposition expects goods back: an Ordered receipt linked to the original issue.
    private async Task CreateExpectedReceiptAsync(string area, SalesTable order, SalesLine line, decimal inventQty, CancellationToken ct)
    {
        var originalOrigin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.InventTransId == line.InventTransIdReturn, ct)
            ?? throw new SubledgerPostingException($"The original inventory transaction of item {line.ItemId} was not found.");
        var transId = await inventoryNumbers.NextInventTransIdAsync(ct);
        var origin = new InventTransOrigin
        {
            DataAreaId = area, InventTransId = transId, ItemId = line.ItemId, ItemInventDimId = line.InventDimId,
            ReferenceCategory = InventRefType.SalesTable, ReferenceId = order.SalesId
        };
        Db.Set<InventTransOrigin>().Add(origin);
        await unitOfWork.CompleteAsync(ct);
        Db.Set<InventTransOriginSalesLine>().Add(new InventTransOriginSalesLine
        {
            InventTransOrigin = origin.RecId, SalesLineDataAreaId = area, SalesLineInventTransId = transId, DataAreaId = area
        });
        Db.Set<InventTrans>().Add(new InventTrans
        {
            DataAreaId = area, InventTransOrigin = origin.RecId, ItemId = line.ItemId, InventDimId = line.InventDimId,
            Qty = inventQty, StatusReceipt = StatusReceipt.Ordered, StatusIssue = StatusIssue.None, ValueOpen = InventTransOpen.Yes,
            CurrencyCode = line.CurrencyCode, DateExpected = DateTime.UtcNow.Date, ReturnInventTransOrigin = originalOrigin.RecId
        });
        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct)
            ?? throw new SubledgerPostingException($"Inventory summary is missing for item {line.ItemId}.");
        summary.Ordered += inventQty;
        summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
        line.InventTransId = transId;
    }

    // ---------------------------------------------------------------- Return receipt (packing slip)

    public Task<CustReturnResult> ReceiveAsync(long returnRecId, CustReturnReceiptDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReceiveCoreAsync(returnRecId, input, ct));

    private async Task<CustReturnResult> ReceiveCoreAsync(long returnRecId, CustReturnReceiptDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(input.RequestId)) return new(400, null, "A request ID is required.");
        if (input.Lines == null || input.Lines.Count == 0) return new(400, null, "At least one receipt line is required.");
        var date = (input.ReceiptDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await LoadReturnOrderAsync(area, returnRecId, ct);
            if (order == null) return await Fail(404, "Return order was not found in the selected company.", ct);
            await support.LockAsync($"ReturnReceipt:{area}:{order.SalesId}", ct);
            var previous = await Db.Set<CustPackingSlipJour>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                && x.SalesId == order.SalesId && x.ParmId == input.RequestId, ct);
            if (previous != null)
            {
                await unitOfWork.CommitTransactionAsync(ct);
                return new(200, new { packingSlipId = previous.PackingSlipId }, "Return receipt already posted for this request.");
            }

            var packingSlipId = (await numbers.NextAsync("CustPackingSlipJour", area, ct)).Code;
            var journal = new CustPackingSlipJour
            {
                DataAreaId = area, PackingSlipId = packingSlipId, InternalPackingSlipId = packingSlipId, SalesId = order.SalesId,
                DeliveryDate = date, ParmId = input.RequestId, OrderAccount = order.CustAccount, InvoiceAccount = order.InvoiceAccount,
                LedgerVoucher = packingSlipId
            };
            Db.Set<CustPackingSlipJour>().Add(journal);
            await unitOfWork.CompleteAsync(ct);

            foreach (var request in input.Lines)
            {
                var line = await Db.Set<SalesLine>().FirstOrDefaultAsync(x => x.RecId == request.SalesLineRecId
                    && x.DataAreaId == area && x.SalesId == order.SalesId, ct);
                if (line == null) return await Fail(404, $"Return line {request.SalesLineRecId} was not found.", ct);
                if (await ActionAsync(area, line, ct) != DispositionAction.Credit)
                    return await Fail(422, $"Return line {line.LineNum} is credit only and is not physically received.", ct);
                if (request.Qty <= 0 || request.Qty > -line.RemainSalesPhysical)
                    return await Fail(422, $"Receipt quantity for item {line.ItemId} must be positive and at most {-line.RemainSalesPhysical}.", ct);
                var inventQty = request.Qty == -line.RemainSalesPhysical
                    ? -line.RemainInventPhysical
                    : decimal.Round(request.Qty * line.QtyOrdered / line.SalesQty, 6);
                var unitCost = await OriginalUnitCostAsync(area, line, ct);

                var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.InventTransId == line.InventTransId && x.ReferenceId == order.SalesId, ct)
                    ?? throw new SubledgerPostingException($"Inventory origin is missing for return item {line.ItemId}.");
                var ordered = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
                    && x.StatusReceipt == StatusReceipt.Ordered).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
                if (ordered.Sum(x => x.Qty) < inventQty)
                    return await Fail(422, $"Expected return receipt does not cover item {line.ItemId}.", ct);
                var left = inventQty;
                foreach (var source in ordered)
                {
                    if (left <= 0) break;
                    var take = Math.Min(source.Qty, left);
                    if (take < source.Qty)
                        Db.Set<InventTrans>().Add(new InventTrans
                        {
                            DataAreaId = area, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                            InventDimId = source.InventDimId, Qty = source.Qty - take, StatusReceipt = StatusReceipt.Ordered,
                            StatusIssue = StatusIssue.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                            DateExpected = source.DateExpected, ReturnInventTransOrigin = source.ReturnInventTransOrigin
                        });
                    source.Qty = take;
                    source.StatusReceipt = StatusReceipt.Received;
                    source.DatePhysical = date;
                    source.DateStatus = date;
                    source.DateInvent = date;
                    source.PackingSlipId = packingSlipId;
                    source.VoucherPhysical = packingSlipId;
                    source.CostAmountPhysical = decimal.Round(take * unitCost, 2, MidpointRounding.AwayFromZero);
                    left -= take;
                }
                var cost = decimal.Round(inventQty * unitCost, 2, MidpointRounding.AwayFromZero);
                var summary = await Db.Set<InventSum>().SingleAsync(x => x.DataAreaId == area
                    && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                summary.Ordered -= inventQty;
                summary.Received += inventQty;
                summary.PhysicalInvent += inventQty;
                summary.PhysicalValue += cost;
                summary.AvailPhysical = summary.PhysicalInvent - summary.ReservPhysical;
                summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
                summary.LastUpdDatePhysical = date;

                line.RemainSalesPhysical += request.Qty;
                line.RemainInventPhysical = Math.Min(0m, line.RemainInventPhysical + inventQty);
                line.ReturnArrivalDate = date;
                if (line.RemainSalesPhysical >= 0)
                {
                    line.ReturnStatus = ReturnStatusLine.Received;
                    line.SalesStatus = SalesStatus.Delivered;
                }
                Db.Set<CustPackingSlipTrans>().Add(new CustPackingSlipTrans
                {
                    DataAreaId = area, PackingSlipId = packingSlipId, DeliveryDate = date, SalesId = order.SalesId,
                    OrigSalesId = order.SalesId, LineNum = line.LineNum, Name = line.Name, ItemId = line.ItemId,
                    SalesUnit = line.SalesUnit, StockedProduct = line.StockedProduct, InventDimId = line.InventDimId,
                    InventTransId = line.InventTransId, Qty = -request.Qty, Ordered = line.SalesQty, InventQty = -inventQty,
                    Remain = line.RemainSalesPhysical, RemainInvent = line.RemainInventPhysical, ParentRecId = journal.RecId,
                    FullyMatched = NoYes.No
                });
                journal.Qty -= request.Qty;
            }
            order.DocumentStatus = DocumentStatus.PackingSlip;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { packingSlipId, qty = -journal.Qty }, "Return receipt posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // ---------------------------------------------------------------- Credit note

    public Task<CustReturnResult> CreditAsync(long returnRecId, CustCreditNoteDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreditCoreAsync(returnRecId, input, ct));

    private async Task<CustReturnResult> CreditCoreAsync(long returnRecId, CustCreditNoteDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var date = (input.InvoiceDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await LoadReturnOrderAsync(area, returnRecId, ct);
            if (order == null) return await Fail(404, "Return order was not found in the selected company.", ct);
            await support.LockAsync($"CreditNote:{area}:{order.SalesId}", ct);
            if (!string.IsNullOrWhiteSpace(input.RequestId))
            {
                var previous = await Db.Set<CustInvoiceJour>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.SalesId == order.SalesId && x.ParmId == input.RequestId, ct);
                if (previous != null)
                {
                    await unitOfWork.CommitTransactionAsync(ct);
                    return new(200, new { invoiceId = previous.InvoiceId, invoiceAmount = previous.InvoiceAmount }, "Credit note already posted for this request.");
                }
            }
            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == order.InvoiceAccount, ct);
            if (customer == null) return await Fail(422, "The invoice customer was not found.", ct);

            // Creditable now: received-not-credited (Credit) and not-yet-credited (Credit only).
            var lines = await Db.Set<SalesLine>().Where(x => x.DataAreaId == area && x.SalesId == order.SalesId
                && x.SalesStatus != SalesStatus.Canceled).ToListAsync(ct);
            var selection = new List<(SalesLine Line, decimal Qty, bool Physical)>();
            foreach (var line in lines)
            {
                var action = await ActionAsync(area, line, ct);
                var qty = action == DispositionAction.Credit
                    ? line.RemainSalesPhysical - line.RemainSalesFinancial
                    : -line.RemainSalesFinancial;
                if (qty > 0) selection.Add((line, qty, action == DispositionAction.Credit));
            }
            if (selection.Count == 0) return await Fail(422, "There is nothing to credit: receive returned goods first, or the return is fully credited.", ct);

            var ledger = await support.LoadLedgerContextAsync(area, date, ct);
            var customerAccount = await support.ResolveCustomerSummaryAsync(area, order.PostingProfile, customer, ct);
            var invoiceId = (await numbers.NextAsync("CustInvoiceJour", area, ct)).Code;
            var entry = support.NewEntry(area, invoiceId, invoiceId, date, ledger);
            Db.Add(entry);
            await unitOfWork.CompleteAsync(ct);
            var currency = ledger.Ledger.AccountingCurrency;

            decimal totalRevenue = 0, totalTax = 0, totalQty = 0;
            var taxByCode = new Dictionary<string, (decimal Base, decimal Tax, string Group, string ItemGroup)>();
            foreach (var (line, qty, physical) in selection)
            {
                var inventQty = decimal.Round(qty * line.QtyOrdered / line.SalesQty, 6);
                var revenue = decimal.Round(-line.LineAmount * qty / -line.SalesQty, 2, MidpointRounding.AwayFromZero);
                var codes = await support.ApplicableTaxCodesAsync(area, line.TaxGroup, line.TaxItemGroup, date, ct);
                var lineTax = decimal.Round(revenue * codes.Sum(x => x.Rate) / 100m, 2, MidpointRounding.AwayFromZero);
                foreach (var (code, codeTax) in SubledgerPostingSupport.AllocateTax(codes, lineTax))
                {
                    var current = taxByCode.GetValueOrDefault(code);
                    taxByCode[code] = (current.Base + revenue, current.Tax + codeTax, line.TaxGroup, line.TaxItemGroup);
                }

                // Reverse revenue (debit).
                var revenueAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesRevenue, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, revenueAccount, LedgerPostingType.SalesRevenue, revenue,
                    $"Return {order.ReturnItemNum} item {line.ItemId}", inventQty, currency));

                if (physical)
                {
                    // Received goods are financially updated at their original cost: reverse COGS.
                    var cost = await ReceiptFinancialUpdateAsync(area, order, line, inventQty, invoiceId, date, ct);
                    if (cost != 0m)
                    {
                        var issueAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesIssue, ct);
                        var consumption = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesConsump, ct);
                        Db.AddRange(
                            SubledgerPostingSupport.AccountEntry(area, entry.RecId, issueAccount, LedgerPostingType.SalesIssue, cost,
                                $"Returned {line.ItemId} to inventory on {invoiceId}", inventQty, currency),
                            SubledgerPostingSupport.AccountEntry(area, entry.RecId, consumption, LedgerPostingType.SalesConsump, -cost,
                                $"Cost reversal for returned {line.ItemId} on {invoiceId}", -inventQty, currency));
                    }
                }

                Db.Set<CustInvoiceTrans>().Add(new CustInvoiceTrans
                {
                    DataAreaId = area, InvoiceId = invoiceId, InvoiceDate = date, SalesId = order.SalesId,
                    LineNum = line.LineNum, ItemId = line.ItemId, InventDimId = line.InventDimId,
                    InventTransId = line.InventTransId, Qty = -qty, QtyPhysical = physical ? -qty : 0m, InventQty = -inventQty,
                    SalesPrice = line.SalesPrice, PriceUnit = line.PriceUnit, SalesUnit = line.SalesUnit,
                    LineAmount = -revenue, LineAmountMst = -revenue, TaxAmount = -lineTax,
                    TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, CurrencyCode = order.CurrencyCode
                });
                line.RemainSalesFinancial += qty;
                line.RemainInventFinancial = Math.Min(0m, line.RemainInventFinancial + inventQty);
                if (line.RemainSalesFinancial >= 0)
                {
                    if (!physical) line.RemainSalesPhysical = 0m;      // credit only: nothing to receive
                    line.ReturnStatus = ReturnStatusLine.Invoiced;
                    line.SalesStatus = SalesStatus.Invoiced;
                    line.ReturnClosedDate = date;
                }
                totalRevenue += revenue;
                totalTax += lineTax;
                totalQty += qty;
            }

            foreach (var (code, value) in taxByCode)
            {
                if (value.Tax == 0m) continue;
                var payable = await support.ResolveTaxPayableAsync(area, code, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, payable, LedgerPostingType.Tax, value.Tax,
                    $"Sales tax {code} credit on {invoiceId}", 0m, currency));
                Db.Set<TaxTrans>().Add(SalesInvoiceService.TaxTransaction(area, date, invoiceId, code, value.Group, value.ItemGroup,
                    order.CurrencyCode, currency, value.Base, value.Tax));
            }

            var total = totalRevenue + totalTax;
            Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, customerAccount, LedgerPostingType.CustBalance, -total,
                $"Credit note {invoiceId}", 0m, currency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = invoiceId, VoucherDataAreaId = area, AccountingDate = date
            });
            var custTrans = new CustTrans
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, OrderAccount = order.CustAccount, TransDate = date,
                Voucher = invoiceId, Invoice = invoiceId, DocumentNum = order.ReturnItemNum, DocumentDate = date,
                Txt = $"Credit note for return {order.ReturnItemNum}", TransType = LedgerTransType.Sales,
                CurrencyCode = order.CurrencyCode, AmountCur = -total, AmountMst = -total, ReportingCurrencyAmount = -total,
                DueDate = date, PostingProfile = order.PostingProfile, DefaultDimension = order.DefaultDimension
            };
            Db.Set<CustTrans>().Add(custTrans);
            await unitOfWork.CompleteAsync(ct);
            Db.Set<CustTransOpen>().Add(new CustTransOpen
            {
                DataAreaId = area, AccountNum = order.InvoiceAccount, RefRecId = custTrans.RecId, TransDate = date,
                DueDate = date, AmountCur = -total, AmountMst = -total, ReportingCurrencyAmount = -total
            });
            Db.Set<CustInvoiceJour>().Add(new CustInvoiceJour
            {
                DataAreaId = area, InvoiceId = invoiceId, InvoiceDate = date, SalesId = order.SalesId,
                OrderAccount = order.CustAccount, InvoiceAccount = order.InvoiceAccount, CustGroup = order.CustGroup,
                CurrencyCode = order.CurrencyCode, LedgerVoucher = invoiceId, ParmId = input.RequestId ?? string.Empty,
                Qty = -totalQty, SalesBalance = -totalRevenue, SumTax = -totalTax, InvoiceAmount = -total,
                SalesBalanceMst = -totalRevenue, SumTaxMst = -totalTax, InvoiceAmountMst = -total,
                PostingProfile = order.PostingProfile, DueDate = date
            });

            if (lines.All(x => x.SalesStatus == SalesStatus.Invoiced))
            {
                order.SalesStatus = SalesStatus.Invoiced;
                order.ReturnStatus = ReturnStatusHeader.Closed;
            }
            order.DocumentStatus = DocumentStatus.Invoice;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { invoiceId, creditAmount = total, revenue = totalRevenue, tax = totalTax }, "Credit note posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // Received -> Purchased for returned goods at the original issue cost; returns the restored inventory value.
    private async Task<decimal> ReceiptFinancialUpdateAsync(string area, SalesTable order, SalesLine line, decimal inventQty,
        string invoiceId, DateTime date, CancellationToken ct)
    {
        var origin = await Db.Set<InventTransOrigin>().FirstAsync(x => x.DataAreaId == area
            && x.InventTransId == line.InventTransId && x.ReferenceId == order.SalesId, ct);
        var received = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
            && x.StatusReceipt == StatusReceipt.Received).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
        if (received.Sum(x => x.Qty) < inventQty)
            throw new SubledgerPostingException($"Received return quantity does not cover the credit for item {line.ItemId}.");
        var unitCost = await OriginalUnitCostAsync(area, line, ct);
        var left = inventQty;
        decimal physicalReleased = 0m, cost = 0m;
        foreach (var source in received)
        {
            if (left <= 0) break;
            var take = Math.Min(source.Qty, left);
            var physicalPerUnit = source.Qty == 0 ? 0m : source.CostAmountPhysical / source.Qty;
            if (take < source.Qty)
                Db.Set<InventTrans>().Add(new InventTrans
                {
                    DataAreaId = area, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                    InventDimId = source.InventDimId, Qty = source.Qty - take, StatusReceipt = StatusReceipt.Received,
                    StatusIssue = StatusIssue.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                    DateExpected = source.DateExpected, DatePhysical = source.DatePhysical, DateStatus = source.DateStatus,
                    DateInvent = source.DateInvent, PackingSlipId = source.PackingSlipId, VoucherPhysical = source.VoucherPhysical,
                    CostAmountPhysical = (source.Qty - take) * physicalPerUnit, ReturnInventTransOrigin = source.ReturnInventTransOrigin
                });
            var portionCost = decimal.Round(take * unitCost, 2, MidpointRounding.AwayFromZero);
            physicalReleased += take * physicalPerUnit;
            cost += portionCost;
            source.Qty = take;
            source.CostAmountPhysical = take * physicalPerUnit;
            source.StatusReceipt = StatusReceipt.Purchased;
            source.DateFinancial = date;
            source.DateStatus = date;
            source.InvoiceId = invoiceId;
            source.Voucher = invoiceId;
            source.CostAmountPosted = portionCost;
            left -= take;
        }
        var summary = await Db.Set<InventSum>().SingleAsync(x => x.DataAreaId == area
            && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
        summary.Received -= inventQty;
        summary.PostedQty += inventQty;
        summary.PhysicalValue -= physicalReleased;
        summary.PostedValue += cost;
        return cost;
    }

    // Returned goods carry the cost of the original issue they reverse (D365 return marking).
    private async Task<decimal> OriginalUnitCostAsync(string area, SalesLine line, CancellationToken ct)
    {
        var originalOrigin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.InventTransId == line.InventTransIdReturn, ct)
            ?? throw new SubledgerPostingException($"The original inventory transaction of item {line.ItemId} was not found.");
        var sold = await Db.Set<InventTrans>().AsNoTracking().Where(x => x.DataAreaId == area
            && x.InventTransOrigin == originalOrigin.RecId && x.StatusIssue == StatusIssue.Sold)
            .Select(x => new { x.Qty, x.CostAmountPosted, x.CostAmountAdjustment }).ToListAsync(ct);
        var qty = -sold.Sum(x => x.Qty);
        if (qty <= 0) throw new SubledgerPostingException($"The original issue of item {line.ItemId} has not been invoiced.");
        return -sold.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) / qty;
    }

    private async Task<DispositionAction> ActionAsync(string area, SalesLine line, CancellationToken ct) =>
        await Db.Set<ReturnDispositionCode>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.DispositionCodeId == line.ReturnDispositionCodeId)
            .Select(x => x.DispositionAction).FirstOrDefaultAsync(ct);

    private Task<SalesTable?> LoadReturnOrderAsync(string area, long recId, CancellationToken ct) =>
        Db.Set<SalesTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area
            && x.SalesType == SalesType.ReturnItem, ct);

    private async Task<CustReturnResult> Fail(int status, string message, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
