using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxLedgerAccountGroup")]
    public class TaxLedgerAccountGroup : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string TaxAccountGroup { get; set; } = string.Empty; // Primary unique code key (e.g., "VAT-STD", "TAX-EU")

        [Required]
        [StringLength(60)]
        public string Name { get; set; } = string.Empty; // Descriptive user label for the posting group

        // ==========================================================
        // Core Standard Tax Ledger Accounts (Sales & Purchase)
        // ==========================================================
        // Basic Properties
        public long? TaxOutgoingLedgerDimension { get; set; } // Main account dimension for payable/sales tax (Output VAT)

        public long? TaxIncomingLedgerDimension { get; set; } // Main account dimension for recoverable/purchase tax (Input VAT)

        public long? TaxReportLedgerDimension { get; set; }   // Main account dimension used during periodic settlement clearance

        // ==========================================================
        // Use Tax & Reverse Charge Posting Accounts
        // ==========================================================
        // Basic Properties
        public long? TaxUseTaxLedgerDimension { get; set; }       // Main account dimension for payable use tax liability

        public long? TaxOffsetUseTaxLedgerDimension { get; set; } // Offset main account dimension for receivable use tax

        public long? TaxReverseOffsetIncLedgerDimension_W { get; set; } // Reverse charge input VAT offset ledger dimension

        public long? TaxReverseOffsetOutLedgerDimension_W { get; set; } // Reverse charge output VAT offset ledger dimension

        // ==========================================================
        // Special Tax Conditions (Non-Deductible, Free %, Transit)
        // ==========================================================
        // Basic Properties
        public long? TaxNonDeductibleTaxLedgerDimension { get; set; } // Account for expense/asset non-deductible tax allocation

        public long? TaxFreePercentLedgerDimension { get; set; }       // Account allocated for tax-exempt proportion calculations

        public long? TaxInterimTransitLedgerDimension { get; set; }   // Interim clearing account for goods in transit or pending tax points

        // ==========================================================
        // Cash Settlement, Unrealized & Cash Discount Adjustments
        // ==========================================================
        // Basic Properties
        public long? TaxUnrealizedPayablesLedgerDimension { get; set; }   // Deferred/Unrealized tax account for open vendor invoices

        public long? TaxUnrealizedReceivablesLedgerDimension { get; set; } // Deferred/Unrealized tax account for open customer invoices

        public long? CashDiscountIncomingLedgerDimension { get; set; }    // Tax recalculation account on incoming cash discounts

        public long? CashDiscountOutgoingLedgerDimension { get; set; }    // Tax recalculation account on outgoing cash discounts

        // ==========================================================
        // Discrepancy & Differential Accounting Accounts
        // ==========================================================
        // Basic Properties
        public long? TaxIncomingDifferenceLedgerDimension { get; set; }    // Variance account for input tax calculation rounding

        public long? TaxIncomingDiffOffsetLedgerDimension { get; set; }    // Offset account for input tax calculation rounding variance

        public long? TaxOutgoingDifferenceLedgerDimension { get; set; }    // Variance account for output tax calculation rounding

        public long? TaxOutgoingDiffOffsetLedgerDimension { get; set; }    // Offset account for output tax calculation rounding variance

        public long? PennyDifferenceCustomerLedgerDimension { get; set; } // Penny rounding threshold variance for customer settlements

        public long? PennyDifferenceVendorLedgerDimension { get; set; }   // Penny rounding threshold variance for vendor settlements


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxOutgoingLedgerDimension))]
        public virtual DimensionAttributeValueCombination? TaxOutgoingLedgerDimensionTable { get; set; }

        [ForeignKey(nameof(TaxIncomingLedgerDimension))]
        public virtual DimensionAttributeValueCombination? TaxIncomingLedgerDimensionTable { get; set; }

        [ForeignKey(nameof(TaxReportLedgerDimension))]
        public virtual DimensionAttributeValueCombination? TaxReportLedgerDimensionTable { get; set; }

        #endregion
    }
}
