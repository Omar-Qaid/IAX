using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventModelGroup")]
    public class InventModelGroup : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ModelGroupId)]
        public string ModelGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // Enum Properties
        public InventModel InventModel { get; set; } // Costing methodology: FIFO, LIFO, Weighted Avg, Standard Cost

        // ==========================================================
        // Inventory Ledger Posting Policies
        // ==========================================================
        // Enum Properties
        public NoYes StockedProduct { get; set; }
        public NoYes PostOnHandPhysical { get; set; }
        public NoYes PostOnHandFinancial { get; set; }
        public NoYes PostPhysicalRevenue { get; set; }
        public NoYes PostPhysicalPurchase { get; set; }
        public NoYes StandardCost { get; set; } // Explicit tracking parameter for fixed asset variances

        // ==========================================================
        // Physical & Financial Inventory Controls
        // ==========================================================
        // Enum Properties
        public NoYes MandatoryRegister { get; set; }
        public NoYes MandatoryReceive { get; set; }
        public NoYes MandatoryPick { get; set; }
        public NoYes MandatoryDeduct { get; set; }
        public NoYes MandatoryWmsOrder { get; set; }
        public NoYes NegativePhysical { get; set; }
        public NoYes NegativeFinancial { get; set; }
        public NoYes QuarantineControl { get; set; }

        // ==========================================================
        // Costing Adjustments & Settings
        // ==========================================================
        // Enum Properties
        public NoYes InclPhysicalValueInCost { get; set; }
        public NoYes InventCostRecalculationIncludePhysicalValueForAverageModel { get; set; }

        // ==========================================================
        // Reservation Framework Settings
        // ==========================================================
        // Enum Properties
        public NoYes ItemProdReservationActive { get; set; }
        public ItemProdReservation ItemProdReservation { get; set; }
        public NoYes ReserveByDate { get; set; }
        public NoYes ReserveReversed { get; set; }
        public NoYes McrReservation { get; set; } // Commerce/Call center auto-reservation policy

        // ==========================================================
        // Process Manufacturing (PDS) Batch & Expiry Directives
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PdsDispositionCode)]
        public string PdsDispositionCode { get; set; } = string.Empty;

        // Enum Properties
        public NoYes PdsCheck { get; set; }
        public NoYes PdsConsReq { get; set; }
        public NoYes PdsReqVendBatchDetail { get; set; }
        public NoYes PdsSameLot { get; set; }
        public NoYes PdsVendorCheckItem { get; set; }
        public PdsPickCriteria PdsPickCriteria { get; set; } // FEFO Date criteria tracking (e.g., Expiry, Best before)
        public PickingListBatchExpirationDateValidationRule PickingListBatchExpirationDateValidationRule { get; set; }
    }
}