using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventSerial")]
    public class InventSerial : Entity<long>
    {
        //----------------------------------------- Core Information & Trace Keys
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventSerialId)]
        public string InventSerialId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        // ==========================================================
        // Production Timeline
        // ==========================================================
        // Basic Properties
        public DateTime ProdDate { get; set; }

        #region Navigation Properties Row

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

        #endregion
    }
}
