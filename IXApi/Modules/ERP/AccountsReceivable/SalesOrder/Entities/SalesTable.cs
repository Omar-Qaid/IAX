using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.ERP.Shared.Features;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using IAX.IXApi.Modules.ERP.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using IAX.IXApi.Modules.ERP.Inventory;


namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("SalesTable")]
    public class SalesTable : Entity<long>
    {

        //----------------------------------------- Core Information

        [Required]
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string SalesName { get; set; } = string.Empty;

        [StringLength(FieldLengths.NameAlias)]
        public string SalesNameAlias { get; set; } = string.Empty;
        public SalesStatus SalesStatus { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public SalesType? SalesType { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        [StringLength(FieldLengths.TaxGroupId)]
        public string TaxGroupId { get; set; } = string.Empty;

        [StringLength(FieldLengths.AccountNum)]
        public string CustAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.CustGroupId)]
        public string CustGroup { get; set; } = string.Empty;

        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;

        [StringLength(FieldLengths.PaymTermId)]
        public string PaymTerm { get; set; } = string.Empty;

        [StringLength(FieldLengths.PaymModeId)]
        public string PaymMode { get; set; } = string.Empty;

        [StringLength(FieldLengths.SalesPoolId)]
        public string SalesPoolId { get; set; } = string.Empty;

        [StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;

        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;

        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;

        [StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;

        [StringLength(FieldLengths.VatNum)]
        public string VatNum { get; set; } = string.Empty;
        public long DefaultDimension { get; set; }

        //----------------------------------------- Customer
        public NoYes OneTimeCustomer { get; set; }
      
        [StringLength(FieldLengths.ReferenceId)]
        public string CustomerRef { get; set; } = string.Empty;
        [StringLength(FieldLengths.Description)]
        public string CustRequisitionNum { get; set; } = string.Empty;



        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;
        //----------------------------------------- Sales & Customer
   
        public long WorkerSalesResponsible { get; set; }
        public long WorkerSalesTaker { get; set; }

        //----------------------------------------- Addresses

        public long AddressRefRecId { get; set; }
        public int AddressRefTableId { get; set; }
        public long DeliveryPostalAddress { get; set; }
        public long ShipCarrierPostalAddress { get; set; }
        public long SubBillBillToPostalAddress { get; set; }

        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;


      
        //----------------------------------------- Dates

        public DateTime DeliveryDate { get; set; }
        public SalesDlvDateControlType DeliveryDateControlType { get; set; }
        public DateTime ShippingDateRequested { get; set; }
        public DateTime ShippingDateConfirmed { get; set; }
        public DateTime ReceiptDateRequested { get; set; }
        public DateTime ReceiptDateConfirmed { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime ReturnDeadline { get; set; }
        public DateTime CashDiscBaseDate { get; set; }

        public DateTime RevRecContractStartDate { get; set; }
        public DateTime RevRecContractEndDate { get; set; }
 


        //----------------------------------------- Amounts & Pricing

        public decimal DiscPercent { get; set; }
        public decimal DiscTotal { get; set; }
        public decimal Estimate { get; set; }
        public decimal CashDiscPercent { get; set; }
        public int CashDiscBaseDays { get; set; }
        public decimal FixedExchRate { get; set; }
        public decimal ReportingCurrencyFixedExchRate { get; set; }
        public decimal SmmSalesAmountTotal { get; set; }


        //----------------------------------------- Payment & Financial


        public decimal CreditCardApprovalAmount { get; set; }
        public SalesCreditCardAuthorizationError CreditCardAuthorizationError { get; set; }
        public long CreditCardCustRefId { get; set; }


        //----------------------------------------- Financial Dimensions & Accounting

 
        public long AccountingDistributionTemplate { get; set; }
        public long FundingSource { get; set; }
        public long FinTag { get; set; }
        public long TaxId { get; set; }
        public long VatNumRecId { get; set; }
        public int VatNumTableType { get; set; }



        //----------------------------------------- Status


        public CovStatus CovStatus { get; set; }
        public SalesReleaseStatus ReleaseStatus { get; set; }
        public SalesAutoReservation Reservation { get; set; }
        public ReturnStatusHeader ReturnStatus { get; set; }
        public bool ReturnReplacementCreated { get; set; }


        //----------------------------------------- Credit Management

        public bool CredManExcludeSalesOrder { get; set; }
        public bool CredManInCreditControl { get; set; }
        public bool CredManRejected { get; set; }
        public bool CredManReleasedFromCreditControl { get; set; }
        public long CreditNoteReasonCode { get; set; }

        //----------------------------------------- Intercompany

        public bool IntercompanyOrder { get; set; }
        public bool IntercompanyAutoCreateOrders { get; set; }
        public bool IntercompanyDirectDelivery { get; set; }
        public bool IntercompanyDirectDeliveryOrig { get; set; }
        public bool IntercompanyAllowIndirectCreation { get; set; }
        public bool IntercompanyAllowIndirectCreationOrig { get; set; }
        public SalesIntercompanyOrigin IntercompanyOrigin { get; set; }


        //----------------------------------------- Logistics

        public bool InclTax { get; set; }
        public SalesGiroType GiroType { get; set; }
        public SalesFreightSlipType FreightSlipType { get; set; }
        public WHSShipCarrierDlvType ShipCarrierDlvType { get; set; }
        public bool ShipCarrierResidential { get; set; }
        public NoYes ShipCarrierBlindShipment { get; set; }



        //----------------------------------------- Revenue Recognition

        public bool RevRecFollowOriginalPricingMethod { get; set; }
        public bool RevRecMultipleSoReallocation { get; set; }
        public long RevRecLatestReverseJournal { get; set; }
   


        //----------------------------------------- Tags

        public WHSCaseTaggingPolicy CaseTagging { get; set; }
        public int ItemTagging { get; set; }
        public WHSPalletTaggingPolicy PalletTagging { get; set; }


        //----------------------------------------- References

        public long SourceDocumentHeader { get; set; }
        public long TransportationDocument { get; set; }
        public long RetailChannelTable { get; set; }
        public long ServiceCodeRefRecId { get; set; }
        public long DirectDebitMandate { get; set; }
        public long ManualEntryChangePolicy { get; set; }
        public long SystemEntryChangePolicy { get; set; }
        public SalesSystemEntrySource SystemEntrySource { get; set; }
        public long MatchingAgreement { get; set; }
        public long CreatedTransactionId { get; set; }
        public long ModifiedTransactionId { get; set; }


   


   


        //----------------------------------------- Miscellaneous

        
        public BankDocumentType BankDocumentType { get; set; }
 
        public SalesListCode ListCode { get; set; }
        public bool McrOrderStopped { get; set; }
        public bool MpsExcludeSalesOrder { get; set; }
        public ReqFullCTPStatus MpsFullRunCtpStatus { get; set; }
        public NoYes OverrideSalesTax { get; set; }
        public int PdsBatchAttribAutoRes { get; set; }
        public CustVendSettleVoucher SettleVoucher { get; set; }
        public SysDataStateCode SysDataStateCode { get; set; }



        //----------------------------------------- String References

   

        [StringLength(FieldLengths.Email)]
        public string Email { get; set; } = string.Empty;

 

        [StringLength(FieldLengths.Phone)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(FieldLengths.Num)]
        public string ReturnItemNum { get; set; } = string.Empty;

        [StringLength(FieldLengths.ReasonCodeId)]
        public string ReturnReasonCodeId { get; set; } = string.Empty;

   

        [StringLength(FieldLengths.CredManId)]
        public string CredManId { get; set; } = string.Empty;


        [StringLength(FieldLengths.QuotationId)]
        public string QuotationId { get; set; } = string.Empty;

       

        [StringLength(FieldLengths.PurchOrderFormNum)]
        public string PurchOrderFormNum { get; set; } = string.Empty;

    

        //----------------------------------------- Notes & References

        [StringLength(FieldLengths.Memo)]
        public string Notes { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.Memo)]
        public string ReturnNotes { get; set; } = string.Empty;



        //----------------------------------------- others
        public DateTime DomProcessedDateTime { get; set; }
        [StringLength(FieldLengths.Num)]
        public string RevRecReallocationId { get; set; } = string.Empty;
        [StringLength(FieldLengths.CampaignId)]
        public string SmmCampaignId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string TamRebateReference { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string TamDeductionId { get; set; } = string.Empty;
        [StringLength(FieldLengths.GroupId)]
        public string PdsCustRebateGroupId { get; set; } = string.Empty;
        [StringLength(FieldLengths.GroupId)]
        public string PdsRebateProgramTmaGroup { get; set; } = string.Empty;
        public int SubBillCreatedFromSb { get; set; }
        [StringLength(FieldLengths.Name)]
        public string SubBillBillToName { get; set; } = string.Empty;
        public NoYes SubBillSuppressChild { get; set; }
        public EInvoiceLineSpecification EInvoiceLineSpec { get; set; }
        [StringLength(FieldLengths.Code)]
        public string EInvoiceAccountCode { get; set; } = string.Empty;

        [StringLength(FieldLengths.Num)]
        public string EnterpriseNumber { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string ExportReason { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string StatProcId { get; set; } = string.Empty;
        public CustAutoSummaryModule AutoSummaryModuleType { get; set; }
        [StringLength(FieldLengths.Code)]
        public string NumberSequenceGroup { get; set; } = string.Empty;
        public int GupDelayPricingCalculation { get; set; }
        public int GupSkipPricingCalculation { get; set; }
        public int InvoiceType { get; set; }
        [StringLength(FieldLengths.Code)]
        public string AsohOrderClass { get; set; } = string.Empty;
        [StringLength(FieldLengths.Url)]
        public string Url { get; set; } = string.Empty;

        [StringLength(FieldLengths.SalesOriginId)]
        public string SalesOriginId { get; set; } = string.Empty;

        [StringLength(FieldLengths.PriceGroupId)]
        public string PriceGroupId { get; set; } = string.Empty;

        [StringLength(FieldLengths.CashDisc)]
        public string CashDisc { get; set; } = string.Empty;
        [StringLength(FieldLengths.EndDisc)]
        public string EndDisc { get; set; } = string.Empty;

        [StringLength(FieldLengths.LineDisc)]
        public string LineDisc { get; set; } = string.Empty;

        [StringLength(FieldLengths.CompanyId)]
        public string IntercompanyCompanyId { get; set; } = string.Empty;

        [StringLength(FieldLengths.ProjId)]
        public string ProjId { get; set; } = string.Empty;

        [StringLength(FieldLengths.IntercompanyPurchId)]
        public string IntercompanyPurchId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymentSched)]
        public string PaymentSched { get; set; } = string.Empty;

        [StringLength(FieldLengths.IntercompanyOriginalCustAccount)]
        public string IntercompanyOriginalCustAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.IntercompanyOriginalSalesId)]
        public string IntercompanyOriginalSalesId { get; set; } = string.Empty;
        [StringLength(FieldLengths.ReturnReplacementId)]
        public string ReturnReplacementId { get; set; } = string.Empty;

        [StringLength(FieldLengths.Code)]
        public string CreditCardAuthorization { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymSpec)]
        public string PaymSpec { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string TransactionCode { get; set; } = string.Empty;
        [StringLength(FieldLengths.MarkupGroup)]
        public string MarkupGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string MultiLineDisc { get; set; } = string.Empty;
        [StringLength(FieldLengths.ContactPersonId)]
        public string ContactPersonId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceId)]
        public string CustInvoiceId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.CommissionGroupId)]
        public string CommissionGroup { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.UnitId)]
        public string SalesUnitId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.CountyId)]
        public string CountyOrigDest { get; set; } = string.Empty;
        [StringLength(FieldLengths.ReasonCodeId)]
        public string DlvReason { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string FreightZone { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string Port { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string ShipCarrierAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string ShipCarrierAccountCode { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string ShipCarrierDeliveryContact { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string ShipCarrierId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string ShipCarrierName { get; set; } = string.Empty;
        [StringLength(FieldLengths.Code)]
        public string Transport { get; set; } = string.Empty;
        public DateTime FixedDueDate { get; set; }

        public bool DomIgnore { get; set; }
        public bool DomProcessed { get; set; }
        public SalesDOMExceptionType DomExceptionType { get; set; }
        public int DomIterations { get; set; }
        public int DomProcessedDateTimeTZID { get; set; }
        //----------------------------------------- Navigation Properties (Single)

        #region Navigation Properties Row

//         [ForeignKey(nameof(CustAccount))]
//         public virtual CustTable? CustAccount_CustTable { get; set; }

//         [ForeignKey(nameof(CustGroup))]
//         public virtual CustGroup? CustGroupTable{ get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey("InventLocationId")]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey("InventSiteId")]
//         public virtual InventSite? InventSite { get; set; }

//         [ForeignKey(nameof(InvoiceAccount))]
//         public virtual CustTable? InvoiceAccount_CustTable { get; set; }

      
//         [ForeignKey(nameof(TaxGroupId))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }


//         [ForeignKey(nameof(ContactPersonId))]
//         public virtual ContactPerson? ContactPerson { get; set; }

//         [ForeignKey(nameof(PaymMode))]
//         public virtual CustPaymModeTable? CustPaymModeTable { get; set; }

//         [ForeignKey(nameof(PostingProfile))]
//         public virtual CustLedger? CustLedger { get; set; }

//         [ForeignKey(nameof(SalesPoolId))]
//         public virtual SalesPool? SalesPool { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(PaymTerm))]
//         public virtual PaymTerm? PaymTermTable { get; set; }

//         [ForeignKey(nameof(WorkerSalesResponsible))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesResponsibleEmployee { get; set; }

//         [ForeignKey(nameof(WorkerSalesTaker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesTakerEmployee { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(ShipCarrierPostalAddress))]
//         public virtual LogisticsPostalAddress? ShipCarrierAddress { get; set; }

//         [ForeignKey(nameof(SubBillBillToPostalAddress))]
//         public virtual LogisticsPostalAddress? SubBillBillToAddress { get; set; }

//         [ForeignKey(nameof(PaymentSched))]
//         public virtual PaymSched? PaymentSchedule { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

//         public virtual ICollection<SalesLine> Lines { get; set; } = new List<SalesLine>();

        #endregion


      
    }
}

