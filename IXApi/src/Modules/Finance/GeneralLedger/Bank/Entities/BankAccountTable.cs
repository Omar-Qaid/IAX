using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("BankAccountTable")]
    public class BankAccountTable : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountId)]
        public string AccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankGroupId)]
        public string BankGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Iban)]
        public string Iban { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.SwiftNo)]
        public string SwiftNo { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.RegistrationNum)]
        public string RegistrationNum { get; set; } = string.Empty;

        // Enum Properties
        public BankAccountStatus BankAccountStatus { get; set; }
        public BankCodeType BankCodeType { get; set; }

        // ==========================================================
        // Currency & Exchange Rate Configurations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public long AccountingCurrencyExchangeRateType { get; set; }
        public long ReportingCurrencyExchangeRateType { get; set; }
        public DateTime LastRevalResetDate { get; set; }

        // Enum Properties
        public NoYes BankMultiCurrency { get; set; }
        public BankRevalDimensionSetting RevalDimensionSetting { get; set; }

        // ==========================================================
        // Ledgers, Posting Profiles & Dimensions
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }
        public long BridgingAccountLedgerDimension { get; set; }
        public long CustomerPaymentFeeLedgerDimension { get; set; }
        public long InvoiceRemittanceLedgerDimension { get; set; }
        public long RemittanceCollectionLedgerDimension { get; set; }
        public long RemittanceDiscountLedgerDimension { get; set; }

        // Enum Properties
        public BankCustPaymFeePost CustPaymFeePost { get; set; }

        // ==========================================================
        // Credit Limits & Threshold Balances
        // ==========================================================
        // Basic Properties
        public decimal OverdraftLimit { get; set; }
        public decimal CfmBankBalanceMinimum { get; set; }
        public decimal DiscCreditMaxMst { get; set; }
        public decimal InvoiceRemitAmount { get; set; }
        public decimal RemitCollectionAmount { get; set; }
        public decimal RemitDiscountAmount { get; set; }

        // ==========================================================
        // Advanced Bank Reconciliation Rules
        // ==========================================================
        // Basic Properties
        public long BankStatementFormat { get; set; }
        public long BankReconciliationReportFormat { get; set; }
        public long BankReconciliationMatchRuleSet { get; set; }
        public decimal BankReconAllowedPennyDifference { get; set; }

        // Enum Properties
        public NoYes BankReconciliationEnabled { get; set; }
        public NoYes BankReconMatchAutoAfterImport { get; set; }
        public NoYes IsRunMatchingRule { get; set; }
        public NoYes BankReconBridgedAutoClearing { get; set; }
        public NoYes BankReconciliationStmtAsPaymConfirm { get; set; }

        // ==========================================================
        // Payment Processing, Electronic Files & Journals
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CompanyPaymId)]
        public string CompanyPaymId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DebitDirectId)]
        public string DebitDirectId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankCompanyStatementName)]
        public string BankCompanyStatementName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BankDestinationName)]
        public string BankDestinationName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CustPaymentJournalName)]
        public string CustPaymentJournalName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.VendPaymentJournalName)]
        public string VendPaymentJournalName { get; set; } = string.Empty;

        public long BankConstantSymbol { get; set; }
        public DateTime BankPositivePayStartDate { get; set; }
        public int PrenoteResponseDays { get; set; }

        // Enum Properties
        public NoYes IsBankPrenote { get; set; }
        public NoYes IsNachaFileBlocked { get; set; }
        public NoYes ReverseDebitCredit { get; set; }

        // ==========================================================
        // NSF (Non-Sufficient Funds) Controls
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.NsfLedgerJournalName)]
        public string NsfLedgerJournalName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.NsfFeeMarkupGroupId)]
        public string NsfFeeMarkupGroupId { get; set; } = string.Empty;

        // Enum Properties
        public BankNsfFeeMarkupGroupModule NsfFeeMarkupGroupModule { get; set; }

        // ==========================================================
        // Regional Localization & Validities
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CorrAccount_W)]
        public string CorrAccount_W { get; set; } = string.Empty;

        public DateTime ActiveFrom { get; set; }
        public int ActiveFromTzId { get; set; }
        public DateTime ActiveTo { get; set; }
        public int ActiveToTzId { get; set; }
        public long Location { get; set; }
        public int TimeZone { get; set; }

        // Enum Properties
        public NoYes LvDefaultBank { get; set; }
        public int LvPayOrderType { get; set; }
        public int TimeZonePreference { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DimensionAttributeValueCombination { get; set; }

//         [ForeignKey(nameof(BankGroupId))]
//         public virtual BankGroup? BankGroup { get; set; }

//         [ForeignKey(nameof(BridgingAccountLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? BridgingAccountCombination { get; set; }

//         [ForeignKey(nameof(CustomerPaymentFeeLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? CustomerPaymentFeeCombination { get; set; }

//         [ForeignKey(nameof(InvoiceRemittanceLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? InvoiceRemittanceCombination { get; set; }

//         [ForeignKey(nameof(RemittanceCollectionLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? RemittanceCollectionCombination { get; set; }

//         [ForeignKey(nameof(RemittanceDiscountLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? RemittanceDiscountCombination { get; set; }

//         [ForeignKey(nameof(Location))]
//         public virtual LogisticsLocation? LogisticsLocation { get; set; }

        #endregion
    }
}

