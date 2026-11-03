using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Posts a purchase order's charges (MarkupTrans) when the order is invoiced — the payable mirror
/// of <see cref="AccountsReceivable.Invoice.SalesChargePosting"/>.
///
/// A charge code carries a debit side and a credit side: CustType/CustomerLedgerDimension are the
/// **debit** type and account, VendType/VendorLedgerDimension the **credit** ones, despite the
/// Cust/Vend prefixes. Type 0 is a ledger account, 1 is the customer or vendor, 2 is the item.
///
/// The documented purchase pattern is the reverse of sales: debit a ledger account for the cost and
/// credit the vendor, so the charge increases what is owed. A charge with both sides on ledger
/// accounts posts its own balanced pair and leaves the vendor balance alone.
///
/// Where payable differs from receivable: the **Item** type is allowed in D365 for purchases, where
/// it adds the charge to the item cost — landed cost. That is outside basic scope, so it is refused
/// rather than approximated, which matters more here than on the sales side, where the type is not
/// even permitted.
///
/// Charges attach to the purchase **header** only. The charge service maps a purchase line level to
/// nothing, and no purchase line table number exists, so a line-level purchase charge cannot be
/// created and is not read here.
///
/// Each charge posts once, recorded in MarkupTrans.Posted, so a later partial invoice of the same
/// order does not bill it again.
/// </summary>
internal sealed class PurchaseChargePosting(DbContext db)
{
    private const int TypeLedgerAccount = 0;
    private const int TypeCustomerVendor = 1;
    private const int TypeItem = 2;

    /// <summary>
    /// <paramref name="ChargedToVendor"/> is the part that reaches the vendor balance, which is
    /// what SumMarkup and the invoice total carry. <paramref name="Tax"/> is the input tax on it.
    /// </summary>
    internal sealed record Result(decimal ChargedToVendor, decimal Tax);

    public async Task<Result> PostAsync(string area, PurchTable order, long entryRecId, string invoiceId,
        string voucher, DateTime date, SubledgerCurrencyTranslation translation, SubledgerPostingSupport support, CancellationToken ct)
    {
        var charges = await UnpostedChargesAsync(area, order, ct);
        if (charges.Count == 0) return new(0m, 0m);

        var codes = await db.Set<MarkupTable>().AsNoTracking().Where(x => x.DataAreaId == area).ToListAsync(ct);
        var charged = 0m;
        var totalTax = 0m;

        foreach (var (charge, amount) in charges)
        {
            if (amount == 0m) continue;
            var code = codes.FirstOrDefault(x => x.MarkupCode == charge.MarkupCode)
                ?? throw new SubledgerPostingException(
                    $"Charges code '{charge.MarkupCode}' does not exist in this company.");
            EnsureInScope(code);

            var text = charge.Txt.Length > 0 ? charge.Txt : $"Charge {charge.MarkupCode} on {invoiceId}";

            // The debit side is always a ledger account on a purchase charge: the cost of the charge.
            var debit = await LedgerSideAsync(area, code, code.CustomerLedgerDimension, "debit", support, ct);
            db.Add(translation.AccountEntry(area, entryRecId, debit,
                LedgerPostingType.LedgerJournal, amount, text, 0m));

            // The credit side. When it is the vendor, no entry is written here: the invoice posts
            // one summary vendor entry and the amount is folded into it through ChargedToVendor.
            if (code.VendType == TypeLedgerAccount)
            {
                var credit = await LedgerSideAsync(area, code, code.VendorLedgerDimension, "credit", support, ct);
                db.Add(translation.AccountEntry(area, entryRecId, credit,
                    LedgerPostingType.LedgerJournal, -amount, text, 0m));
            }
            else
            {
                charged += amount;
                var tax = await ChargeInputTaxAsync(area, order, code, charge, amount, invoiceId, voucher,
                    date, translation, entryRecId, support, ct);
                totalTax += tax;
                charge.TaxAmount = tax;
            }

            charge.Posted = amount;
            charge.Voucher = voucher;
            charge.TransDate = date;
            charge.DocumentStatus = DocumentStatus.Invoice;
        }
        return new(charged, totalTax);
    }

    /// <summary>
    /// The order's unposted charges with the amount each is worth, measured against the order's
    /// lines the same way the charge calculator measures them.
    /// </summary>
    private async Task<List<(MarkupTrans Charge, decimal Amount)>> UnpostedChargesAsync(
        string area, PurchTable order, CancellationToken ct)
    {
        var charges = await db.Set<MarkupTrans>()
            .Where(x => x.DataAreaId == area
                && !x.IsDeleted
                && x.Posted == 0m
                && x.ModuleType == MarkupModuleType.Vendor
                && x.TransRecId == order.RecId)
            .ToListAsync(ct);
        if (charges.Count == 0) return [];

        var lines = await db.Set<PurchLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.PurchId == order.PurchId && x.PurchStatus != PurchStatus.Canceled)
            .Select(x => new { x.PurchQty, x.LineAmount })
            .ToListAsync(ct);
        var quantity = lines.Sum(x => x.PurchQty);
        var net = lines.Sum(x => Math.Max(0m, x.LineAmount));
        return charges.OrderBy(x => x.LineNum).ThenBy(x => x.RecId)
            .Select(charge => (charge, MarkupChargeCalculator.CalculateForBase(charge, quantity, net)))
            .ToList();
    }

    /// <summary>Refuses the charge configurations this path does not post.</summary>
    private static void EnsureInScope(MarkupTable code)
    {
        if (code.CustType == TypeItem || code.VendType == TypeItem)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' adds the charge to the item cost. Landed cost is not posted by "
                + "this path; configure the charge against a ledger account instead.");
        if (code.CustType == TypeCustomerVendor)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' debits a customer or vendor account. A purchase charge debits a "
                + "ledger account for its cost; configure its debit side as a ledger account.");
        if (code.CustType != TypeLedgerAccount)
            throw new SubledgerPostingException(
                $"Charges code '{code.MarkupCode}' has an unrecognised debit type '{code.CustType}'.");
        if (code.VendType is not (TypeLedgerAccount or TypeCustomerVendor))
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
    /// Input tax on a charge billed by the vendor, posted to the incoming tax account the way
    /// purchase line tax is. The tax transaction is keyed by the invoice voucher and carries no
    /// source line, as the receivable side's invoice tax does, because a charge is not an invoice
    /// line and no table number identifies one.
    /// </summary>
    private async Task<decimal> ChargeInputTaxAsync(string area, PurchTable order, MarkupTable code,
        MarkupTrans charge, decimal amount, string invoiceId, string voucher, DateTime date, SubledgerCurrencyTranslation translation,
        long entryRecId, SubledgerPostingSupport support, CancellationToken ct)
    {
        var taxGroup = charge.TaxGroup.Length > 0 ? charge.TaxGroup : order.TaxGroup;
        var itemGroup = charge.TaxItemGroup.Length > 0 ? charge.TaxItemGroup : code.TaxItemGroup;
        if (taxGroup.Length == 0 || itemGroup.Length == 0) return 0m;

        var taxCodes = await support.ApplicableTaxCodesAsync(area, taxGroup, itemGroup, date, ct);
        if (taxCodes.Count == 0) return 0m;
        var total = decimal.Round(amount * taxCodes.Sum(x => x.Rate) / 100m, 2, MidpointRounding.AwayFromZero);

        foreach (var (taxCode, codeTax) in SubledgerPostingSupport.AllocateTax(taxCodes, total))
        {
            var table = await db.Set<TaxTable>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxCode == taxCode, ct)
                ?? throw new SubledgerPostingException($"Purchase tax code '{taxCode}' was not found.");
            // A zero-rated charge needs no input tax account and posts no ledger line; its base is
            // still recorded below so the charge's turnover is reportable under the code.
            if (codeTax != 0m)
            {
                var group = await db.Set<TaxLedgerAccountGroup>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.TaxAccountGroup == table.TaxAccountGroup, ct);
                if (group?.TaxIncomingLedgerDimension is not > 0)
                    throw new SubledgerPostingException(
                        $"Purchase input tax account is missing for tax code '{taxCode}'.");
                var account = await support.LoadDimensionAsync(area, group.TaxIncomingLedgerDimension.Value,
                    "purchase input tax", ct);
                db.Add(translation.AccountEntry(area, entryRecId, account, LedgerPostingType.Tax, codeTax,
                    $"Purchase input tax {taxCode} on charge {code.MarkupCode} of {invoiceId}", 0m));
            }
            db.Add(new TaxTrans
            {
                DataAreaId = area, TransDate = date, Voucher = voucher, TaxCode = taxCode,
                TaxGroup = taxGroup, TaxItemGroup = itemGroup,
                TaxValue = taxCodes.Single(x => x.Code == taxCode).Rate, TaxDirection = TaxDirection.Incoming,
                SourceCurrencyCode = order.CurrencyCode, CurrencyCode = translation.TransactionCurrency,
                SourceBaseAmountCur = amount, SourceTaxAmountCur = codeTax,
                TaxBaseAmountCur = amount, TaxAmountCur = codeTax,
                TaxBaseAmount = translation.ToAccounting(amount), TaxAmount = translation.ToAccounting(codeTax),
                TaxBaseAmountRep = translation.ToReporting(amount), TaxAmountRep = translation.ToReporting(codeTax)
            });
        }
        return total;
    }
}
