using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("PaymSched")]
    public class PaymSched : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PaymSched)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // ==========================================================
        // Installment Breakdown Calculation Profile
        // ==========================================================
        // Basic Properties
        public int NumOfPayment { get; set; } // Number of scheduled installments
        public decimal AmountCur { get; set; }   // Fixed default value or threshold base
        public decimal LowestAmount { get; set; } // Minimal calculated value threshold tolerated per installment step

        // Enum Properties
        public PaymSchedAllocateMethod PayBy { get; set; } // Allocation basis context (e.g., Specified Amount vs. Equal Parts)
        public PeriodUnit PeriodUnit { get; set; }          // Frequency interval metric spacing (Days, Months, Years)
        public int QtyUnit { get; set; }                    // Interval numeric multiplier count (e.g., every '2' Months)

        // ==========================================================
        // Subsidiary Tax & Charge Distribution Matrices
        // ==========================================================
        // Enum Properties
        public PaymSchedTaxDist TaxDistribution { get; set; } // Dictates if tax is fully allocated to the 1st installment or split proportionally
        public PaymSchedMiscChargeDist McrMiscChargeDist { get; set; } // Strategic distribution rule for miscellaneous sales charges

        // ==========================================================
        // Commerce / Call Center Integration Limits (MCR)
        // ==========================================================
        // Basic Properties
        public decimal McrMinOrderValue { get; set; }
        public decimal McrMaxOrderValue { get; set; }
        public int McrMinNumInstallments { get; set; }
        public int McrMaxNumInstallments { get; set; }

        // Enum Properties
        public NoYes McrFlexiblePlan { get; set; } // Flag allowing runtime user alteration of standard installment intervals
    }
}
