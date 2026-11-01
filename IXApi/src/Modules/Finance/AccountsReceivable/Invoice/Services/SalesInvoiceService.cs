using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

/// <summary>
/// Sales invoice posting (CycleSalesorder.md Phase 8 / ARTables.md §8). Invoices delivered,
/// uninvoiced quantities only (packing-slip-based policy) and supports partial invoicing.
/// One voucher posts, from configuration:
///   Dr Customer summary (CustLedgerAccounts)                net + tax
///   Cr Sales revenue  (InventPosting SalesRevenue)          net
///   Cr Sales tax payable (TaxLedgerAccountGroup)            tax
///   Dr Cost of units invoiced (InventPosting SalesConsump)  COGS
///   Cr Inventory issue        (InventPosting SalesIssue)    COGS
/// and writes CustInvoiceJour/Trans, CustTrans + CustTransOpen, TaxTrans, Deducted -> Sold
/// inventory portions, and the line's financial remainders. COGS is the provisional running
/// average; the FIFO inventory close settles it to actual cost.
/// </summary>
public sealed class SalesInvoiceService : BaseService<CustInvoiceJour>, ISalesInvoiceService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public SalesInvoiceService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public SalesInvoiceService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<SalesInvoiceResult> PostAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(salesRecId, input, ct));

    private async Task<SalesInvoiceResult> PostCoreAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var date = (input.InvoiceDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<SalesTable>().FirstOrDefaultAsync(x => x.RecId == salesRecId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Sales order was not found in the selected company.", ct);
            if (order.SalesType == SalesType.ReturnItem)
                return await Fail(422, "Return orders are credited through the return order credit note.", ct);
            await support.LockAsync($"SalesInvoice:{area}:{order.SalesId}", ct);

            if (!string.IsNullOrWhiteSpace(input.RequestId))
            {
                var previous = await Db.Set<CustInvoiceJour>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.SalesId == order.SalesId && x.ParmId == input.RequestId, ct);
                if (previous != null)
                {
                    await _unitOfWork.CommitTransactionAsync(ct);
                    return new(200, Summary(previous), "Sales invoice already posted for this request.");
                }
            }

            var invoiceAccount = string.IsNullOrWhiteSpace(order.InvoiceAccount) ? order.CustAccount : order.InvoiceAccount;
            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == invoiceAccount, ct);
            if (customer == null) return await Fail(422, "The invoice customer was not found.", ct);
            if (customer.Blocked is CustVendorBlocked.All or CustVendorBlocked.Invoicing)
                return await Fail(422, "The invoice customer is blocked for invoicing.", ct);

            var dueDate = date;
            if (!string.IsNullOrWhiteSpace(order.PaymTerm))
            {
                var term = await Db.Set<PaymTerm>().AsNoTracking().SingleOrDefaultAsync(
                    x => x.DataAreaId == area && x.PaymTermId == order.PaymTerm, ct);
                if (term == null) return await Fail(422, "The sales order payment terms were not found in this company.", ct);
                try { dueDate = IAX.IXApi.Modules.Finance.Foundation.PaymentTerms.PaymentDueDateCalculator.Calculate(date, term); }
                catch (InvalidOperationException ex) { return await Fail(422, ex.Message, ct); }
            }
            var lines = await Db.Set<SalesLine>().Where(x => x.DataAreaId == area && x.SalesId == order.SalesId).ToListAsync(ct);
            var selection = new List<(SalesLine Line, decimal Qty)>();
            if (input.Lines == null || input.Lines.Count == 0)
            {
                selection.AddRange(lines.Where(x => x.RemainSalesFinancial - x.RemainSalesPhysical > 0)
                    .Select(x => (x, x.RemainSalesFinancial - x.RemainSalesPhysical)));
            }
            else
            {
                foreach (var request in input.Lines)
                {
                    var line = lines.FirstOrDefault(x => x.RecId == request.SalesLineRecId);
                    if (line == null) return await Fail(404, $"Sales line {request.SalesLineRecId} was not found on this order.", ct);
                    if (request.Qty <= 0) return await Fail(422, "The invoice quantity must be greater than zero.", ct);
                    var deliveredNotInvoiced = line.RemainSalesFinancial - line.RemainSalesPhysical;
                    if (request.Qty > deliveredNotInvoiced)
                        return await Fail(422, $"Invoice quantity for item {line.ItemId} exceeds the delivered, uninvoiced quantity {deliveredNotInvoiced}.", ct);
                    selection.Add((line, request.Qty));
                }
            }
            if (selection.Count == 0) return await Fail(422, "There is no delivered, uninvoiced quantity to invoice.", ct);

            var ledger = await support.LoadLedgerContextAsync(area, date,
                LedgerPostingModule.Customer, _currentUser.GetCurrentUserId(), ct);
            if (string.IsNullOrWhiteSpace(order.CurrencyCode))
                return await Fail(422, "The sales order has no currency.", ct);
            // The order amounts are translated into the ledger accounting and reporting currencies
            // at the invoice date rate, resolved once so every line of the voucher uses the same
            // rate. Inventory cost is already an accounting-currency amount, so the cost of goods
            // lines below are posted in that currency and are not translated.
            var translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, ledger.Ledger,
                order.CurrencyCode, date, ct);
            var customerAccount = await support.ResolveCustomerSummaryAsync(area, order.PostingProfile, customer, ct);
            var invoiceId = (await _numbers.NextAsync("CustInvoiceJour", area, ct)).Code;
            var entry = support.NewEntry(area, invoiceId, invoiceId, date, ledger);
            Db.Add(entry);
            await _unitOfWork.CompleteAsync(ct);
            var currency = ledger.Ledger.AccountingCurrency;

            decimal totalRevenue = 0, totalTax = 0, totalQty = 0;
            var taxByCode = new Dictionary<string, (decimal Base, decimal Tax, string Group, string ItemGroup)>();
            foreach (var (line, qty) in selection)
            {
                var boundaryError = await IAX.IXApi.Modules.Finance.Inventory.InventoryFinancialBoundary.CheckAsync(Db, area, line.ItemId, date, ct);
                if (boundaryError != null) return await Fail(422, boundaryError, ct);
                var inventQty = qty == line.RemainSalesFinancial
                    ? line.RemainInventFinancial
                    : decimal.Round(qty * line.QtyOrdered / (line.SalesQty == 0 ? 1 : line.SalesQty), 6);
                var ratio = line.SalesQty == 0 ? 0m : qty / line.SalesQty;
                var net = decimal.Round((line.LineAmount - line.LineDisc - line.MultiLnDisc) * ratio, 2, MidpointRounding.AwayFromZero);

                var codes = await support.ApplicableTaxCodesAsync(area, line.TaxGroup, line.TaxItemGroup, date, ct);
                var rate = codes.Sum(x => x.Rate);
                var lineTax = order.InclTax && rate > 0
                    ? decimal.Round(net * rate / (100m + rate), 2, MidpointRounding.AwayFromZero)
                    : decimal.Round(net * rate / 100m, 2, MidpointRounding.AwayFromZero);
                var revenue = order.InclTax ? net - lineTax : net;
                foreach (var (code, codeTax) in SubledgerPostingSupport.AllocateTax(codes, lineTax))
                {
                    var current = taxByCode.GetValueOrDefault(code);
                    taxByCode[code] = (current.Base + revenue, current.Tax + codeTax, line.TaxGroup, line.TaxItemGroup);
                }

                var (cogs, physicalCost, inventOrigin) = await IssueInventoryAsync(area, order, line, inventQty, invoiceId, date, ct);
                var customerScope = await support.CustomerScopeAsync(area,
                    order.InvoiceAccount.Length > 0 ? order.InvoiceAccount : order.CustAccount, ct);
                var revenueAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesRevenue, ct,
                    custVendAccount: customerScope.Account, custVendGroup: customerScope.Group);
                Db.Add(translation.AccountEntry(area, entry.RecId, revenueAccount, LedgerPostingType.SalesRevenue, -revenue,
                    $"Sales {order.SalesId} item {line.ItemId}", -inventQty));
                if (cogs != 0m)
                {
                    var consumption = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesConsump, ct,
                        custVendAccount: customerScope.Account, custVendGroup: customerScope.Group);
                    var issueAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesIssue, ct,
                        custVendAccount: customerScope.Account, custVendGroup: customerScope.Group);
                    Db.AddRange(
                        SubledgerPostingSupport.AccountEntry(area, entry.RecId, consumption, LedgerPostingType.SalesConsump, cogs,
                            $"Cost of {line.ItemId} invoiced on {invoiceId}", inventQty, currency),
                        SubledgerPostingSupport.AccountEntry(area, entry.RecId, issueAccount, LedgerPostingType.SalesIssue, -cogs,
                            $"Inventory issue {line.ItemId} invoiced on {invoiceId}", -inventQty, currency));
                }
                // The financial update takes over from the delivery, so its physical posting is reversed.
                await new PackingSlip.PackingSlipPhysicalPosting(Db)
                    .ReverseAsync(area, inventOrigin, entry.RecId, date, currency, physicalCost, ct);

                Db.Set<CustInvoiceTrans>().Add(new CustInvoiceTrans
                {
                    DataAreaId = area, InvoiceId = invoiceId, InvoiceDate = date, SalesId = order.SalesId,
                    LineNum = line.LineNum, ItemId = line.ItemId, InventDimId = line.InventDimId,
                    InventTransId = line.InventTransId, Qty = qty, QtyPhysical = qty, InventQty = inventQty,
                    SalesPrice = line.SalesPrice, PriceUnit = line.PriceUnit, SalesUnit = line.SalesUnit,
                    LineAmount = revenue, LineAmountMst = translation.ToAccounting(revenue), TaxAmount = lineTax,
                    TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, CurrencyCode = order.CurrencyCode
                });

                line.RemainSalesFinancial -= qty;
                line.RemainInventFinancial = Math.Max(0m, line.RemainInventFinancial - inventQty);
                if (line.RemainSalesFinancial <= 0 && line.RemainSalesPhysical <= 0) line.SalesStatus = SalesStatus.Invoiced;
                totalRevenue += revenue;
                totalTax += lineTax;
                totalQty += qty;
            }

            // Order and line charges post here, before tax, so a charge billed to the customer is
            // part of the invoice total and its tax joins the rest of the voucher's tax.
            var charges = await new SalesChargePosting(Db).PostAsync(area, order, entry.RecId, invoiceId,
                date, translation, support, customerAccount, ct);
            foreach (var (code, chargeBase, chargeTax, group, itemGroup) in charges.TaxByCode)
            {
                var current = taxByCode.GetValueOrDefault(code);
                taxByCode[code] = (current.Base + chargeBase, current.Tax + chargeTax,
                    current.Group.Length > 0 ? current.Group : group,
                    current.ItemGroup.Length > 0 ? current.ItemGroup : itemGroup);
            }
            totalTax += charges.Tax;

            foreach (var (code, value) in taxByCode)
            {
                if (value.Tax == 0m) continue;
                var payable = await support.ResolveTaxPayableAsync(area, code, ct);
                Db.Add(translation.AccountEntry(area, entry.RecId, payable, LedgerPostingType.Tax, -value.Tax,
                    $"Sales tax {code} on {invoiceId}", 0m));
                Db.Set<TaxTrans>().Add(TaxTransaction(area, date, invoiceId, code, value.Group, value.ItemGroup,
                    order.CurrencyCode, currency, -value.Base, -value.Tax,
                    translation.ToAccounting(-value.Base), translation.ToAccounting(-value.Tax),
                    translation.ToReporting(-value.Base), translation.ToReporting(-value.Tax)));
            }

            // Charges debited to the customer are part of what the customer owes; charges the
            // company absorbs posted to their own accounts above and are not in the total.
            var total = totalRevenue + charges.BilledToCustomer + totalTax;
            Db.Add(translation.AccountEntry(area, entry.RecId, customerAccount, LedgerPostingType.CustBalance, total,
                $"Customer invoice {invoiceId}", 0m));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = invoiceId, VoucherDataAreaId = area, AccountingDate = date
            });
            await AddCustomerTransactionAsync(area, order, invoiceAccount, invoiceId, date, dueDate, total, LedgerTransType.Sales,
                $"Sales order {order.SalesId}", translation, ct);

            var journal = new CustInvoiceJour
            {
                DataAreaId = area, InvoiceId = invoiceId, InvoiceDate = date, SalesId = order.SalesId,
                OrderAccount = order.CustAccount, InvoiceAccount = invoiceAccount, CustGroup = order.CustGroup,
                CurrencyCode = order.CurrencyCode, LedgerVoucher = invoiceId, ParmId = input.RequestId ?? string.Empty,
                Qty = totalQty, SalesBalance = totalRevenue, SumTax = totalTax, InvoiceAmount = total,
                SalesBalanceMst = translation.ToAccounting(totalRevenue), SumTaxMst = translation.ToAccounting(totalTax),
                InvoiceAmountMst = translation.ToAccounting(total),
                SumMarkup = charges.BilledToCustomer, SumMarkupMst = translation.ToAccounting(charges.BilledToCustomer),
                Payment = order.PaymTerm, PostingProfile = order.PostingProfile, DueDate = dueDate
            };
            Db.Set<CustInvoiceJour>().Add(journal);

            if (lines.All(x => x.SalesStatus == SalesStatus.Invoiced || x.SalesStatus == SalesStatus.Canceled))
                order.SalesStatus = SalesStatus.Invoiced;
            order.DocumentStatus = DocumentStatus.Invoice;
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, Summary(journal), "Sales invoice posted.");
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

    // Financial issue: Deducted -> Sold for the invoiced inventory quantity at provisional running-average cost.
    private async Task<(decimal Cogs, decimal PhysicalCost, long InventTransOrigin)> IssueInventoryAsync(
        string area, SalesTable order, SalesLine line, decimal inventQty, string invoiceId, DateTime date, CancellationToken ct)
    {
        var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == area
            && x.InventTransId == line.InventTransId && x.ReferenceId == order.SalesId
            && x.ReferenceCategory == InventRefType.SalesTable, ct)
            ?? throw new SubledgerPostingException($"Inventory origin is missing for item {line.ItemId}.");
        var deducted = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
            && x.StatusIssue == StatusIssue.Deducted).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
        if (deducted.Sum(x => -x.Qty) < inventQty)
            throw new SubledgerPostingException($"Delivered inventory does not cover the invoice for item {line.ItemId}.");

        var site = await Db.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == area && x.InventDimId == line.InventDimId)
            .Select(x => x.InventSiteId).FirstOrDefaultAsync(ct) ?? string.Empty;
        var unitCost = await IAX.IXApi.Modules.Finance.Inventory.InventoryMovement.SiteRunningAverageCostAsync(Db, area, line.ItemId, site, ct);
        var left = inventQty;
        var physicalReleased = 0m;
        foreach (var source in deducted)
        {
            if (left <= 0) break;
            var available = -source.Qty;
            var take = Math.Min(available, left);
            if (take < available)
                Db.Set<InventTrans>().Add(new InventTrans
                {
                    DataAreaId = source.DataAreaId, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                    InventDimId = source.InventDimId, Qty = -(available - take), StatusIssue = StatusIssue.Deducted,
                    StatusReceipt = StatusReceipt.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                    DateExpected = source.DateExpected, DatePhysical = source.DatePhysical, DateStatus = source.DateStatus,
                    DateInvent = source.DateInvent, PackingSlipId = source.PackingSlipId, VoucherPhysical = source.VoucherPhysical
                });
            source.Qty = -take;
            source.StatusIssue = StatusIssue.Sold;
            source.DateFinancial = date;
            source.DateStatus = date;
            source.InvoiceId = invoiceId;
            source.Voucher = invoiceId;
            source.CostAmountPosted = -decimal.Round(take * unitCost, 2, MidpointRounding.AwayFromZero);
            physicalReleased += -source.CostAmountPhysical;
            source.CostAmountPhysical = 0m;
            left -= take;
        }
        var cogs = decimal.Round(inventQty * unitCost, 2, MidpointRounding.AwayFromZero);
        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct)
            ?? throw new SubledgerPostingException($"Inventory summary is missing for item {line.ItemId}.");
        summary.Deducted -= inventQty;   // no longer delivered-not-invoiced
        summary.PostedQty -= inventQty;  // financially issued (physical already reduced at packing slip)
        summary.PostedValue -= cogs;
        summary.PhysicalValue += physicalReleased; // the delivery's provisional value is handed over
        summary.LastUpdDatePhysical = date;
        return (cogs, physicalReleased, origin.RecId);
    }


    private async Task AddCustomerTransactionAsync(string area, SalesTable order, string invoiceAccount, string invoiceId,
        DateTime date, DateTime dueDate, decimal amount, LedgerTransType type, string text,
        SubledgerCurrencyTranslation translation, CancellationToken ct)
    {
        // AmountCur is what the customer owes in the order currency; AmountMst and the reporting
        // amount are the same debt translated, which is what the ledger and reports read.
        var accountingAmount = translation.ToAccounting(amount);
        var reportingAmount = translation.ToReporting(amount);
        var custTrans = new CustTrans
        {
            DataAreaId = area, AccountNum = invoiceAccount, OrderAccount = order.CustAccount, TransDate = date,
            Voucher = invoiceId, Invoice = invoiceId, DocumentNum = invoiceId, DocumentDate = date, Txt = text,
            TransType = type, CurrencyCode = order.CurrencyCode, AmountCur = amount, AmountMst = accountingAmount,
            ReportingCurrencyAmount = reportingAmount, DueDate = dueDate, PaymTermId = order.PaymTerm, PaymMode = order.PaymMode,
            PostingProfile = order.PostingProfile, DefaultDimension = order.DefaultDimension,
            // The order's cash discount code travels to the transaction, which is where settlement
            // reads it from. The base date is the invoice date unless the order set its own.
            CashDiscCode = order.CashDisc,
            CashDiscBaseDate = order.CashDiscBaseDate == default ? date : order.CashDiscBaseDate
        };
        Db.Set<CustTrans>().Add(custTrans);
        await _unitOfWork.CompleteAsync(ct);

        // The open row carries the discount it could earn, so it can be read without re-deriving
        // it. Settlement resolves the discount itself through the same resolver, so this is
        // reporting detail rather than the figure settlement depends on.
        var terms = await Foundation.CashDiscount.CashDiscountResolver.ResolveAsync(Db, area,
            custTrans.CashDiscCode, custTrans.CashDiscBaseDate, amount, ct);
        Db.Set<CustTransOpen>().Add(new CustTransOpen
        {
            DataAreaId = area, AccountNum = invoiceAccount, RefRecId = custTrans.RecId, TransDate = date,
            DueDate = dueDate, AmountCur = amount, AmountMst = accountingAmount, ReportingCurrencyAmount = reportingAmount,
            CashDiscDate = terms.Applies ? terms.DiscountDate : default,
            PossibleCashDisc = terms.PossibleAmount,
            CashDiscountLedgerDimension = terms.Discount?.CashDiscLedgerDimension ?? 0
        });
    }

    /// <summary>
    /// The posted tax row. The Cur amounts are in the document currency; the unsuffixed and Rep
    /// amounts are the accounting and reporting translations, which is what tax settlement sums.
    /// They default to the document amounts so a same-currency caller is unchanged.
    /// </summary>
    internal static TaxTrans TaxTransaction(string area, DateTime date, string voucher, string code, string group,
        string itemGroup, string sourceCurrency, string currency, decimal baseAmount, decimal taxAmount,
        decimal? accountingBase = null, decimal? accountingTax = null,
        decimal? reportingBase = null, decimal? reportingTax = null) => new()
    {
        DataAreaId = area, TransDate = date, Voucher = voucher, TaxCode = code, TaxGroup = group, TaxItemGroup = itemGroup,
        TaxDirection = TaxDirection.Outgoing, SourceCurrencyCode = sourceCurrency, CurrencyCode = currency,
        SourceBaseAmountCur = baseAmount, SourceTaxAmountCur = taxAmount, TaxBaseAmountCur = baseAmount, TaxAmountCur = taxAmount,
        TaxBaseAmount = accountingBase ?? baseAmount, TaxAmount = accountingTax ?? taxAmount,
        TaxBaseAmountRep = reportingBase ?? baseAmount, TaxAmountRep = reportingTax ?? taxAmount
    };

    private static object Summary(CustInvoiceJour journal) => new
    {
        invoiceId = journal.InvoiceId, salesId = journal.SalesId, invoiceDate = journal.InvoiceDate,
        qty = journal.Qty, salesBalance = journal.SalesBalance, sumTax = journal.SumTax, invoiceAmount = journal.InvoiceAmount
    };

    private async Task<SalesInvoiceResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
