using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("TaxAuthorityAddress")]
    public class TaxAuthorityAddress : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string TaxAuthority { get; set; } = string.Empty; // Primary foreign key code referencing TaxAuthority

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty; // Descriptive label or name of the tax authority branch/office

        [StringLength(50)]
        public string TaxAuthorityId { get; set; } = string.Empty; // Short code identifier for internal authority routing

        public long? Location { get; set; } // Foreign Key link pointing to the LogisticsLocation record for physical address details

        // ==========================================================
        // Contact & Vendor Settlement Links
        // ==========================================================
        // Basic Properties
        [StringLength(20)]
        public string AccountNum { get; set; } = string.Empty; // Associated vendor account number used for posting tax settlement payments

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [StringLength(20)]
        public string Fax { get; set; } = string.Empty;

        [StringLength(20)]
        public string Sms { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telex { get; set; } = string.Empty;

        [StringLength(10)]
        public string Extension { get; set; } = string.Empty;

        [StringLength(20)]
        public string Pager { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        public string Url { get; set; } = string.Empty; // Portal or web address for electronic tax filing

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // ==========================================================
        // Tax Settlement Rounding Engine & Ledger Posting
        // ==========================================================
        // Basic Properties
        public decimal RoundOff { get; set; } // Rounding precision threshold for tax calculations (e.g., 1.00, 0.05)

        public long? RoundOffGainLedgerDimension { get; set; } // Ledger account dimension for posting rounding gain amounts

        public long? RoundOffLossLedgerDimension { get; set; } // Ledger account dimension for posting rounding loss amounts

        // Enum Properties
        public TaxRoundOffType RoundOffType { get; set; } // Rounding rule strategy (e.g., Ordinary, Round Down, Round Up)

        // ==========================================================
        // Report Generation & Print Layout Directives
        // ==========================================================
        // Enum Properties
        public TaxReportLayout TaxReportLayout { get; set; } // Identifies country-specific official tax report layout formats

        public NoYes UseDefaultLayout { get; set; }          // Toggle to force standard reporting layout instead of custom formats

        public NoYes SeparateTaxSummary { get; set; }         // Controls printing of separate summary sheets on tax reports

        public NoYes PrintBlankPage { get; set; }             // Controls printing of trailing blank pages for formal filing layouts


        #region Navigation Properties Row

        //[ForeignKey(nameof(TaxAuthority))]
        //public virtual TaxAuthorityAddress? TaxAuthorityAddressTable { get; set; }

        [ForeignKey(nameof(Location))]
        public virtual LogisticsLocation? AddressLocation { get; set; }

        [ForeignKey(nameof(AccountNum))]
        public virtual VendTable? VendTable { get; set; }

        [ForeignKey(nameof(RoundOffGainLedgerDimension))]
        public virtual DimensionAttributeValueCombination? RoundOffGainLedgerDimensionTable { get; set; }

        [ForeignKey(nameof(RoundOffLossLedgerDimension))]
        public virtual DimensionAttributeValueCombination? RoundOffLossLedgerDimensionTable { get; set; }

        #endregion
    }
}