using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("TaxOnItem")]
    public class TaxOnItem : Entity<long>
    {
        //----------------------------------------- Core Identity & Matrix Mapping
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;


        [Required]
        [StringLength(FieldLengths.TaxExemptCode)]
        public string TaxExemptCode { get; set; } = string.Empty;


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxItemGroup))]
        public virtual TaxItemGroupHeading? TaxItemGroupHeadingTable { get; set; }

        [ForeignKey(nameof(TaxCode))]
        public virtual TaxTable? TaxTable { get; set; }


        [ForeignKey(nameof(TaxExemptCode))]
        public virtual TaxExemptCodeTable? TaxExemptCodeTable { get; set; }

        

        #endregion
    }
}
