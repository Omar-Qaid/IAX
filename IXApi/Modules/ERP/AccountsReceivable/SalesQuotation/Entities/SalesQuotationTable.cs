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
    [Table("SalesQuotationTable")]
    public class SalesQuotationTable : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.QuotationId)]
        public string QuotationId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string QuotationName { get; set; } = string.Empty;
        public DateTime ConfirmDate { get; set; }
        public int IsRevision { get; set; }
        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;

        // Enum Properties
        public QuotationType QuotationType { get; set; }
        public QuotationStatus QuotationStatus { get; set; }
        public QuotationOwnership QuotationOwnership { get; set; }
        public QuotationHeaderCreationMethod QuotationHeaderCreationMethod { get; set; }

        // ==========================================================
        // Customer & Accounts
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CustAccount)]
        public string CustAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.AccountNum)]
        public string BusRelAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string OpportunityId { get; set; } = string.Empty;

        // ==========================================================
        // Delivery & Addressing
        // ==========================================================
        // Basic Properties
        public long DeliveryPostalAddress { get; set; }
        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public long AddressRefRecId { get; set; }
        public int AddressRefTableId { get; set; }
        public DateTime ShippingDateRequested { get; set; }
        public DateTime ReceiptDateRequested { get; set; }
        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;
        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;
        [StringLength(FieldLengths.DlvTermId)]
        public string DeliveryTerms { get; set; } = string.Empty;

        // Enum Properties
        public SalesDlvDateControlType DeliveryDateControlType { get; set; }

        // ==========================================================
        // Pricing, Financials & Discounts
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal CashDiscPercent { get; set; }
        [StringLength(FieldLengths.CashDisc)]
        public string CashDisc { get; set; } = string.Empty;
        public decimal DiscPercent { get; set; }
        public decimal DiscTotal { get; set; }
        public decimal Estimate { get; set; }
        public decimal FixedExchRate { get; set; }
        public decimal ReportingCurrencyFixedExchRate { get; set; }
        public long DefaultDimension { get; set; }
        [StringLength(FieldLengths.PriceGroupId)]
        public string PriceGroupId { get; set; } = string.Empty;
        public int GupDelayPricingCalculation { get; set; }
        public int GupSkipPricingCalculation { get; set; }

        // ==========================================================
        // Payment & Terms
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Payment)]
        public string Payment { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymTermId)]
        public string PaymentTerms { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymMode)]
        public string PaymMode { get; set; } = string.Empty;
        public DateTime FixedDueDate { get; set; }
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;
        public int SettleVoucher { get; set; }

        // Enum Properties
        public BankDocumentType BankDocumentType { get; set; }

        // ==========================================================
        // Tax
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxGroupId)]
        public string TaxGroupId { get; set; } = string.Empty;
        public long VatNumRecId { get; set; }

        // Enum Properties
        public NoYes OverrideSalesTax { get; set; }
        public int InclTax { get; set; } // Map to Enum if relevant (e.g. NoYes)
        public int VatNumTableType { get; set; }

        // ==========================================================
        // Inventory & Logistics
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        // Enum Properties
        public int CovStatus { get; set; }
        public int FreightSlipType { get; set; }

        // ==========================================================
        // Sales & Tracking
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.SalesPoolId)]
        public string SalesPoolId { get; set; } = string.Empty;
        [StringLength(FieldLengths.SalesOriginId)]
        public string SalesOriginId { get; set; } = string.Empty;
        [StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.SalesId)]
        public string SalesIdRef { get; set; } = string.Empty;
        public long WorkerSalesResponsible { get; set; }
        public long WorkerSalesTaker { get; set; }
        [StringLength(FieldLengths.ReasonCodeId)]
        public string ReasonId { get; set; } = string.Empty;
        public DateTime QuotationExpiryDate { get; set; }
        public DateTime QuotationFollowUpDate { get; set; }
        [StringLength(FieldLengths.Txt)]
        public string QuotationFollowUpActivity { get; set; } = string.Empty;

        // Enum Properties
        public ListCode ListCode { get; set; }

        // ==========================================================
        // Project Management (PSA)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.ProjId)]
        public string ProjIdRef { get; set; } = string.Empty;
        [StringLength(FieldLengths.ProjId)]
        public string ProjInvoiceProjId { get; set; } = string.Empty;
        public decimal PsaEstProjDuration { get; set; }
        public DateTime PsaEstProjStartDate { get; set; }
        public DateTime PsaEstProjEndDate { get; set; }
        [StringLength(FieldLengths.CalendarId)]
        public string PsaSchedCalendarId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Memo)]
        public string ScopeOfWork { get; set; } = string.Empty;
        public int PsaWizardNotOk { get; set; }
        public int TransferredToForecast { get; set; }
        public int TransferredToItemReq { get; set; }

        // Enum Properties
        public NoYes PsaSchedIgnoreCalendar { get; set; }

        // ==========================================================
        // Retail & Channels
        // ==========================================================
        // Basic Properties
        public long RetailChannelTable { get; set; }

        // ==========================================================
        // Miscellaneous & System Policy
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Code)]
        public string ModelId { get; set; } = string.Empty;
        public long ManualEntryChangePolicy { get; set; }
        public long SystemEntryChangePolicy { get; set; }
        public int TemplateActive { get; set; }
        public int Touched { get; set; }

        // Enum Properties
        public WHSCaseTaggingPolicy CaseTagging { get; set; }
        public WHSPalletTaggingPolicy PalletTagging { get; set; }
        public int ItemTagging { get; set; }
        public SalesSystemEntrySource SystemEntrySource { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(CustAccount))]
//         public virtual CustTable? CustAccount_CustTable { get; set; }

//         [ForeignKey(nameof(InvoiceAccount))]
//         public virtual CustTable? InvoiceAccount_CustTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DlvMode))]
//         public virtual DlvMode? DlvModeTable { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey(nameof(InventLocationId))]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey(nameof(InventSiteId))]
//         public virtual InventSite? InventSite { get; set; }

//         [ForeignKey(nameof(TaxGroupId))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(PaymMode))]
//         public virtual CustPaymModeTable? CustPaymModeTable { get; set; }

//         [ForeignKey(nameof(Payment))]
//         public virtual PaymTerm? PaymTermTable { get; set; }

//         [ForeignKey(nameof(PostingProfile))]
//         public virtual CustLedger? CustLedger { get; set; }

//         [ForeignKey(nameof(SalesPoolId))]
//         public virtual SalesPool? SalesPool { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(WorkerSalesResponsible))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesResponsibleEmployee { get; set; }

//         [ForeignKey(nameof(WorkerSalesTaker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesTakerEmployee { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List
//         public virtual ICollection<SalesQuotationLine> QuotationLines { get; set; } = new List<SalesQuotationLine>();

        #endregion
    }
}
