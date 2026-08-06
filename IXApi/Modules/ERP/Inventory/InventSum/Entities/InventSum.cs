using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventSum")]
    public class InventSum : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Links
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        // ==========================================================
        // Denormalized Dimension Tracking (Optimized On-Hand Scopes)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.WmsLocationId)]
        public string WmsLocationId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.LicensePlateId)]
        public string LicensePlateId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ConfigId)]
        public string ConfigId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventSizeId)]
        public string InventSizeId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventColorId)]
        public string InventColorId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventStyleId)]
        public string InventStyleId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventVersionId)]
        public string InventVersionId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventBatchId)]
        public string InventBatchId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventSerialId)]
        public string InventSerialId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventStatusId)]
        public string InventStatusId { get; set; } = string.Empty;

        // ==========================================================
        // Standard Inventory On-Hand Quantities
        // ==========================================================
        // Basic Properties
        public decimal PostedQty { get; set; }
        public decimal Deducted { get; set; }
        public decimal Picked { get; set; }
        public decimal Received { get; set; }
        public decimal Registered { get; set; }
        public decimal Arrived { get; set; }
        public decimal Ordered { get; set; }
        public decimal OnOrder { get; set; }
        public decimal ReservPhysical { get; set; }
        public decimal ReservOrdered { get; set; }
        public decimal QuotationReceipt { get; set; }
        public decimal QuotationIssue { get; set; }
        public decimal PhysicalInvent { get; set; }
        public decimal AvailPhysical { get; set; }
        public decimal AvailOrdered { get; set; }

        // ==========================================================
        // Standard Financial Inventory Values
        // ==========================================================
        // Basic Properties
        public decimal PostedValue { get; set; }
        public decimal PhysicalValue { get; set; }

        // ==========================================================
        // Catch Weight (PDS) On-Hand Quantities
        // ==========================================================
        // Basic Properties
        public decimal PdscwPostedQty { get; set; }
        public decimal PdscwDeducted { get; set; }
        public decimal PdscwPicked { get; set; }
        public decimal PdscwReceived { get; set; }
        public decimal PdscwRegistered { get; set; }
        public decimal PdscwArrived { get; set; }
        public decimal PdscwOrdered { get; set; }
        public decimal PdscwOnOrder { get; set; }
        public decimal PdscwReservPhysical { get; set; }
        public decimal PdscwReservOrdered { get; set; }
        public decimal PdscwQuotationReceipt { get; set; }
        public decimal PdscwQuotationIssue { get; set; }
        public decimal PdscwPhysicalInvent { get; set; }
        public decimal PdscwAvailPhysical { get; set; }
        public decimal PdscwAvailOrdered { get; set; }

        // ==========================================================
        // Custom/Localization Extensions
        // ==========================================================
        // Basic Properties
        public decimal InventDimension10 { get; set; }
        public DateTime InventDimension9 { get; set; }
        public int InventDimension9TzId { get; set; }

        // ==========================================================
        // Status Flags & Audit Dates
        // ==========================================================
        // Basic Properties
        public DateTime LastUpdDatePhysical { get; set; }
        public DateTime LastUpdDateExpected { get; set; }

        // Enum Properties
        public NoYes Closed { get; set; }
        public NoYes ClosedQty { get; set; }
        public NoYes IsExcludedFromInventoryValue { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? Dimensions { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(InventSiteId))]
//         public virtual InventSite? InventSite { get; set; }

//         [ForeignKey(nameof(InventLocationId))]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey(nameof(InventBatchId))]
//         public virtual InventBatch? InventBatch { get; set; }

//         [ForeignKey(nameof(InventSerialId))]
//         public virtual InventSerial? InventSerial { get; set; }

        #endregion
    }
}
