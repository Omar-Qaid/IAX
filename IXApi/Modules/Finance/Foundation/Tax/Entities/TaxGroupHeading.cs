using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxGroupHeading")]
    public class TaxGroupHeading : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxGroupName)]
        public string TaxGroupName { get; set; } = string.Empty;

        // Enum Properties
        public TaxGroupSetup TaxGroupSetup { get; set; } // Identifies standard vs. reverse charge group behaviors
        public TaxGroupSource Source { get; set; }       // Identifies core general ledger matrix vs. external tier integration markers

        // ==========================================================
        // Accounting Policies & Cash Discount Treatment
        // ==========================================================
        // Enum Properties
        public TaxGroupRounding TaxGroupRounding { get; set; } // Rounding rule configurations (e.g., Per Line vs. Per Invoice)
        public NoYes TaxReverseOnCashDisc { get; set; }         // Recalculate and reverse tax amount weights when cash discounts are settled

        // ==========================================================
        // Reporting Rules & Localized Chronological Directives
        // ==========================================================
        // Enum Properties
        public NoYes EuTrade_W { get; set; }               // Multi-jurisdictional European Union intra-community trade classification
        public NoYes MandatorySalesDate_W { get; set; }     // Verification rule enforcing explicit sales dates on tax entries
        public NoYes FillSalesDate_W { get; set; }          // Automation toggle mapping source order dates to tax registration sales dates

        // ==========================================================
        // VAT Due Date / Tax Point Resolution Engine
        // ==========================================================
        // Basic Properties
        public int FillVatDueDatePeriodNumber { get; set; } // Numerical offset count for computing forward tax points

        // Enum Properties
        public NoYes FillVatDueDate_W { get; set; }            // Automation toggle calculating fiscal VAT due dates upon transaction posting
        public TaxPointBase FillVatDueDateBasedOn { get; set; } // Underlying logic pivot anchor (e.g., Invoice Date, Delivery Date, Document Date)
        public TaxPeriodUnit FillVatDueDatePeriod { get; set; } // Interval spacing type (Days, Weeks, Months, Years)

        // ==========================================================
        // Document Generation & Print Layout Directives
        // ==========================================================
        // Enum Properties
        public TaxPrintDetail TaxPrintDetail { get; set; } // Controls granular visibility summary options for customer-facing documents
    }
}
