using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("PaymTerm")]
    public class PaymTerm : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PaymTermId)]
        public string PaymTermId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // ==========================================================
        // Chronological Due Date Calculation Engine
        // ==========================================================
        // Basic Properties
        public int NumOfMonths { get; set; }
        public int NumOfDays { get; set; }
        public int CutOffDay { get; set; }
        public int AdditionalMonths { get; set; }

        [Required]
        [StringLength(FieldLengths.PaymDayId)]
        public string PaymDayId { get; set; } = string.Empty; // Calendar payment day constraint link

        // Enum Properties
        public PaymMethod PaymMethod { get; set; } // Pivot logic (e.g., Net, Current Month, COD)

        // ==========================================================
        // Cash and Liquidity Clearing Controls
        // ==========================================================
        // Basic Properties
        public long CashLedgerDimension { get; set; } // Immediate bridging main account combination

        // Enum Properties
        public NoYes Cash { get; set; } // Cash-on-Delivery (COD) immediate clearing flag
        public NoYes PostOffsettingAr { get; set; } // Post offsetting entries within Accounts Receivable parameters

        // ==========================================================
        // Credit Card Processing & Authorization Layouts
        // ==========================================================
        // Enum Properties
        public CreditCardPaymentType CreditCardPaymentType { get; set; }
        public NoYes CreditCardCreditCheck { get; set; }

        // ==========================================================
        // Payment Schedule & Distribution Maps
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PaymSched)]
        public string PaymSched { get; set; } = string.Empty; // Maps rules to installment payment schedules

        // ==========================================================
        // Operational / Sub-ledger Behavior Modifications
        // ==========================================================
        // Enum Properties
        public NoYes CustomerUpdateDueDate { get; set; } // Recalculation allowance flags on open entries
        public NoYes VendorUpdateDueDate { get; set; }

        // ==========================================================
        // Cash Flow Management (CFM) Directives
        // ==========================================================
        // Basic Properties
        public long CfmPaymentRequestTypePayment { get; set; } // Liquidity analysis forecasting node anchor
        public long CfmPaymentRequestTypePrepayment { get; set; }

        // ==========================================================
        // Logistics & Carrier Integration Properties
        // ==========================================================
        // Enum Properties
        public NoYes ShipCarrierCertifiedCheck { get; set; }
        public NoYes ShipCarrierAncillaryCharge { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(CashLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? CashClearingAccountCombination { get; set; }

//         [ForeignKey(nameof(PaymSched))]
//         public virtual PaymSched? AssociatedPaymentSchedule { get; set; }

        #endregion
    }
}
