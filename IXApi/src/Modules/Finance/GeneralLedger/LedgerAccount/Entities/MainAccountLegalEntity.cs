using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Legal entity overrides for a main account: what a company may post to the account and
    /// which dimension and tax defaults apply there.
    ///
    /// The table is shared and keyed by LegalEntity with MainAccount, not partitioned by
    /// DataAreaId. Its France-specific field is not carried here.
    /// </summary>
    [Table("MainAccountLegalEntity")]
    public class MainAccountLegalEntity : SharedEntity<long>
    {
        //----------------------------------------- Account & Owning Company
        // Basic Properties
        public long MainAccount { get; set; }
        public long LegalEntity { get; set; }

        // ==========================================================
        // Manual Entry & Dimension Defaults
        // ==========================================================
        // Basic Properties
        public long DefaultDimension { get; set; } // Dimension values defaulted on entry
        public long FixedDimensions { get; set; }  // Dimension values enforced at posting

        // Enum Properties
        public NoYes DoNotAllowManualEntry { get; set; }
        public NoYes AutoAllocate { get; set; }

        // ==========================================================
        // Tax Defaults & Validation
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BackingRecordDataAreaId)]
        public string TaxTableDataAreaId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BackingRecordDataAreaId)]
        public string TaxItemGroupHeadingDataAreaId { get; set; } = string.Empty;

        // Enum Properties
        public int TaxDirection { get; set; }
        public NoYes TaxFree { get; set; }
        public int ValidateTaxCode { get; set; }

        // ==========================================================
        // Exchange Adjustment
        // ==========================================================
        // Basic Properties
        public long ExchangeAdjustmentRateType { get; set; }
        public long ReportingExchangeAdjustmentRateType { get; set; }
    }
}
