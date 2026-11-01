using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendSettlementDto : EntityDto<long>
{
    public string AccountNum { get; set; } = string.Empty;
    public long InvoiceTransRecId { get; set; }
    public long PaymentTransRecId { get; set; }
    public DateTime TransDate { get; set; }
    public string Voucher { get; set; } = string.Empty;
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }
    public NoYes CanBeReversed { get; set; }
}
