using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("SubledgerVoucherGeneralJournalEntry")]
    public class SubledgerVoucherGeneralJournalEntry : Entity<long>
    {
        //----------------------------------------- Core Information & Links
        // Basic Properties
        public long GeneralJournalEntry { get; set; }
        public long SubledgerJournalEntry { get; set; }

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.VoucherDataAreaId)]
        public string VoucherDataAreaId { get; set; } = string.Empty;

        // ==========================================================
        // Timeline & Audit Anchors
        // ==========================================================
        // Basic Properties
        public DateTime AccountingDate { get; set; }
        public long TransferId { get; set; }
        public int SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(GeneralJournalEntry))]
//         public virtual GeneralJournalEntry? GeneralJournalEntryDefinition { get; set; }

        // Note: Link to SubledgerJournalEntry can be configured here 
        // if that specific table entity is generated within your module.

        #endregion
    }
}
