using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxTable")]
    public class TaxTable : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxName)]
        public string TaxName { get; set; } = string.Empty;

        [StringLength(FieldLengths.TaxPeriod)]
        public string TaxPeriod { get; set; } = "Monthly"; // Code relation link to settlement periods

        [StringLength(FieldLengths.TaxAccountGroup)]
        public string TaxAccountGroup { get; set; } = "STANDARD"; // Core ledger main account group assignment

        [StringLength(FieldLengths.TaxCurrencyCode)]
        public string TaxCurrencyCode { get; set; } = "SAR";

        // Enum Properties
        public TaxGroupSource Source { get; set; }

        // ==========================================================
        // Calculation Engines & Base Criteria Directives
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxOnTax)]
        public string TaxOnTax { get; set; } = string.Empty; // Identifies parent tax code if nested compounding is active

        [StringLength(FieldLengths.TaxUnit)]
        public string TaxUnit { get; set; } = string.Empty; // Operational unit mapping code for unit-driven models

        // Enum Properties
        public TaxBase TaxBase { get; set; }             // Net, Gross, Marginal base, or Compounded values
        public TaxCalcMethod TaxCalcMethod { get; set; } // Internal calculation distribution logic (Whole amount vs. Interval)
        public TaxLimitBase TaxLimitBase { get; set; }   // Basis parameter rules for evaluating maximum/minimum tax bands
        public NoYes TaxIncludeInTax { get; set; }
        public NoYes NegativeTax { get; set; }
        public NoYes UnrealizedTax { get; set; }          // Deferred cash-basis tax calculation mapping control flag
        public NoYes TaxAllowLineDiscountOnTaxPerUnit { get; set; }

        // ==========================================================
        // Rounding Parameters & Fractions Behavior
        // ==========================================================
        // Basic Properties
        public decimal TaxRoundOff { get; set; }

        // Enum Properties
        public RoundingType TaxRoundOffType { get; set; } // Normal, Round Up, Round Down
        public NoYes RoundDeductibleFirst { get; set; }

        // ==========================================================
        // Regional Customization & Reporting Demarcations
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.PrintCode)]
        public string PrintCode { get; set; } = string.Empty;

        [StringLength(FieldLengths.PaymentTaxCode)]
        public string PaymentTaxCode { get; set; } = string.Empty; // Alternate payment code switch for cash accounting

        [StringLength(FieldLengths.TaxJurisdictionCode)]
        public string TaxJurisdictionCode { get; set; } = string.Empty; // Regional sub-classification anchor string

        // Enum Properties
        public TaxType_W TaxType_W { get; set; } // Core localization structural grouping indicator
        public TaxCountryRegionType TaxCountryRegionType { get; set; } // Domestic, EU, Third-Country classifications
        public NoYes NotEuSalesList { get; set; }
        public NoYes ExcludeFromInvoice { get; set; }
        public NoYes TaxPurchaseTax { get; set; }
        public NoYes TaxPackagingTax { get; set; }
        public TaxWriteSelection TaxWriteSelection { get; set; }
        public TaxReconcileAmountOrigin ReconcileAmountOrigin { get; set; }

        // ==========================================================
        // Fiscal Reporting Mapping Core Matrices (Tax Declaration Box Field IDs)
        // ==========================================================
        // Outgoing/Sales Bases & Amounts
        public int RepFieldBaseOutgoing { get; set; }
        public int RepFieldBaseOutgoingCreditNote { get; set; }
        public int RepFieldTaxOutgoing { get; set; }
        public int RepFieldTaxOutgoingCreditNote { get; set; }

        // Incoming/Purchase Bases & Amounts
        public int RepFieldBaseIncoming { get; set; }
        public int RepFieldBaseIncomingCreditNote { get; set; }
        public int RepFieldTaxIncoming { get; set; }
        public int RepFieldTaxIncomingCreditNote { get; set; }

        // Use Tax Mechanics
        public int RepFieldBaseUseTax { get; set; }
        public int RepFieldBaseUseTaxCreditNote { get; set; }
        public int RepFieldUseTax { get; set; }
        public int RepFieldUseTaxCreditNote { get; set; }

        // Use Tax Accounting Offsets
        public int RepFieldBaseUseTaxOffset { get; set; }
        public int RepFieldBaseUseTaxOffsetCreditNote { get; set; }
        public int RepFieldUseTaxOffset { get; set; }
        public int RepFieldUseTaxOffsetCreditNote { get; set; }

        // Exempt / Tax-Free Trade Frameworks
        public int RepFieldTaxFreeSales { get; set; }
        public int RepFieldTaxFreeSalesCreditNote { get; set; }
        public int RepFieldTaxFreeBuy { get; set; }
        public int RepFieldTaxFreeBuyCreditNote { get; set; }


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxUnit))]
        public virtual UnitOfMeasure? UnitOfMeasureTable { get; set; }

        [ForeignKey(nameof(TaxCurrencyCode))]
        public virtual Currency? Currency { get; set; }

        #endregion
    }
}

