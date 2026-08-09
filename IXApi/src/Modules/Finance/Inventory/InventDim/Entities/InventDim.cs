using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventDim")]
    public class InventDim : Entity<long>
    {
        //----------------------------------------- Core Identity & Cryptographic Hashing
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Sha1HashHex)]
        public string Sha1HashHex { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Sha3HashHex)]
        public string Sha3HashHex { get; set; } = string.Empty;

        // ==========================================================
        // Product Dimensions (Variants configuration)
        // ==========================================================
        // Basic Properties
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

        // ==========================================================
        // Storage Dimensions (Logistical Framework)
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

        // ==========================================================
        // Tracking Dimensions (Traceability Mechanics)
        // ==========================================================
        // Basic Properties
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
        // Extensibility / Localization Custom Dimensions
        // ==========================================================
        // Basic Properties
        public decimal InventDimension10 { get; set; }
        public DateTime InventDimension9 { get; set; }
        public int InventDimension9TzId { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventBatchId))]
//         public virtual InventBatch? InventoryBatch { get; set; }

//         [ForeignKey(nameof(InventSiteId))]
//         public virtual InventSite? InventSite { get; set; }

//         [ForeignKey(nameof(InventLocationId))]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey(nameof(InventSerialId))]
//         public virtual InventSerial? InventSerial { get; set; }

        #endregion
    }
}

