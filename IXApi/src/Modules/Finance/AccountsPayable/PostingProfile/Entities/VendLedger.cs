using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>D365 VendLedger: the vendor posting profile header that VendLedgerAccounts lines belong to.</summary>
    [Table("VendLedger")]
    public class VendLedger : Entity<long>
    {
        [Required]
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        /// <summary>D365 Settlement: transactions posted with this profile can be settled.</summary>
        public NoYes Settlement { get; set; }

        /// <summary>D365 Cancel.</summary>
        public NoYes Cancel { get; set; }

        /// <summary>D365 PostingProfileClose: the profile a transaction moves to when it is closed.</summary>
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfileClose { get; set; } = string.Empty;
    }
}
