using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LedgerJournalName")]
    public class LedgerJournalName : Entity<long>
    {
        //----------------------------------------- Core Information & Rules
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalNameId)]
        public string JournalName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public long NumberSequenceTable { get; set; }

        // Enum Properties
        public LedgerJournalType JournalType { get; set; }
        public LedgerJournalVoucherChange NewVoucher { get; set; }
        public NoYes VoucherAllocatedAtPosting { get; set; }
        public DetailSummary DetailSummary { get; set; }

        // ==========================================================
        // Financial & Default Dimensions
        // ==========================================================
        // Basic Properties
        public long DefaultDimension { get; set; }
        public long OffsetLedgerDimension { get; set; }

        // Enum Properties
        public LedgerJournalACType OffsetAccountType { get; set; }
        public NoYes FixedOffsetAccount { get; set; }

        // ==========================================================
        // Multi-Currency & Payments Setup
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        // Enum Properties
        public NoYes FixedExchRate { get; set; }
        public NoYes IsAdvancedPayment { get; set; }
        public LedgerJournalFeePosting LedgerJournalFeePosting { get; set; }
        public NoYes Prepayment_W { get; set; }

        // ==========================================================
        // Approval & Workflow Configurations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ApproveGroupId)]
        public string ApproveGroupId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes ApproveActive { get; set; }
        public NoYes WorkflowApproval { get; set; }

        // ==========================================================
        // Sales Tax Policies & Regulatory Behavior
        // ==========================================================
        // Enum Properties
        public NoYes LedgerJournalInclTax { get; set; }
        public NoYes DelayTaxCalculation { get; set; }
        public NoYes OverrideSalesTax { get; set; }
        public NoYes CurrentOperationsTax { get; set; }
        public TaxHideAmountFields TaxHideAmountFields { get; set; }
        public TaxBookTypeJournal TaxBookTypeJournal { get; set; }

        // ==========================================================
        // Bank Summarization & Performance Controls
        // ==========================================================
        // Basic Properties
        public int LinesLimitBeforeDistribution { get; set; }

        // Enum Properties
        public NoYes BankTransSummarizationEnabled { get; set; }
        public BankTransSummarizationCriteria BankTransSummarizationCriteria { get; set; }
        public NoYes EndBalanceControl { get; set; }
        public NoYes RemoveLineAfterPosting { get; set; }

        // ==========================================================
        // Integration Settings
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Configuration)]
        public string Configuration { get; set; } = string.Empty;


        #region Navigation Properties Row

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DefaultDimensionSet { get; set; }

//         [ForeignKey(nameof(OffsetLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? OffsetLedgerAccount { get; set; }

        #endregion
    }
}

