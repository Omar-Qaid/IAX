using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

    public class UpdateSalesHeaderInputDto
    {

        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InvoiceAccount)]
        public string? InvoiceAccount { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CurrencyCode)]
        public string? CurrencyCode { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReferenceId)]
        public string CustomerReference { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymTermId)]
        public string PaymentTerms { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvModeId)]
        public string DeliveryMode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvTermId)]
        public string DeliveryTerms { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.NameAlias)]
        public string SalesNameAlias { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesType))]
        public SalesType SalesType { get; set; } = SalesType.Sales;
        public bool OneTimeCustomer { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Email)]
        public string Email { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Phone)]
        public string Phone { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Description)]
        public string CustomerRequisitionNumber { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CampaignId)]
        public string CampaignId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.TaxGroupId)]
        public string TaxGroupId { get; set; } = string.Empty;
        public bool PricesIncludeSalesTax { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ShippingDateRequested { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public string DeliveryPostalAddress { get; set; } = string.Empty;
        public DateTime? ShippingDateConfirmed { get; set; }
        public DateTime? ReceiptDateConfirmed { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDlvDateControlType))]
        public SalesDlvDateControlType DeliveryDateControlType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(ReqFullCTPStatus))]
        public ReqFullCTPStatus MpsFullRunCtpStatus { get; set; }
        public bool BlindShipment { get; set; }
        public bool ResidentialDestination { get; set; }
        public bool ExcludeFromMasterPlanning { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReasonCodeId)]
        public string DeliveryReason { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string ExportReason { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Name)]
        public string ShippingCarrier { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string CarrierId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.MarkupGroup)]
        public string CarrierGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string BrokerId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string TransportMode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(WHSShipCarrierDlvType))]
        public WHSShipCarrierDlvType CarrierService { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymModeId)]
        public string PaymentMethod { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymentSched)]
        public string PaymentSchedule { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymSpec)]
        public string PaymentSpecification { get; set; } = string.Empty;
        public DateTime? FixedDueDate { get; set; }
        public DateTime? PaymentTermsBaseDate { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CashDisc)]
        public string CashDiscountCode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal DiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal TotalDiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal FixedExchangeRate { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PriceGroupId)]
        public string PriceGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.LineDisc)]
        public string LineDiscountGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string MultiLineDiscountGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.EndDisc)]
        public string TotalDiscountGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.MarkupGroup)]
        public string ChargesGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.GroupId)]
        public string CustomerRebateGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.GroupId)]
        public string CustomerTmaGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Num)]
        public string RebateReference { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.SalesPoolId)]
        public string SalesPool { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string CarrierCustomerAccount { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string FreightZone { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Memo)]
        public string Notes { get; set; } = string.Empty;
        public bool IntercompanyAutoCreateOrders { get; set; }
        public bool IntercompanyDirectDelivery { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesIntercompanyOrigin))]
        public SalesIntercompanyOrigin IntercompanyOrigin { get; set; }
        public bool IntercompanyAllowIndirectCreation { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesAutoReservation))]
        public SalesAutoReservation Reservation { get; set; }
    }
