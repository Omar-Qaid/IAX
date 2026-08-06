using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventJournalTrans")]
    public class InventJournalTrans : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Layout
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalId)]
        public string JournalId { get; set; } = string.Empty;

        public decimal LineNum { get; set; }

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        public DateTime TransDate { get; set; }

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        // Enum Properties
        public InventJournalType JournalType { get; set; }

        // ==========================================================
        // Inventory Inventory Dimensions & Sub-Transactions
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ToInventDimId)]
        public string ToInventDimId { get; set; } = string.Empty; // Used in Transfer Journals

        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ToInventTransId)]
        public string ToInventTransId { get; set; } = string.Empty; // Used in Transfer Journals

        [Required]
        [StringLength(FieldLengths.InventTransIdFather)]
        public string InventTransIdFather { get; set; } = string.Empty;

        // Enum Properties
        public InventRefType InventRefType { get; set; }

        // ==========================================================
        // Quantities & Catch Weight Configurations
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal InventOnHand { get; set; }
        public decimal Counted { get; set; }

        [Required]
        [StringLength(FieldLengths.Unit)]
        public string Unit { get; set; } = string.Empty;
        public decimal UnitQty { get; set; }

        public decimal PdscwQty { get; set; }
        public decimal PdscwInventOnHand { get; set; }
        public decimal PdscwInventQtyCounted { get; set; }

        // Enum Properties
        public NoYes PdsCopyBatchAttrib { get; set; }

        // ==========================================================
        // Financial Costings & Pricing Controls
        // ==========================================================
        // Basic Properties
        public decimal CostPrice { get; set; }
        public decimal PriceUnit { get; set; }
        public decimal CostMarkup { get; set; }
        public decimal CostAmount { get; set; }
        public decimal SalesAmount { get; set; }

        // Enum Properties
        public ProfitSet ProfitSet { get; set; }

        // ==========================================================
        // General Ledger Accounts & Dimensions
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }

        // ==========================================================
        // Project Management Module Integration
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ProjId)]
        public string ProjId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProjCategoryId)]
        public string ProjCategoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProjLinePropertyId)]
        public string ProjLinePropertyId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProjSalesCurrencyId)]
        public string ProjSalesCurrencyId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProjUnitId)]
        public string ProjUnitId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProjTransId)]
        public string ProjTransId { get; set; } = string.Empty;

        public decimal ProjSalesPrice { get; set; }

        [Required]
        [StringLength(FieldLengths.ProjTaxGroupId)]
        public string ProjTaxGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ProjTaxItemGroupId)]
        public string ProjTaxItemGroupId { get; set; } = string.Empty;

        // ==========================================================
        // Production, Fixed Assets & Enterprise Asset Operations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ProdGroupId)]
        public string ProdGroupId { get; set; } = string.Empty;

        public long EntAssetWorkOrderLine { get; set; }

        // Enum Properties
        public NoYes BomLine { get; set; }
        public AssetTransType AssetTransType { get; set; }

        // ==========================================================
        // Landed Cost & Subscription Deferrals
        // ==========================================================
        // Basic Properties
        public long SubBillDeferralRecIdOriginal { get; set; }

        // Enum Properties
        public int ItmOverUnderTransfer { get; set; } // Landed Cost over/under status mapping

        // ==========================================================
        // System Audit Details & Workers Contexts
        // ==========================================================
        // Basic Properties
        public long Worker { get; set; }
        public long ReasonRefRecId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ReleaseDateTzId { get; set; }
        public int SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(JournalId))]
//         public virtual InventJournalTable? JournalHeader { get; set; }

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? Dimensions { get; set; }

//         [ForeignKey(nameof(ToInventDimId))]
//         public virtual InventDim? ToDimensions { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? AccountCombination { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionSet { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(Unit))]
//         public virtual UnitOfMeasure? UnitOfMeasure { get; set; }

//         [ForeignKey(nameof(ProjTaxGroupId))]
//         public virtual TaxGroupHeading? ProjTaxGroup { get; set; }

//         [ForeignKey(nameof(ProjTaxItemGroupId))]
//         public virtual TaxItemGroupHeading? ProjTaxItemGroup { get; set; }

//         [ForeignKey(nameof(Worker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? Employee { get; set; }

        #endregion
    }
}

