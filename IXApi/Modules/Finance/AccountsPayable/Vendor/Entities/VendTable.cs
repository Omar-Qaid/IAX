using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;
using System.Net.NetworkInformation;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("VendTable")]
    public class VendTable : Entity<long>
    {
        //----------------------------------------- Core Identity & Global Directory Anchor
        // Basic Properties
        [Required]
        [StringLength(20)]
        public string AccountNum { get; set; } = string.Empty; // Unique vendor identification number key

        public long? Party { get; set; } // Foreign key link pointing directly to global DirPartyTable details
        
   

        [Required]
        [StringLength(10)]
        public string VendGroup { get; set; } = string.Empty; // Grouping identifier for posting defaults and sub-ledger setup

        [Required]
        [StringLength(20)]
        public string InvoiceAccount { get; set; } = string.Empty; // Alternative invoicing/parent vendor account for settlement

        // ==========================================================
        // Payment Terms, Cash Discounts & Banking Defaults
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(100)]
        public string PaymTermId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PaymMode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PaymSpec { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PaymDayId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CashDisc { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string BankAccount { get; set; } = string.Empty; // Default operational bank account ID

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;

        public decimal CreditMax { get; set; }
        public long LvPaymTransCodes { get; set; }

        // Enum Properties
        public VendTableUseCashDisc UseCashDisc { get; set; }

        // ==========================================================
        // Financial Ledger Dimensions & Default Counterparts
        // ==========================================================
        // Basic Properties
        public long? DefaultDimension { get; set; }
        public long? OffsetLedgerDimension { get; set; }

        // Enum Properties
        public LedgerJournalACType OffsetAccountType { get; set; }

        // ==========================================================
        // Logistics, Shipping & Delivery Defaults
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string DlvTerm { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DlvMode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string InventSiteId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string InventLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DefaultInventStatusId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DestinationCodeId { get; set; } = string.Empty;

        // ==========================================================
        // Pricing, Discounts & Procurement Groupings
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string PriceGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string LineDisc { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string MultiLineDisc { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string EndDisc { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PurchPoolId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ItemBuyerGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string MarkupGroup { get; set; } = string.Empty;

        // ==========================================================
        // Tax, VAT & Withholding Configurations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string TaxGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string TaxWithholdGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string VatNum { get; set; } = string.Empty;

        public long VatNumRecId { get; set; }

        [Required]
        [StringLength(16)]
        public string FiscalCode { get; set; } = string.Empty;

        public decimal TaxVendorChargeTaxToleranceAmount { get; set; }
        public decimal TaxVendorChargeTaxTolerancePercent { get; set; }

        // Enum Properties
        public NoYes InclTax { get; set; }
        public NoYes OverrideSalesTax { get; set; }
        public NoYes TaxWithholdCalculate { get; set; }
        public AccrueSalesTaxType AccrueSalesTaxType { get; set; }
        public TaxVendorChargeTaxToleranceValidation TaxVendorChargeTaxToleranceValidation { get; set; }
        public VatNumTableType VatNumTableType { get; set; }

        // ==========================================================
        // United States 1099, W9 & Regulatory Reporting
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(11)]
        public string Tax1099RegNum { get; set; } = string.Empty;

        public long Tax1099Fields { get; set; }

        // Enum Properties
        public NoYes Tax1099Reports { get; set; }
        public NoYes W9 { get; set; }
        public NoYes W9Included { get; set; }
        public NoYes SecondTin { get; set; }
        public NoYes FatcaFilingRequirement { get; set; }
        public NoYes ForeignEntityIndicator { get; set; }
        public Tax1099NameChoice Tax1099NameChoice { get; set; }
        public TaxIdType TaxIdType { get; set; }

        // ==========================================================
        // Diversity, Ownership & Socio-Economic Classifications
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(15)]
        public string EthnicOriginId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ResidenceForeignCountryRegionId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes SmallBusiness { get; set; }
        public NoYes MinorityOwned { get; set; }
        public NoYes FemaleOwned { get; set; }
        public NoYes VeteranOwned { get; set; }
        public NoYes DisabledOwned { get; set; }
        public NoYes HubZone { get; set; }
        public NoYes LocallyOwned { get; set; }

        // ==========================================================
        // Vendor Collaboration, Portal & Order Processing
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(20)]
        public string ContactPersonId { get; set; } = string.Empty;

        public long? MainContactWorker { get; set; }
        public long? VendorPortalAdministratorRecId { get; set; }

        // Enum Properties
        public NoYes CxmlOrderEnable { get; set; }
        public NoYes BidOnly { get; set; }
        public NoYes OneTimeVendor { get; set; }
        public VendVendorCollaborationType VendVendorCollaborationType { get; set; }
        public PurchAmountPurchaseOrder PurchAmountPurchaseOrder { get; set; }

        // ==========================================================
        // Matching Policy & Change Request Controls
        // ==========================================================
        // Enum Properties
        public MatchingPolicy MatchingPolicy { get; set; }
        public NoYes ChangeRequestEnabled { get; set; }
        public NoYes ChangeRequestAllowOverride { get; set; }
        public NoYes ChangeRequestOverride { get; set; }

        // ==========================================================
        // Blocked Status, Release Schedules & Governance
        // ==========================================================
        // Basic Properties
        public DateTime BlockedReleaseDate { get; set; }
        public int BlockedReleaseDateTzId { get; set; }

        // Enum Properties
        public CustVendBlocked Blocked { get; set; } // 0: No, 1: Invoice, 2: All, 3: Payment, 4: Requisition
        public WorkflowState WorkflowState { get; set; }

        // ==========================================================
        // International Landed Cost & Miscellaneous Setup
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string LineOfBusinessId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SegmentId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SubSegmentId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string NumberSequenceGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ItmCostTypeGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ItmOverUnderToleranceGroupId { get; set; } = string.Empty;

        public long CompanyNafCode { get; set; }
        public long VendExceptionGroup { get; set; }
        public DateTime CisVerificationDate { get; set; }

        // Enum Properties
        public NoYes ItmImportCostingVendor { get; set; }
        public NoYes ItmServicesProvider { get; set; }
        public ItmVendType ItmVendType { get; set; }
        public CisStatus CisStatus { get; set; }


        #region Navigation Properties Row

        [ForeignKey(nameof(Party))]
        public virtual DirPartyTable? DirPartyTable { get; set; }

        [ForeignKey(nameof(VendGroup))]
        public virtual VendGroup? VendGroupTable { get; set; }

        [ForeignKey(nameof(PaymTermId))]
        public virtual PaymTerm? PaymTermTable { get; set; }

        [ForeignKey(nameof(MainContactWorker))]
        public virtual HcmWorker? HcmWorkerTable { get; set; }

        #endregion
    }
}
