using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Vendor posting profile account rule (Table/Group/All) — mirrors CustLedgerAccounts.</summary>
    [Table("VendLedgerAccounts")]
    public class VendLedgerAccounts : Entity<long>
    {
        [Required]
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;
        public AccountCode AccountCode { get; set; }
        [StringLength(FieldLengths.Num)]
        public string Num { get; set; } = string.Empty;
        /// <summary>Vendor summary (accounts payable control) account.</summary>
        public long SummaryLedgerDimension { get; set; }
        public long ClearingLedgerDimension { get; set; }
    }
}
