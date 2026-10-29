using IAX.IXApi.Modules.Finance.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Inventory posting records: the bridge between an inventory transaction and the ledger
    /// voucher that posted it. One row records which accounts an inventory origin posted to,
    /// on which date and voucher, and whether the row covers the physical or financial update.
    ///
    /// Not every physical inventory update creates a voucher; that depends on the item model
    /// group policy. A row exists only where a posting actually happened.
    /// </summary>
    [Table("InventTransPosting")]
    public class InventTransPosting : Entity<long>
    {
        //----------------------------------------- Posted Inventory Transaction
        // Basic Properties
        public long InventTransOrigin { get; set; }

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        // Enum Properties
        public InventTransPostingType InventTransPostingType { get; set; }
        public NoYes IsPosted { get; set; }

        // ==========================================================
        // Ledger Accounts & Posting Types
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }
        public long OffsetLedgerDimension { get; set; }
        public long DefaultDimension { get; set; }

        // Enum Properties
        public LedgerPostingType PostingType { get; set; }
        public LedgerPostingType PostingTypeOffset { get; set; }

        // ==========================================================
        // Voucher & Chronology
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        public DateTime TransDate { get; set; }
        public DateTime TransBeginTime { get; set; }
        public int TransBeginTimeTzId { get; set; }

        [Required]
        [StringLength(FieldLengths.ProjId)]
        public string ProjId { get; set; } = string.Empty;

        public int SysDataStateCode { get; set; }
    }
}
