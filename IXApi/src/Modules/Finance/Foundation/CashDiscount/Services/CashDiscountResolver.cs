using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.CashDiscount;

/// <summary>
/// What cash discount a transaction carries, resolved from the code on the transaction and the
/// <see cref="CashDisc"/> master.
/// </summary>
/// <param name="Discount">The master record, or null when the transaction carries no code.</param>
/// <param name="DiscountDate">The last date the discount may be taken.</param>
/// <param name="PossibleAmount">
/// The discount on the still-open amount, signed like the transaction, so a receivable discount is
/// positive and a payable one negative.
/// </param>
public sealed record CashDiscountTerms(CashDisc? Discount, DateTime DiscountDate, decimal PossibleAmount)
{
    public bool Applies => Discount != null && PossibleAmount != 0m;
}

/// <summary>
/// The single derivation of a transaction's cash discount, used both when an invoice populates the
/// open table and when a payment settles against it, so the two cannot disagree.
/// </summary>
internal static class CashDiscountResolver
{
    /// <summary>
    /// Resolves the discount for <paramref name="code"/> against <paramref name="openAmount"/>.
    /// Returns terms that do not apply when the code is blank or unknown — a blank code is the
    /// ordinary case of an invoice with no discount, not an error.
    /// </summary>
    public static async Task<CashDiscountTerms> ResolveAsync(DbContext db, string area, string? code,
        DateTime baseDate, decimal openAmount, CancellationToken ct)
    {
        var discountCode = (code ?? string.Empty).Trim();
        if (discountCode.Length == 0)
            return new CashDiscountTerms(null, default, 0m);

        var discount = await db.Set<CashDisc>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.CashDiscCode == discountCode, ct);
        if (discount == null)
            return new CashDiscountTerms(null, default, 0m);

        return new CashDiscountTerms(
            discount,
            CashDiscountCalculator.Date(baseDate, discount),
            CashDiscountCalculator.Amount(openAmount, discount));
    }
}
