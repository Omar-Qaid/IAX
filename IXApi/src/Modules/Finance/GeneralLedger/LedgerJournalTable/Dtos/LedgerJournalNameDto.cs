using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class LedgerJournalNameDto : EntityDto<long>
    {
        [StringLength(FieldLengths.JournalNameId)]
        public string JournalName { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public long NumberSequenceTable { get; set; }
        public LedgerJournalType JournalType { get; set; }
        public LedgerJournalVoucherChange NewVoucher { get; set; }
        public NoYes VoucherAllocatedAtPosting { get; set; }
        public DetailSummary DetailSummary { get; set; }
        public long DefaultDimension { get; set; }
        public long OffsetLedgerDimension { get; set; }
        public LedgerJournalACType OffsetAccountType { get; set; }
        public NoYes FixedOffsetAccount { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public NoYes FixedExchRate { get; set; }
    }
}
