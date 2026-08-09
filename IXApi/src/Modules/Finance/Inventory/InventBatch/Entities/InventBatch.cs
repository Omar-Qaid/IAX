using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventBatch")]
    public class InventBatch : Entity<long>
    {
        //----------------------------------------- Core Information & Trace Keys
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventBatchId)]
        public string InventBatchId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        // ==========================================================
        // Production & Standard Expiry Timelines
        // ==========================================================
        // Basic Properties
        public DateTime ProdDate { get; set; }
        public DateTime ExpDate { get; set; }

        // ==========================================================
        // Process Manufacturing (PDS) Life-Cycle & Quality Dates
        // ==========================================================
        // Basic Properties
        public DateTime PdsBestBeforeDate { get; set; }
        public DateTime PdsShelfAdviceDate { get; set; }
        public DateTime PdsFinishedGoodsDateTested { get; set; }

        [Required]
        [StringLength(FieldLengths.PdsDispositionCode)]
        public string PdsDispositionCode { get; set; } = string.Empty;

        // Enum Properties
        public NoYes PdsSameLot { get; set; }

        // ==========================================================
        // Vendor Batch Mapping & Lineage Tracking
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PdsVendBatchId)]
        public string PdsVendBatchId { get; set; } = string.Empty;

        public DateTime PdsVendBatchDate { get; set; }
        public DateTime PdsVendExpiryDate { get; set; }
        public long ManufacturerId { get; set; }
        public long OriginManufacturerId { get; set; }

        // Enum Properties
        public NoYes PdsUseVendBatchDate { get; set; }
        public NoYes PdsUseVendBatchExp { get; set; }
        public NoYes PdsInheritBatchAttrib { get; set; }
        public NoYes PdsInheritedShelfLife { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventoryItem { get; set; }

        #endregion
    }
}

