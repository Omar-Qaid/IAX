using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("MainAccount")]
    public class MainAccount : Entity<long>
    {
        //----------------------------------------- Core Information & Identities
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.MainAccountValue)]
        public string MainAccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public long LedgerChartOfAccounts { get; set; }
        public long ParentMainAccount { get; set; }
        public long MainAccountTemplate { get; set; }
        public int AccountCategoryRef { get; set; }

        // Enum Properties
        public MainAccountType Type { get; set; } // e.g., Asset, Liability, Revenue, Expense

        // ==========================================================
        // Multi-Currency & Financial Translation Policies
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public long ExchangeAdjustmentRateType { get; set; }
        public long ReportingExchangeAdjustmentRateType { get; set; }
        public long FinancialReportingExchangeRateType { get; set; }

        // Enum Properties
        public NoYes ExchangeAdjusted { get; set; }
        public NoYes Monetary { get; set; }
        public int FinancialReportingTranslationType { get; set; }

        // ==========================================================
        // Balance Controls & Posting Rules
        // ==========================================================
        // Basic Properties
        public long OffsetLedgerDimension { get; set; }
        public long UnitOfMeasure { get; set; }

        // Enum Properties
        public LedgerPostingType PostingType { get; set; }
        public DebitCreditProposal DebitCreditProposal { get; set; }
        public DebitCreditCheck DebitCreditCheck { get; set; }
        public DebitCreditDemand DebitCreditBalanceDemand { get; set; }
        public NoYes MandatoryPaymentReference { get; set; }

        // ==========================================================
        // Validation Rules & System Gates
        // ==========================================================
        // Enum Properties
        public int ValidateCurrency { get; set; } // Map to CurrencyValidation Enum if matching
        public int ValidatePosting { get; set; }  // Map to PostingValidation Enum if matching
        public int ValidateUser { get; set; }     // Map to UserValidation Enum if matching

        // ==========================================================
        // Year-End Closing & Period Transitions
        // ==========================================================
        // Basic Properties
        public long OpeningAccount { get; set; }

        // Enum Properties
        public MainAccountCloseType CloseType { get; set; }
        public NoYes Closing { get; set; }

        // ==========================================================
        // Reporting, Consolidations & Localized Hierarchies
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ConsolidationMainAccount)]
        public string ConsolidationMainAccount { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.GroupLevel01)]
        public string GroupLevel01 { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.GroupLevel02)]
        public string GroupLevel02 { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.GroupLevel03)]
        public string GroupLevel03 { get; set; } = string.Empty;

        public long StandardMainAccount_W { get; set; }

        // Enum Properties
        public int ReportingAccountType { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(LedgerChartOfAccounts))]
//         public virtual LedgerChartOfAccounts? ChartOfAccounts { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(OffsetLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? OffsetAccountCombination { get; set; }

//         [ForeignKey(nameof(ParentMainAccount))]
//         public virtual MainAccount? ParentAccount { get; set; }

//         [ForeignKey(nameof(OpeningAccount))]
//         public virtual MainAccount? OpeningMainAccount { get; set; }

        #endregion
    }
}

