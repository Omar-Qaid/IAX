using IAX.IXApi.Modules.Finance.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Transfer order line. The four inventory transaction identifiers are what make transit
    /// real: the issue from the source, the two transit legs, and the receipt into the
    /// destination each own their inventory transaction.
    ///
    /// The country and region fields, the catch-weight (PDSCW) quantities, the ATP planning
    /// fields and the retail replenishment fields of the D365 table are not carried here.
    /// </summary>
    [Table("InventTransferLine")]
    public class InventTransferLine : Entity<long>
    {
        //----------------------------------------- Line Identity
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TransferId)]
        public string TransferId { get; set; } = string.Empty;

        public decimal LineNum { get; set; }

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.UnitId)]
        public string UnitId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        // ==========================================================
        // Inventory Transaction References
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;            // Issue from the source

        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransIdTransitTo { get; set; } = string.Empty;   // Receipt into transit

        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransIdTransitFrom { get; set; } = string.Empty; // Issue out of transit

        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransIdReceive { get; set; } = string.Empty;     // Receipt into the destination

        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransIdScrap { get; set; } = string.Empty;

        // ==========================================================
        // Shipment and Receipt Quantities
        // ==========================================================
        // Basic Properties
        public decimal QtyTransfer { get; set; }
        public decimal QtyShipNow { get; set; }
        public decimal QtyShipped { get; set; }
        public decimal QtyRemainShip { get; set; }
        public decimal QtyReceiveNow { get; set; }
        public decimal QtyReceived { get; set; }
        public decimal QtyRemainReceive { get; set; }
        public decimal QtyScrapped { get; set; }

        // ==========================================================
        // Delivery Tolerance & Dates
        // ==========================================================
        // Basic Properties
        public DateTime ShipDate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public decimal OverDeliveryPct { get; set; }
        public decimal UnderDeliveryPct { get; set; }

        // Enum Properties
        public InventTransferRemainStatus RemainStatus { get; set; }
    }
}
