using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Vendor subledger transaction. Invoices are negative (credit liability), payments positive.</summary>
    [Table("VendTrans")]
    public class VendTrans : Entity<long>
    {
        [StringLength(FieldLengths.VendAccount)]
        public string AccountNum { get; set; } = string.Empty;
        public DateTime TransDate { get; set; }
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceId)]
        public string Invoice { get; set; } = string.Empty;
        public string Txt { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal AmountCur { get; set; }
        public decimal AmountMst { get; set; }
        public decimal SettleAmountCur { get; set; }
        public decimal SettleAmountMst { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime Closed { get; set; }
        public LedgerTransType TransType { get; set; }
    }
}
