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
        public NoYes LedgerJournalInclTax { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal ExchRate { get; set; }
        public decimal ExchRateSecondary { get; set; }
        public decimal ReportingCurrencyExchRate { get; set; }
        public decimal ReportingCurrencyExchRateSecondary { get; set; }
        public NoYes FixedExchRate { get; set; }
        public NoYes ReportingCurrencyFixedExchRate { get; set; }
        public DateTime PostedDateTime { get; set; }
        public NoYes Posted { get; set; }
        public NoYes SystemBlocked { get; set; }

        [StringLength(FieldLengths.OriginalJournalNum)]
        public string OriginalJournalNum { get; set; } = string.Empty;

        public DateTime ReverseDate { get; set; }
        public NoYes ReverseEntry { get; set; }

        // Header fields shown on the journal tabs (General, Setup, Blocking, History).
        // Read-only through this DTO: LedgerJournalTableService keeps the stored values on update.
        public NoYes CurrentOperationsTax { get; set; }
        public DetailSummaryPosting DetailSummaryPosting { get; set; }
        public int LinesLimitBeforeDistribution { get; set; }

        [StringLength(FieldLengths.ParentJournalNum)]
        public string ParentJournalNum { get; set; } = string.Empty;

        [StringLength(FieldLengths.DocumentNum)]
        public string DocumentNum { get; set; } = string.Empty;

        public NoYes TaxObligationCompany { get; set; }
        public NoYes BankTransSummarizationEnabled { get; set; }
        public BankTransSummarizationCriteria BankTransSummarizationCriteria { get; set; }
        public int SessionId { get; set; }
        public int SystemBlockedReason { get; set; }
        public WorkflowApprovalStatus WorkflowApprovalStatus { get; set; }

        [StringLength(FieldLengths.OriginalCompany)]
        public string OriginalCompany { get; set; } = string.Empty;

        [StringLength(FieldLengths.Log)]
        public string Log { get; set; } = string.Empty;
        public List<LedgerJournalTransDto> Lines { get; set; } = new List<LedgerJournalTransDto>();
    }
}

