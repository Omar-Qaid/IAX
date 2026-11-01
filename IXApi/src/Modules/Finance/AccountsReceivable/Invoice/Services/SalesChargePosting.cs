using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

/// <summary>
/// Posts a sales order's charges (MarkupTrans) when the order is invoiced.
///
/// Charges were configured, attached to orders and then dropped: no posting path referenced
/// MarkupTrans, and CustInvoiceJour.SumMarkup was never assigned, so a freight charge entered on an
/// order vanished from both the invoice and the ledger.
///
/// A charge code carries a debit side and a credit side — despite the Cust/Vend field prefixes,
/// which are historical. In D365 terms CustType/CustPosting/CustomerLedgerDimension are the
/// **debit** type, posting and account, and VendType/VendPosting/VendorLedgerDimension are the
/// **credit** ones. The type selects where that side goes:
///
/// - Ledger account (0) — the side posts to that side's own ledger dimension.
/// - Customer or vendor (1) — the side posts to the customer's summary account, which is what
///   bills the charge to the customer and puts it in the invoice total.
/// - Item (2) — the charge is added to the item cost. Sales does not allow it, and landed cost is
///   outside basic scope, so it is refused rather than approximated.
///
/// The documented sales pattern is debit Customer, credit Ledger account: the customer is billed
/// and a charge account is credited. A charge with both sides on ledger accounts is one the company
/// absorbs — it posts, but it does not touch the customer balance or the invoice total.
///
/// Each charge posts **once, in full**, recorded in MarkupTrans.Posted so a later partial invoice
/// of the same order does not bill it again. Prorating a charge across partial invoices
/// (CalculatedProratedAmount, IsAdvancedLineProrated) is a separate D365 feature and is not done
/// here.
/// </summary>
internal sealed class SalesChargePosting(DbContext db)
{
    /// <summary>The debit or credit side posts to that side's ledger dimension.</summary>
    private const int TypeLedgerAccount = 0;
    /// <summary>The side posts to the customer or vendor account.</summary>
    private const int TypeCustomerVendor = 1;
    /// <summary>The charge is added to the item cost. Not a sales charge, and not basic scope.</summary>
    private const int TypeItem = 2;

    /// <summary>
    /// What the posted charges add to the invoice. <paramref name="BilledToCustomer"/> is the part
    /// that reaches the customer balance, which is what SumMarkup and the invoice total carry.
    /// </summary>
    internal sealed record Result(decimal BilledToCustomer, decimal Tax,
        IReadOnlyList<(string Code, decimal Base, decimal Tax, string Group, string ItemGroup)> TaxByCode);

    public async Task<Result> PostAsync(string area, SalesTable order, long entryRecId, string invoiceId,
        DateTime date, SubledgerCurrencyTranslation translation, SubledgerPostingSupport support,
        DimensionAttributeValueCombination customerAccount, CancellationToken ct)
    {
        var charges = await UnpostedChargesAsync(area, order, ct);
        if (charges.Count == 0) return new(0m, 0m, []);

        var codes = await db.Set<MarkupTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area).ToListAsync(ct);
        var billed = 0m;
        var totalTax = 0m;
        var taxByCode = new List<(string, decimal, decimal, string, string)>();

        foreach (var (charge, amount) in charges)
        {
            if (amount == 0m) continue;
            var code = codes.FirstOrDefault(x => x.MarkupCode == charge.MarkupCode)
                ?? throw new SubledgerPostingException(
                    $"Charges code '{charge.MarkupCode}' does not exist in this company.");
            EnsureInScope(code);

            var text = charge.Txt.Length > 0 ? charge.Txt : $"Charge {charge.MarkupCode} on {invoiceId}";

            // The debit side. When it is the customer, no entry is written here: the invoice posts
            // one summary customer entry, and the amount is folded into it through BilledToCustomer.
            // Writing it twice would debit the customer for the charge twice over.
            if (code.CustType == TypeLedgerAccount)
            {
                var debit = await LedgerSideAsync(area, code, code.CustomerLedgerDimension, "debit", support, ct);
                db.Add(translation.AccountEntry(area, entryRecId, debit,
                    LedgerPostingType.LedgerJournal, amount, text, 0m));
            }

            // The credit side is always a ledger account on a sales charge.
            var credit = await LedgerSideAsync(area, code, code.VendorLedgerDimension, "credit", support, ct);
            db.Add(translation.AccountEntry(area, entryRecId, credit,
                LedgerPostingType.LedgerJournal, -amount, text, 0m));

            if (code.CustType == TypeCustomerVendor)
            {
                billed += amount;
                var tax = await ChargeTaxAsync(area, order, code, charge, amount, date, support, taxByCode, ct);
                totalTax += tax;
                charge.TaxAmount = tax;
            }

            charge.Posted = amount;
            charge.Voucher = invoiceId;
            charge.TransDate = date;
            charge.DocumentStatus = DocumentStatus.Invoice;
        }
        return new(billed, totalTax, taxByCode);
    }

    /// <summary>
    /// The order's and its lines' charges that are not yet posted, with the amount each is worth.
    /// The amount is recomputed from the same basis the order's charge refresh uses, so it does not
    /// depend on that refresh having run.
    /// </summary>
    private async Task<List<(MarkupTrans Charge, decimal Amount)>> UnpostedChargesAsync(
        string area, SalesTable order, CancellationToken ct)
    {
        var lines = await db.Set<SalesLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.SalesId == order.SalesId)
            .Select(x => new { x.RecId, x.SalesQty, x.LineAmount, x.LineDisc, x.MultiLnDisc })
            .ToListAsync(ct);
        var lineIds = lines.Select(x => x.RecId).ToList();
        var charges = await db.Set<MarkupTrans>()
            .Where(x => x.DataAreaId == area
                && !x.IsDeleted
                && x.Posted == 0m
                && (x.ModuleType == MarkupModuleType.Customer || x.ModuleType == MarkupModuleType.Sales)
                && ((x.TransRecId == order.RecId
                        && (x.TransTableId == FinanceTableNumbers.SalesTable || x.TransTableId == FinanceTableNumbers.Unspecified))
                    || (lineIds.Contains(x.TransRecId) && x.TransTableId == FinanceTableNumbers.SalesLine)))
            .ToListAsync(ct);

        var byLine = lines.ToDictionary(x => x.RecId,
            x => (x.SalesQty, Net: Math.Max(0m, x.LineAmount - x.LineDisc - x.MultiLnDisc)));
        var orderQuantity = lines.Sum(x => x.SalesQty);
        var orderNet = byLine.Values.Sum(x => x.Net);
        return charges.OrderBy(x => x.LineNum).ThenBy(x => x.RecId).Select(charge =>
        {
            var basis = charge.TransTableId == FinanceTableNumbers.SalesLine
                && byLine.TryGetValue(charge.TransRecId, out var line)
                ? line
                : (SalesQty: orderQuantity, Net: orderNet);
            return (charge, MarkupChargeCalculator.CalculateForBase(charge, basis.SalesQty, basis.Net));
        }).ToList();
    }

    /// <summary>
    /// Refuses the charge configurations this path does not post, rather than approximating them.
    /// </summary>
    private static void EnsureInScope(MarkupTable code)
    {
        if (code.CustType == TypeItem || code.VendType == TypeItem)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' adds the charge to the item cost. Sales charges do not allow "
                + "the Item type, and landed cost is not posted by this path.");
        if (code.VendType == TypeCustomerVendor)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' credits a customer or vendor account. A sales charge credits a "
                + "ledger account; configure its credit side as a ledger account.");
        if (code.CustType is not (TypeLedgerAccount or TypeCustomerVendor))
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' has an unrecognised debit type '{code.CustType}'.");
        if (code.VendType != TypeLedgerAccount)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' has an unrecognised credit type '{code.VendType}'.");
    }

    private static async Task<DimensionAttributeValueCombination> LedgerSideAsync(string area, MarkupTable code,
        long? ledgerDimension, string side, SubledgerPostingSupport support, CancellationToken ct)
    {
        if (ledgerDimension is not > 0)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' posts its {side} to a ledger account but has none configured.");
        return await support.LoadDimensionAsync(area, ledgerDimension.Value, $"charge {code.MarkupCode} {side}", ct);
    }

    /// <summary>
    /// Sales tax on a billed charge, resolved from the order's tax group and the charge's item tax
    /// group, through the same path the invoice lines use.
    /// </summary>
    private async Task<decimal> ChargeTaxAsync(string area, SalesTable order, MarkupTable code, MarkupTrans charge,
        decimal amount, DateTime date, SubledgerPostingSupport support,
        List<(string, decimal, decimal, string, string)> taxByCode, CancellationToken ct)
    {
        var taxGroup = charge.TaxGroup.Length > 0 ? charge.TaxGroup : order.TaxGroupId;
        var itemGroup = charge.TaxItemGroup.Length > 0 ? charge.TaxItemGroup : code.TaxItemGroup;
        if (taxGroup.Length == 0 || itemGroup.Length == 0) return 0m;

        var codes = await support.ApplicableTaxCodesAsync(area, taxGroup, itemGroup, date, ct);
        var rate = codes.Sum(x => x.Rate);
        if (rate == 0m) return 0m;
        var tax = order.InclTax
            ? decimal.Round(amount * rate / (100m + rate), 2, MidpointRounding.AwayFromZero)
            : decimal.Round(amount * rate / 100m, 2, MidpointRounding.AwayFromZero);
        foreach (var (taxCode, codeTax) in SubledgerPostingSupport.AllocateTax(codes, tax))
            taxByCode.Add((taxCode, amount, codeTax, taxGroup, itemGroup));
        return tax;
    }
}
