using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("BankTransType")]
    public class BankTransType : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BankTransType)]
        public string BankTransactionType { get; set; } = string.Empty; // Renamed from BANKTRANSTYPE to prevent conflict with class name

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Ledgers & Financial Clearing
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DefaultLedgerAccount { get; set; }

        #endregion
    }
}
