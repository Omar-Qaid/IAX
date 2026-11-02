using IAX.IXApi.Modules.Finance.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Posted transfer order journal: one row per posted shipment or receipt of a transfer order.
    /// This is the document that owns the posting voucher, which is why the voucher no longer has
    /// to be composed from the caller's request identifier.
    ///
    /// The country and region fields of the D365 table and the delivery/address fields are not
    /// carried here.
    /// </summary>
    [Table("InventTransferJour")]
    public class InventTransferJour : Entity<long>
    {
        //----------------------------------------- Document Identity
        [Required]
        [StringLength(FieldLengths.TransferId)]
        public string TransferId { get; set; } = string.Empty;

        /// <summary>The posted journal number, allocated from the transfer journal number sequence.</summary>
        [Required]
        [StringLength(FieldLengths.JournalId)]
        public string JournalId { get; set; } = string.Empty;

        // Enum Properties
        public InventTransferUpdateType UpdateType { get; set; }

        //----------------------------------------- Posting
        public DateTime TransDate { get; set; }

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        /// <summary>
        /// The caller's posting request identifier. D365 has no equivalent field: it prevents a
        /// repeated posting through the line's remaining quantity alone. This column is kept as a
        /// deliberate local extension so the existing exactly-once API contract survives the move
        /// of the voucher onto a number sequence. It is recorded in the module's GAPS.md.
        /// </summary>
        [StringLength(FieldLengths.JournalId)]
        public string PostingRequestId { get; set; } = string.Empty;
    }
}
