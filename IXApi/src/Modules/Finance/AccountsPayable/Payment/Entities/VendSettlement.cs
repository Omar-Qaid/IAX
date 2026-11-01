using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Vendor invoice/payment settlement.
/// </summary>
/// <remarks>
/// This is a reduced field set, not D365's: the standard <c>VendSettlement</c> carries the same
/// shape as <c>CustSettlement</c> (<c>SettlementVoucher</c>, <c>TransRecId</c>/<c>OffsetRecId</c>
/// pairs, the reporting-currency and adjustment amounts). Aligning it is a change of its own and
/// has not been made here. <c>CanBeReversed</c> is a standard field and was added so a settlement
/// can be reversed once, the way the receivable side does it.
/// </remarks>
public class VendSettlement : Entity<long>
{
    [MaxLength(20)] public string AccountNum { get; set; } = "";
    public long InvoiceTransRecId { get; set; }
    public long PaymentTransRecId { get; set; }
    public DateTime TransDate { get; set; }
    [MaxLength(20)] public string Voucher { get; set; } = "";
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }

    /// <summary>Whether the match is still in force and may be reversed.</summary>
    public NoYes CanBeReversed { get; set; }
}
