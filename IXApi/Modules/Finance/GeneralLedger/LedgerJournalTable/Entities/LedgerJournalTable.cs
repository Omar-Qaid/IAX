using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LedgerJournalTable")]
    public class LedgerJournalTable : Entity<long>
    {
        //----------------------------------------- Core Information & Configuration
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalNum)]
        public string JournalNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.JournalNameId)]
        public string JournalName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public long NumberSequenceTable { get; set; }
        public int NumOfLines { get; set; }

        // Enum Properties
        public LedgerJournalType JournalType { get; set; }
        public NoYes VoucherAllocatedAtPosting { get; set; }

        // ==========================================================
        // Financial Balances & Validation Totals
        // ==========================================================
        // Basic Properties
        public decimal JournalBalance { get; set; }
        public decimal JournalTotalOffsetBalance { get; set; }
        public decimal EndBalance { get; set; }
        public decimal JournalTotalDebit { get; set; }
        public decimal JournalTotalCredit { get; set; }
        public decimal JournalTotalDebitReportingCurrency { get; set; }
        public decimal JournalTotalCreditReportingCurrency { get; set; }

        // ==========================================================
        // Default & Offset Dimensions
        // ==========================================================
        // Basic Properties
        public long DefaultDimension { get; set; }
        public long OffsetLedgerDimension { get; set; }

        // Enum Properties
        public LedgerJournalACType OffsetAccountType { get; set; }
        public NoYes FixedOffsetAccount { get; set; }
        public NoYes IsLedgerDimensionNameUpdated { get; set; }

        // ==========================================================
        // Multi-Currency & Exchange Rate Details
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal ExchRate { get; set; }
        public decimal ExchRateSecondary { get; set; }
        public decimal ReportingCurrencyExchRate { get; set; }
        public decimal ReportingCurrencyExchRateSecondary { get; set; }

        // Enum Properties
        public NoYes FixedExchRate { get; set; }
        public NoYes ReportingCurrencyFixedExchRate { get; set; }
        public NoYes EuroTriangulation { get; set; }

        // ==========================================================
        // Lifecycle States & Posting Audits
        // ==========================================================
        // Basic Properties
        public DateTime PostedDateTime { get; set; }
        public int PostedDateTimeTzId { get; set; }

        // Enum Properties
        public NoYes Posted { get; set; }

        // ==========================================================
        // Workflow, Approvals & System Locks
        // ==========================================================
        // Basic Properties
        public long Approver { get; set; }

        // Enum Properties
        public WorkflowApprovalStatus WorkflowApprovalStatus { get; set; }
        public NoYes SystemBlocked { get; set; }
        public int SystemBlockedReason { get; set; }

        // ==========================================================
        // Sales Tax Policies & Regulatory Controls
        // ==========================================================
        // Enum Properties
        public NoYes LedgerJournalInclTax { get; set; }
        public NoYes DelayTaxCalculation { get; set; }
        public NoYes OverrideSalesTax { get; set; }
        public NoYes CurrentOperationsTax { get; set; }
        public NoYes TaxObligationCompany { get; set; }

        // ==========================================================
        // Performance, Splitting & Processing Controls
        // ==========================================================
        // Basic Properties
        public int LinesLimitBeforeDistribution { get; set; }

        // Enum Properties
        public DetailSummaryPosting DetailSummaryPosting { get; set; }
        public NoYes BankTransSummarizationEnabled { get; set; }
        public BankTransSummarizationCriteria BankTransSummarizationCriteria { get; set; }
        public NoYes RemoveLineAfterPosting { get; set; }

        // ==========================================================
        // Reversals, Corrections & Intercompany Contexts
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.OriginalJournalNum)]
        public string OriginalJournalNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ParentJournalNum)]
        public string ParentJournalNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.OriginalCompany)]
        public string OriginalCompany { get; set; } = string.Empty;

        public DateTime ReverseDate { get; set; }

        // Enum Properties
        public NoYes ReverseEntry { get; set; }
        public NoYes IsAdjustmentJournal { get; set; }

        // ==========================================================
        // Banking, Retail, Leases & Sub-Ledger Extensions
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.RetailStatementId)]
        public string RetailStatementId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DocumentNum)]
        public string DocumentNum { get; set; } = string.Empty;

        public Guid AssetLeaseProcessId { get; set; }
        public long FinTag { get; set; }

        // Enum Properties
        public BankRemittanceType BankRemittanceType { get; set; }
        public CustVendNegInstProtestProcess CustVendNegInstProtestProcess { get; set; }
        public NoYes ProtestSettledBill { get; set; }

        // ==========================================================
        // System Diagnostics, Logs & Sessions
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Log)]
        public string Log { get; set; } = string.Empty;

        public int SessionId { get; set; }
        public DateTime SessionLoginDateTime { get; set; }
        public int SessionLoginDateTimeTzId { get; set; }
        public int SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(JournalName))]
//         public virtual LedgerJournalName? JournalDefinition { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DefaultDimensionSet { get; set; }

//         [ForeignKey(nameof(OffsetLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? OffsetLedgerAccount { get; set; }

//         [ForeignKey(nameof(Approver))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? ApproverEmployee { get; set; }

        #endregion
    }
}

