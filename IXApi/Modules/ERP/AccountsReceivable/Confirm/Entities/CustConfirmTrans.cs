using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Modules.ERP.Inventory;


namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustConfirmTrans")]
    public class CustConfirmTrans : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.ConfirmId)]
        public string ConfirmId { get; set; } = string.Empty;
        public DateTime ConfirmDate { get; set; }
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;
        [StringLength(FieldLengths.SalesId)]
        public string OrigSalesId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        [StringLength(FieldLengths.Txt)]
        public string LineHeader { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Item & Product
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;
        [StringLength(FieldLengths.UnitId)]
        public string SalesUnit { get; set; } = string.Empty;
        public decimal PriceUnit { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal SalesMarkup { get; set; }

        // Enum Properties
        public NoYes StockedProduct { get; set; }

        // ==========================================================
        // Inventory & Logistics
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        public DateTime DlvDate { get; set; }
        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;

        // ==========================================================
        // Quantities
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal InventQty { get; set; }
        public decimal PdsCwQty { get; set; }

        // ==========================================================
        // Pricing & Discounts
        // ==========================================================
        // Basic Properties
        public decimal LineAmount { get; set; }
        public decimal LineDisc { get; set; }
        public decimal LinePercent { get; set; }
        public decimal DiscPercent { get; set; }
        public decimal DiscAmount { get; set; }
        public decimal MultiLnDisc { get; set; }
        public decimal MultiLnPercent { get; set; }

        // ==========================================================
        // Tax
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;
        public decimal TaxAmount { get; set; }
        public decimal LineAmountTax { get; set; }
        [StringLength(FieldLengths.TaxCode)]
        public string TaxWriteCode { get; set; } = string.Empty;

        // Enum Properties
        public NoYes OverrideSalesTax { get; set; }

        // ==========================================================
        // Financials, Categories & Tracking
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public long DefaultDimension { get; set; }
        public long SalesCategory { get; set; }
        [StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;


        #region Navigation Properties Row

//         [ForeignKey(nameof(ConfirmId))]
//         public virtual CustConfirmJour? CustConfirmJour { get; set; }

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? InventDim { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(TaxItemGroup))]
//         public virtual TaxItemGroupHeading? TaxItemGroupHeading { get; set; }

        #endregion
    }
}
