using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

    public class SalesLineInputDto
    {

        [System.ComponentModel.DataAnnotations.Required]
        public string ItemNumber { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.000001", "1000000000")]
        public decimal Quantity { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal UnitPrice { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? Description { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? Unit { get; set; }
        public DateTime? DeliveryDate { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesType))]
        public SalesType? LineType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDeliveryType))]
        public SalesDeliveryType? DeliveryType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(InventRefType))]
        public InventRefType? ItemReferenceType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesLineSourcingOrigin))]
        public SalesLineSourcingOrigin? SourcingOrigin { get; set; }
        public bool? ExcludeFromMasterPlanning { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDeliveryType))]
        public SalesDeliveryType? LineDeliveryType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDlvDateControlType))]
        public SalesDlvDateControlType? DeliveryDateControlType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(ReqFullCTPStatus))]
        public ReqFullCTPStatus? MpsFullRunCtpStatus { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(WHSShipCarrierDlvType))]
        public WHSShipCarrierDlvType? ShipCarrierDlvType { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal? PlanningPriority { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesIntercompanyOrigin))]
        public SalesIntercompanyOrigin? IntercompanyOrigin { get; set; }
        public bool? Stopped { get; set; }
        public bool? PreventPartialDelivery { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(long), "0", "9223372036854775807")]
        public long SalesCategory { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? InventSiteId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? InventLocationId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ConfigId)]
        public string? ConfigId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventSizeId)]
        public string? InventSizeId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventColorId)]
        public string? InventColorId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventStyleId)]
        public string? InventStyleId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventVersionId)]
        public string? InventVersionId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventBatchId)]
        public string? BatchNumber { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventSerialId)]
        public string? SerialNumber { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvModeId)]
        public string? DeliveryMode { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvTermId)]
        public string? DeliveryTerms { get; set; }
        public DateTime? ShippingDateRequested { get; set; }
        public DateTime? ShippingDateConfirmed { get; set; }
        public DateTime? ReceiptDateConfirmed { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal OverDeliveryPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal UnderDeliveryPercent { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Name)]
        public string? DeliveryName { get; set; }
        public string? DeliveryPostalAddress { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventTransId)]
        public string? ReturnLotId { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesAutoReservation))]
        public SalesAutoReservation? Reservation { get; set; }
        public bool? AutoBatchReservation { get; set; }
        public bool? SameBatchSelection { get; set; }
        public bool? Scrap { get; set; }
        public long? LedgerDimension { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.SalesGroupId)]
        public string? SalesGroup { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReferenceId)]
        public string? CustomerReference { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int? CustomerLineNumber { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.UnitId)]
        public string? PackingUnit { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal PackingUnitQuantity { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.000001", "1000000000")]
        public decimal? PriceUnit { get; set; }
        public bool UsePriceAgreement { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal LineDiscount { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal LineDiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal MultiLineDiscount { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal MultiLineDiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal SalesMarkup { get; set; }
        public bool? ExcludeFromRebate { get; set; }
        public bool? ExcludeFromRebateManagement { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.TaxGroup)]
        public string? TaxGroup { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.TaxItemGroup)]
        public string? TaxItemGroup { get; set; }
    }
