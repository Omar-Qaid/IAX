using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("PaymSchedLine")]
    public class PaymSchedLine : Entity<long>
    {
        //----------------------------------------- Core Identity & Parent Matrix Link
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty; // Foreign Key relation string pointing to PaymSched.Name

        public decimal LineNum { get; set; } // Precision index tracking the evaluation sequence ranking of the lines

        // ==========================================================
        // Installment Weight & Value Allocation Controls
        // ==========================================================
        // Basic Properties
        public int PercentAmount { get; set; } // Percentage portion of the total invoice assigned to this specific line step
        public decimal Value { get; set; }     // Explicit flat amount threshold allocated if a percentage allocation is not utilized

        // ==========================================================
        // Chronological Interval Offsets
        // ==========================================================
        // Basic Properties
        public int Qty { get; set; } // Numerical wait interval before execution (e.g., '1' period spacing unit from origin)

        // ==========================================================
        // Sub-System Operational & Cash Flow Flags
        // ==========================================================
        // Enum Properties
        public NoYes CfmPrepayment { get; set; } // Cash Flow Management indicator identifying if this row represents a downpayment/prepayment anchor
        public NoYes McrShipping { get; set; }    // Commerce Call Center flag routing delivery or shipping costs specifically to this payment segment


        #region Navigation Properties Row

        [ForeignKey(nameof(Name))]
        public virtual PaymSched? PaymSchedTable { get; set; }

        #endregion
    }
}

