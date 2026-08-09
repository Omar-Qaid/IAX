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
    [Table("SalesLine")]
    public class SalesLine : Entity<long>
    {
        //----------------------------------------- Core Information
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
        [StringLength(FieldLengths.ReferenceId)]
        public string CustomerRef { get; set; } = string.Empty;
        public int CustomerLineNum { get; set; }
        public int Complete { get; set; }
        public SalesLineBlocked Blocked { get; set; }

        // ==========================================================
        // Customer
        // ==========================================================
        [StringLength(FieldLengths.CustAccount)]
        public string CustAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.CustGroupId)]
        public string CustGroupId { get; set; } = string.Empty;
        public long DeliveryPostalAddress { get; set; }
        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public long AddressRefRecId { get; set; }
        public int AddressRefTableId { get; set; }

        // ==========================================================
        // Item
        // ==========================================================
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;
        public int ItemReplaced { get; set; }
        public int ItemTagging { get; set; }
        public NoYes StockedProduct { get; set; } 
        [StringLength(FieldLengths.UnitId)]
        public string SalesUnit { get; set; } = string.Empty;
        public decimal PriceUnit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal SalesMarkup { get; set; }

        // ==========================================================
        // Inventory
        // ==========================================================
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransIdReturn { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.ReferenceId)]
        public string InventRefId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.InventTransId)]
        public string InventRefTransId { get; set; } = string.Empty;
        
        public InventRefType InventRefType { get; set; } // Changed from int to Enum
        public decimal InventDeliverNow { get; set; }
        public int InventoryServiceAutoOffset { get; set; }

        // ==========================================================
        // Quantities
        // ==========================================================
        public decimal QtyOrdered { get; set; }
        public decimal SalesQty { get; set; }
        public decimal SalesDeliverNow { get; set; }
        public decimal RemainSalesPhysical { get; set; }
        public decimal RemainSalesFinancial { get; set; }
        public decimal RemainInventPhysical { get; set; }
        public decimal RemainInventFinancial { get; set; }
        public decimal ExpectedRetQty { get; set; }

        // ==========================================================
        // Pricing & Discounts
        // ==========================================================
        public decimal LineAmount { get; set; }
        public decimal LineDisc { get; set; }
        public decimal LinePercent { get; set; }
        public decimal MultiLnDisc { get; set; }
        public decimal MultiLnPercent { get; set; }
        public decimal OverDeliveryPct { get; set; }
        public decimal UnderDeliveryPct { get; set; }
        public decimal McrMarginPercent { get; set; }

        // ==========================================================
        // Delivery
        // ==========================================================
        public DateTime ConfirmedDlv { get; set; }
        public DateTime ShippingDateRequested { get; set; }
        public DateTime ShippingDateConfirmed { get; set; }
        public DateTime ReceiptDateRequested { get; set; }
        public DateTime ReceiptDateConfirmed { get; set; }
        public SalesDeliveryType DeliveryType { get; set; } // Changed from int to Enum
        public SalesDeliveryType LineDeliveryType { get; set; }
        public SalesDlvDateControlType DeliveryDateControlType { get; set; } // Changed from int to Enum
        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;

        // ==========================================================
        // Shipping
        // ==========================================================
        public WHSShipCarrierDlvType ShipCarrierDlvType { get; set; } // Changed from int to Enum
        public long ShipCarrierPostalAddress { get; set; }

        // ==========================================================
        // Financial
        // ==========================================================
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public long LedgerDimension { get; set; }
        public long DefaultDimension { get; set; }
        public long AccountingDistributionTemplate { get; set; }
        public NoYes OverrideSalesTax { get; set; } // Changed from int to Enum

        // ==========================================================
        // Tax
        // ==========================================================
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;
        public long TaxId { get; set; }
        public NoYes TaxAutoGenerated { get; set; } // Changed from int to Enum

        // ==========================================================
        // Sales
        // ==========================================================
        public SalesStatus SalesStatus { get; set; } // Changed from int to Enum
        public SalesType SalesType { get; set; } // Changed from int to Enum
        public SalesOrderCreationMethod SalesSalesOrderCreationMethod { get; set; } // Changed from int to Enum
        [StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;
        
        public long SalesCategory { get; set; }
        [StringLength(FieldLengths.PurchOrderFormNum)]
        public string PurchOrderFormNum { get; set; } = string.Empty;
        public SalesServiceLineType ServiceLineType { get; set; }
        public SalesDOMExceptionType DomExceptionType { get; set; }

        // ==========================================================
        // Reservation
        // ==========================================================
        public SalesAutoReservation Reservation { get; set; } // Changed from int to Enum
        public WHSSoftReserveBlockLevel SoftReserveBlockLevel { get; set; } // Changed from int to Enum
        public int IsSoftReservedExternally { get; set; }
        public int ReturnAllowReservation { get; set; }

        // ==========================================================
        // Return
        // ==========================================================
        public ReturnStatusHeader ReturnStatus { get; set; } // Changed from int to Enum
        public DateTime ReturnArrivalDate { get; set; }
        public DateTime ReturnDeadline { get; set; }
        public DateTime ReturnClosedDate { get; set; }
        [StringLength(FieldLengths.PdsDispositionCode)]
        public string ReturnDispositionCodeId { get; set; } = string.Empty;
        
        public long RefReturnInvoiceTransW { get; set; }

        // ==========================================================
        // Revenue Recognition
        // ==========================================================
        public int RevRecBundle { get; set; }
        public SalesStatus RevRecBundleSalesStatus { get; set; } // Changed from int to Enum
        public int RevRecIsBundleComponent { get; set; }
        public int RevRecOccurrences { get; set; }
        public DateTime RevRecContractStartDate { get; set; }
        public DateTime RevRecContractEndDate { get; set; }

        // ==========================================================
        // Bundle
        // ==========================================================
        public int BundleLineStatus { get; set; }
        public RevRecBundleLineType BundleLineType { get; set; } // Changed from int to Enum
        public decimal RevRecBundleQty { get; set; }
        public decimal RevRecBundleQtyOrdered { get; set; }
        public decimal RevRecBundleSalesPrice { get; set; }
        public decimal RevRecBundleNetAmount { get; set; }
        public decimal RevRecBundleRatio { get; set; }

        // ==========================================================
        // Product Dimensions (Process Manufacturing)
        // ==========================================================
        public decimal PdsCwQty { get; set; }
        public decimal PdsCwExpectedRetQty { get; set; }
        public decimal PdsCwInventDeliverNow { get; set; }
        public decimal PdsCwRemainInventPhysical { get; set; }
        public decimal PdsCwRemainInventFinancial { get; set; }
        public int PdsBatchAttribAutoRes { get; set; }
        public int PdsExcludeFromRebate { get; set; }
        public int PdsSameLot { get; set; }
        public int PdsSameLotOverride { get; set; }

        // ==========================================================
        // Planning
        // ==========================================================
        public decimal PlanningPriority { get; set; }
        public int MpsExcludeSalesLine { get; set; }
        public ReqFullCTPStatus MpsFullRunCtpStatus { get; set; } // Changed from int to Enum

        // ==========================================================
        // Intercompany
        // ==========================================================
        public SalesIntercompanyOrigin IntercompanyOrigin { get; set; } // Changed from int to Enum

        // ==========================================================
        // References
        // ==========================================================
        public long MatchingAgreementLine { get; set; }
        public long ManualEntryChangePolicy { get; set; }
        public long CreditNoteReasonCode { get; set; }
        public long FinTag { get; set; }
        public long ProjFundingSource { get; set; }
        public long IntrastatCommodity { get; set; }
        public long SourceDocumentLine { get; set; }

        // ==========================================================
        // Miscellaneous
        // ==========================================================
        public int AgreementSkipAutoLink { get; set; }
        public WHSCaseTaggingPolicy CaseTagging { get; set; } // Changed from int to Enum
        public int KittingSkipUpdateHelper { get; set; }
        public WHSPalletTaggingPolicy PalletTagging { get; set; } // Changed from int to Enum
        public int Scrap { get; set; }
        public SalesLineSourcingOrigin SourcingOrigin { get; set; } // Changed from int to Enum
        public SysDataStateCode SysDataStateCode { get; set; } // Changed from int to Enum
        public long SystemEntryChangePolicy { get; set; }
        public SalesSystemEntrySource SystemEntrySource { get; set; } // Changed from int to Enum
        public int StAtTriangularDeal { get; set; }
        public int TamRebateExcludeRebateManagement { get; set; }
        public int UnbilledRevenueCredit { get; set; }

        // ==========================================================
        // Electronic Invoice
        // ==========================================================
        [StringLength(FieldLengths.Code)]
        public string EInvoiceAccountCode { get; set; } = string.Empty;

        // ==========================================================
        // Packing
        // ==========================================================
        [StringLength(FieldLengths.UnitId)]
        public string PackingUnit { get; set; } = string.Empty;
        public decimal PackingUnitQty { get; set; }

        // ==========================================================
        // Project
        // ==========================================================
        public decimal PsaProjProposalQty { get; set; }
        public decimal PsaProjProposalInventQty { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(CustAccount))]
//         public virtual CustTable? CustAccount_CustTable { get; set; }

//         [ForeignKey(nameof(InventTransId))]
//         public virtual InventTransOrigin? InventTransOrigin { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency Currency { get; set; }

//         [ForeignKey(nameof(CustGroupId))]
//         public virtual CustGroup CustGroup { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DimensionAttributeValueCombination { get; set; }

//         [ForeignKey(nameof(DlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(TaxItemGroup))]
//         public virtual TaxItemGroupHeading? TaxItemGroupHeading { get; set; }

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? InventDim { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(ShipCarrierPostalAddress))]
//         public virtual LogisticsPostalAddress? ShipCarrierAddress { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

//         public virtual ICollection<SalesLine> Lines { get; set; } = new List<SalesLine>();

        #endregion
    }
}

