using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendPaymModeDto : EntityDto<long>
    {
        [Required]
        [StringLength(FieldLengths.PaymModeId)]
        public string PaymMode { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public LedgerJournalACType AccountType { get; set; }

        public long? PaymentLedgerDimension { get; set; }

        [StringLength(FieldLengths.JournalNameId)]
        public string PaymJournalNameId { get; set; } = string.Empty;
    }
}
