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


namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustInvoiceTrans")]
    public class CustInvoiceTrans : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.InvoiceId)]
        public string InvoiceId { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;
        [StringLength(FieldLengths.SalesId)]
        public string OrigSalesId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        public int LineCreationSequenceNumber { get; set; }
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
        // Inventory & Tracking
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        [StringLength(FieldLengths.ReferenceId)]
        public string InventRefId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.InventTransId)]
        public string InventRefTransId { get; set; } = string.Empty;

        // Enum Properties
        public InventRefType InventRefType { get; set; }

        // ==========================================================
        // Quantities & Logistics
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal QtyPhysical { get; set; }
        public decimal InventQty { get; set; }
        public decimal Remain { get; set; }
        public decimal RemainBefore { get; set; }
        public decimal Weight { get; set; }
        public int CustomerLineNum { get; set; }

        // Enum Properties
        public NoYes PartDelivery { get; set; }

        // ==========================================================
        // Pricing, Financials & Discounts (Transaction Currency)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal LineAmount { get; set; }
        public decimal LineDisc { get; set; }
        public decimal LinePercent { get; set; }
        public decimal DiscPercent { get; set; }
        public decimal DiscAmount { get; set; }
        public decimal MultiLnDisc { get; set; }
        public decimal MultiLnPercent { get; set; }
        public decimal SumLineDisc { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal TotalTax { get; set; }
        public decimal OlapCostValue { get; set; }

        // ==========================================================
        // Financials & Amounts (Accounting Currency - MST)
        // ==========================================================
        // Basic Properties
        public decimal LineAmountMst { get; set; }
        public decimal SumLineDiscMst { get; set; }
        public decimal StatLineAmountMst { get; set; }

        // ==========================================================
        // Tax
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string TaxWriteCode { get; set; } = string.Empty;
        public decimal TaxAmount { get; set; }
        public decimal TaxAmountMst { get; set; }
        public decimal LineAmountTax { get; set; }
        public decimal LineAmountTaxMst { get; set; }

        // Enum Properties
        public NoYes OverrideSalesTax { get; set; }
        public NoYes TaxAutoGenerated { get; set; }
        public NoYes ReverseCharge_W { get; set; }
        public NoYes ReverseChargeSalesList { get; set; }

        // ==========================================================
        // Ledger & Accounting Templates
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }
        public long CustInvoiceLineIdRef { get; set; }

        // ==========================================================
        // Commissions
        // ==========================================================
        // Basic Properties
        public decimal CommissAmountCur { get; set; }
        public decimal CommissAmountMst { get; set; }

        // Enum Properties
        public NoYes CommissCalc { get; set; }

        // ==========================================================
        // Delivery Details (MCR / Standard)
        // ==========================================================
        // Basic Properties
        public DateTime DlvDate { get; set; }
        public long DeliveryPostalAddress { get; set; }
        [StringLength(FieldLengths.Name)]
        public string McrDeliveryName { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.DlvModeId)]
        public string McrDlvMode { get; set; } = string.Empty;

        // Enum Properties
        public SalesDeliveryType DeliveryType { get; set; }

        // ==========================================================
        // Sales Categories & Tracking
        // ==========================================================
        // Basic Properties
        public long SalesCategory { get; set; }
        public long RetailCategory { get; set; }
        [StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string BillingCode { get; set; } = string.Empty;

        // ==========================================================
        // Fixed Assets
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Num)]
        public string AssetId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string AssetBookId { get; set; } = string.Empty;

        // ==========================================================
        // Return Management
        // ==========================================================
        // Basic Properties
        public DateTime ReturnArrivalDate { get; set; }
        public DateTime ReturnClosedDate { get; set; }
        [StringLength(FieldLengths.PdsDispositionCode)]
        public string ReturnDispositionCodeId { get; set; } = string.Empty;

        // ==========================================================
        // Revenue Recognition & Billing Splits
        // ==========================================================
        // Basic Properties
        public DateTime PeriodChargeInvoiceLineBaseFromDate { get; set; }
        public DateTime PeriodChargeInvoiceLineBaseToDate { get; set; }
        public decimal SubBillRevenueSplitParentAmount { get; set; }
        public long SubBillRevenueSplitParentLineRecId { get; set; }

        // Enum Properties
        public NoYes RevRecDeferred { get; set; }
        public NoYes RevRecDeferredProcessed { get; set; }
        public NoYes SubBillRevenueSplit { get; set; }
        public int SubBillRevenueSplitAllocationMethod { get; set; }

        // ==========================================================
        // System Flags & Audit Trailing
        // ==========================================================
        // Basic Properties
        public long ParentRecId { get; set; } // Direct link back to CustInvoiceJour RecId
        public long ReversedRecId { get; set; }
        public long SourceDocumentLine { get; set; }
        public long ReasonRefRecId { get; set; }
        public long FinTag { get; set; }

        // Enum Properties
        public SysDataStateCode SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(ParentRecId))]
//         public virtual CustInvoiceJour? CustInvoiceJour { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DimensionAttributeValueCombination { get; set; }

//         [ForeignKey(nameof(McrDlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? InventDim { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(TaxItemGroup))]
//         public virtual TaxItemGroupHeading? TaxItemGroupHeading { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

        #endregion
    }
}
