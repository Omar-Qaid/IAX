using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventTrans")]
    public class InventTrans : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Links
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        public long InventTransOrigin { get; set; }
        public int InventDimFixed { get; set; }

        // ==========================================================
        // Transaction Statuses & Milestone States
        // ==========================================================
        // Enum Properties
        public StatusIssue StatusIssue { get; set; }
        public StatusReceipt StatusReceipt { get; set; }
        public InventTransOpen ValueOpen { get; set; } // Yes/No whether transaction is financially open

        // ==========================================================
        // Transaction Quantities & Catch Weight Parameters
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal QtySettled { get; set; }
        public decimal PdscwQty { get; set; } // Catch Weight Process Manufacturing Metric
        public decimal PdscwSettled { get; set; }

        // ==========================================================
        // Financial Values & Cost Breakdowns
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal CostAmountPosted { get; set; }
        public decimal CostAmountPhysical { get; set; }
        public decimal CostAmountAdjustment { get; set; }
        public decimal CostAmountSettled { get; set; }
        public decimal CostAmountStd { get; set; }
        public decimal CostAmountOperations { get; set; }
        public decimal RevenueAmountPhysical { get; set; }
        public decimal TaxAmountPhysical { get; set; }

        // ==========================================================
        // Sub-Ledger Posting Audit Anchors (Vouchers & Invoices)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.VoucherPhysical)]
        public string VoucherPhysical { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InvoiceId)]
        public string InvoiceId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PackingSlipId)]
        public string PackingSlipId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes InvoiceReturned { get; set; }
        public NoYes PackingSlipReturned { get; set; }

        // ==========================================================
        // Core Timeline Matrix
        // ==========================================================
        // Basic Properties
        public DateTime DateStatus { get; set; }
        public DateTime DatePhysical { get; set; }
        public DateTime DateFinancial { get; set; }
        public DateTime DateClosed { get; set; }
        public DateTime DateExpected { get; set; }
        public int TimeExpected { get; set; }
        public DateTime DateInvent { get; set; }
        public DateTime ShippingDateRequested { get; set; }
        public DateTime ShippingDateConfirmed { get; set; }

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
        [StringLength(FieldLengths.ProjAdjustRefId)]
        public string ProjAdjustRefId { get; set; } = string.Empty;

        // ==========================================================
        // Sub-Ledger Logistical References (WMS / Production / Intercompany)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PickingRouteId)]
        public string PickingRouteId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TransChildRefId)]
        public string TransChildRefId { get; set; } = string.Empty;

        // Enum Properties
        public InventTransChildType TransChildType { get; set; }
        public NoYes IntercompanyInventDimTransferred { get; set; }

        // ==========================================================
        // Inventory Lineage, Closings & Marking Pointers
        // ==========================================================
        // Basic Properties
        public long MarkingRefInventTransOrigin { get; set; }
        public long ReturnInventTransOrigin { get; set; }
        public long NonFinancialTransferInventClosing { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? Dimensions { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? TransactionCurrency { get; set; }

//         [ForeignKey(nameof(InventTransOrigin))]
//         public virtual InventTransOrigin? TransactionOriginLink { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(MarkingRefInventTransOrigin))]
//         public virtual InventTransOrigin? MarkingRefOrigin { get; set; }

//         [ForeignKey(nameof(ReturnInventTransOrigin))]
//         public virtual InventTransOrigin? ReturnOrigin { get; set; }

        #endregion
    }
}

