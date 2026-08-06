using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DlvMode")]
    public class DlvMode : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Code)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        // Enum Properties
        public WHSShipCarrierDlvType ShipCarrierDlvType { get; set; }

        // ==========================================================
        // Retail, Advanced Warehousing & Charges Groups
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.MarkupGroup)]
        public string MarkupGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.McrExpedite)]
        public string McrExpedite { get; set; } = string.Empty;

        public long DomPriority { get; set; }
    }
}