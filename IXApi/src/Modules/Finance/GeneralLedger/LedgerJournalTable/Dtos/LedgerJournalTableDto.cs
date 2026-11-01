using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class LedgerJournalTableDto : EntityDto<long>
    {
        [StringLength(FieldLengths.JournalNum)]
        public string JournalNum { get; set; } = string.Empty;

        [StringLength(FieldLengths.JournalNameId)]
        public string JournalName { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public int NumOfLines { get; set; }
        public LedgerJournalType JournalType { get; set; }
        public NoYes VoucherAllocatedAtPosting { get; set; }
        public decimal JournalBalance { get; set; }
        public decimal JournalTotalDebit { get; set; }
        public decimal JournalTotalCredit { get; set; }
        public decimal JournalTotalDebitReportingCurrency { get; set; }
        public decimal JournalTotalCreditReportingCurrency { get; set; }
        public long DefaultDimension { get; set; }
        public long OffsetLedgerDimension { get; set; }
        public LedgerJournalACType OffsetAccountType { get; set; }
        public NoYes FixedOffsetAccount { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal ExchRate { get; set; }
        public decimal ReportingCurrencyExchRate { get; set; }
        public NoYes FixedExchRate { get; set; }
        public DateTime PostedDateTime { get; set; }
        public NoYes Posted { get; set; }
        public NoYes SystemBlocked { get; set; }

        [StringLength(FieldLengths.OriginalJournalNum)]
        public string OriginalJournalNum { get; set; } = string.Empty;

        public DateTime ReverseDate { get; set; }
        public NoYes ReverseEntry { get; set; }
        public List<LedgerJournalTransDto> Lines { get; set; } = new List<LedgerJournalTransDto>();
    }
}

