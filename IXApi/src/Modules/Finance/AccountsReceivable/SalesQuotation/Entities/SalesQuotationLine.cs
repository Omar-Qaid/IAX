using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory;


namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("SalesQuotationLine")]
    public class SalesQuotationLine : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.QuotationId)]
        public string QuotationId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        public int LineCreationSequenceNumber { get; set; }
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
        [StringLength(FieldLengths.ReferenceId)]
        public string CustomerRef { get; set; } = string.Empty;
        public DateTime TransDate { get; set; }
        [StringLength(FieldLengths.Company)]
        public string Company { get; set; } = string.Empty;

        // Enum Properties
        public QuotationType QuotationType { get; set; }
        public QuotationStatus QuotationStatus { get; set; }
        public QuotationLineCreationMethod QuotationLineCreationMethod { get; set; }

        // ==========================================================
        // Customer & Account
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CustAccount)]
        public string CustAccount { get; set; } = string.Empty;
        public long DeliveryPostalAddress { get; set; }
        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public long AddressRefRecId { get; set; }
        public int AddressRefTableId { get; set; }

        // ==========================================================
        // Item & Product
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;
        public int ItemTagging { get; set; }
        [StringLength(FieldLengths.UnitId)]
        public string SalesUnit { get; set; } = string.Empty;
        public decimal PriceUnit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal OrigCostPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal SalesMarkup { get; set; }
        public decimal NewSalesPrice { get; set; }
        public decimal NewTotalContributionRatio { get; set; }

        // Enum Properties
        public NoYes StockedProduct { get; set; }

        // ==========================================================
        // Inventory
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        public decimal InventDeliverNow { get; set; }

        // Enum Properties
        public InventRefType InventRefType { get; set; }

        // ==========================================================
        // Quantities
        // ==========================================================
        // Basic Properties
        public decimal QtyOrdered { get; set; }
        public decimal SalesQty { get; set; }
        public decimal SalesDeliverNow { get; set; }
        public decimal RemainSalesPhysical { get; set; }
        public decimal RemainSalesFinancial { get; set; }
        public decimal RemainInventPhysical { get; set; }

        // ==========================================================
        // Pricing & Discounts
        // ==========================================================
        // Basic Properties
        public decimal LineAmount { get; set; }
        public decimal LineDisc { get; set; }
        public decimal LinePercent { get; set; }
        public decimal MultiLnDisc { get; set; }
        public decimal MultiLnPercent { get; set; }
        public decimal OverDeliveryPct { get; set; }
        public decimal UnderDeliveryPct { get; set; }
        public long McrOrderLine2PriceHistoryRef { get; set; }

        // ==========================================================
        // Delivery
        // ==========================================================
        // Basic Properties
        public DateTime ConfirmedDlv { get; set; }
        public DateTime ShippingDateRequested { get; set; }
        public DateTime ReceiptDateRequested { get; set; }
        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;

        // Enum Properties
        public SalesDeliveryType LineDeliveryType { get; set; }
        public SalesDlvDateControlType DeliveryDateControlType { get; set; }

        // ==========================================================
        // Financial & Ledger
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }
        public long OffsetLedgerDimension { get; set; }
        [StringLength(FieldLengths.OffsetCompany)]
        public string OffsetCompany { get; set; } = string.Empty;

        // Enum Properties
        public LedgerJournalACType AccountType { get; set; }
        public LedgerJournalACType OffsetAccountType { get; set; }

        // ==========================================================
        // Tax
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;

        // Enum Properties
        public NoYes OverrideSalesTax { get; set; }
        public NoYes TaxAutoGenerated { get; set; }

        // ==========================================================
        // Sales Management & Categories
        // ==========================================================
        // Basic Properties
        public long SalesCategory { get; set; }

        // ==========================================================
        // Project Management
        // ==========================================================
        // Basic Properties
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long ProjectResource { get; set; }
        public long PsaRefRecId { get; set; }
        public int Transferred2Forecast { get; set; }
        public int Transferred2ItemReq { get; set; }
        public int Transferred2Journal { get; set; }

        // Enum Properties
        public ProjTransType ProjTransType { get; set; }

        // ==========================================================
        // Product Dimensions (Process Manufacturing / Catch Weight)
        // ==========================================================
        // Basic Properties
        public decimal PdsCwQty { get; set; }
        public decimal PdsCwDeliverNow { get; set; }
        public decimal PdsCwRemainInventPhysical { get; set; }

        // ==========================================================
        // References & Promotions
        // ==========================================================
        // Basic Properties
        public long GupFreeItemLineRecId { get; set; }
        public int IsFreeItemLine { get; set; }
        public long IntrastatCommodity { get; set; }

        // ==========================================================
        // Miscellaneous & System Policy
        // ==========================================================
        // Basic Properties
        public int KittingSkipUpdateHelper { get; set; }
        public int StatTriangularDeal { get; set; }
        public long ManualEntryChangePolicy { get; set; }
        public long SystemEntryChangePolicy { get; set; }

        // Enum Properties
        public WHSCaseTaggingPolicy CaseTagging { get; set; }
        public WHSPalletTaggingPolicy PalletTagging { get; set; }
        public SalesSystemEntrySource SystemEntrySource { get; set; }

        // ==========================================================
        // Packing
        // ==========================================================
        // Basic Properties
        public decimal PackingUnitQty { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(QuotationId))]
//         public virtual SalesQuotationTable? SalesQuotationTable { get; set; }

//         [ForeignKey(nameof(CustAccount))]
//         public virtual CustTable? CustAccount_CustTable { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DimensionAttributeValueCombination { get; set; }

//         [ForeignKey(nameof(InventTransId))]
//         public virtual InventTransOrigin? InventTransOrigin { get; set; }

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? InventDim { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(TaxItemGroup))]
//         public virtual TaxItemGroupHeading? TaxItemGroupHeading { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(OffsetLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? OffsetLedgerDimensionNavigation { get; set; }

        #endregion
    }
}

