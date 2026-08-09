using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("VendGroup")]
    public class VendGroup : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string VendGroupCode { get; set; } = string.Empty; // Unique key identifier for the vendor group (mapped from VENDGROUP)

        [Required]
        [StringLength(60)]
        public string Name { get; set; } = string.Empty; // Descriptive label for the vendor group

        // ==========================================================
        // Commercial & Settlement Defaults
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(100)]
        public string PaymTermId { get; set; } = string.Empty; // Default payment terms code applied to new vendors in this group

        [Required]
        [StringLength(100)]
        public string ClearingPeriod { get; set; } = string.Empty; // Identifies settlement period interval configuration

        [Required]
        [StringLength(10)]
        public string TaxGroupId { get; set; } = string.Empty; // Default sales tax group applied to vendors in this group

        // ==========================================================
        // Financial Ledger Dimensions & Exchange Rate Types
        // ==========================================================
        // Basic Properties
        public long? DefaultDimension { get; set; } // Financial dimension set applied by default to vendor transactions

        public long? AccountingCurrencyExchangeRateType { get; set; } // Foreign exchange rate type for accounting currency translation

        public long? ReportingCurrencyExchangeRateType { get; set; } // Foreign exchange rate type for reporting currency translation

        // ==========================================================
        // Number Sequence Governance
        // ==========================================================
        // Basic Properties
        public long? VendAccountNumSeq { get; set; } // Sequence reference ID driving auto-numbering for vendors created under this group


        #region Navigation Properties Row

        [ForeignKey(nameof(PaymTermId))]
        public virtual PaymTerm? PaymTermTable { get; set; }

        [ForeignKey(nameof(AccountingCurrencyExchangeRateType))]
        public virtual ExchangeRateType? AccountingExchangeRateTypeTable { get; set; }

        [ForeignKey(nameof(ReportingCurrencyExchangeRateType))]
        public virtual ExchangeRateType? ReportingExchangeRateTypeTable { get; set; }

        #endregion
    }
}
