using IAX.IXApi.Modules.Finance.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Transfer order header. A transfer order carries a document lifecycle: stock leaves the
    /// source warehouse into transit on shipment, and leaves transit into the destination on
    /// receipt. That is what distinguishes it from an immediate transfer journal.
    ///
    /// The country and region fields of the D365 table (_RU, _IN, _MX, _HU, _CZ, _LT), the ATP
    /// planning fields and the retail replenishment fields are not carried here.
    /// </summary>
    [Table("InventTransferTable")]
    public class InventTransferTable : Entity<long>
    {
        //----------------------------------------- Document Identity
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TransferId)]
        public string TransferId { get; set; } = string.Empty;

        // Enum Properties
        public InventTransferStatus TransferStatus { get; set; }

        // ==========================================================
        // Source, Transit and Destination Warehouses
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationIdFrom { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationIdTransit { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationIdTo { get; set; } = string.Empty;

        // ==========================================================
        // Delivery
        // ==========================================================
        // Basic Properties
        public DateTime ShipDate { get; set; }
        public DateTime ReceiveDate { get; set; }

        [Required]
        [StringLength(FieldLengths.DlvModeId)]
        public string DlvModeId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTermId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string FromAddressName { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string ToAddressName { get; set; } = string.Empty;

        public long FromPostalAddress { get; set; }
        public long ToPostalAddress { get; set; }
    }
}
