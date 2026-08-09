using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("CustInvoiceTable")]
    public class CustInvoiceTable : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.InvoiceId)]
        public string InvoiceId { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;

        // ==========================================================
        // Customer & Accounts
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.OrderAccount)]
        public string OrderAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        public long OrderAccountRefRecId { get; set; }
        public string CustGroup { get; set; } = string.Empty;
        public string ProjIntercompany { get; set; } = string.Empty;

        // Enum Properties
        public NoYes OneTimeCustomer { get; set; }

        // ==========================================================
        // Addressing & Logistics
        // ==========================================================
        // Basic Properties
        public long PostalAddress { get; set; }
        public long DeliveryPostalAddress { get; set; }
        public long DeliveryLocation { get; set; }
        public string DlvTerm { get; set; } = string.Empty;
        public long TransportationDocument { get; set; }

        // ==========================================================
        // Payment Terms & Financials
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal ExchRate_W { get; set; }
        [StringLength(FieldLengths.Payment)]
        public string Payment { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymentSched)]
        public string PaymentSched { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymMode)]
        public string PaymMode { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime SalesDate_W { get; set; }
        public string CustBankAccountID { get; set; } = string.Empty;
        public long DirectDebitMandate { get; set; }
        public decimal CashDiscPercent { get; set; }
        [StringLength(FieldLengths.CashDiscCode)]
        public string CashDiscCode { get; set; } = string.Empty;
        public DateTime CashDiscDate { get; set; }
        public DateTime CashDiscBaseDate { get; set; }
        public int CashDiscBaseDays { get; set; }

        // ==========================================================
        // Posting & Ledgers
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;
        public long DefaultDimension { get; set; }
        public long CustBillingClassification { get; set; }
        public long AccountingDistributionTemplate { get; set; }

        // Enum Properties
        public NoYes Posted { get; set; }
        public int SubledgerJournalStatus { get; set; } // Map to SubledgerJournalTransferStatus enum if preferred
        public NoYes ExcludeFromDecoupledPostingProcess { get; set; }

        // ==========================================================
        // Tax & VAT Localization
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;
        public long TaxId { get; set; }
        [StringLength(FieldLengths.VatNum)]
        public string VatNum { get; set; } = string.Empty;
        public long VatNumRecId { get; set; }
        public DateTime VatDueDate_W { get; set; }

        // Enum Properties
        public NoYes OverrideSalesTax { get; set; }
        public NoYes TaxWithholdCalculate { get; set; }
        public NoYes PostponeVat { get; set; }
        public int VatNumTableType { get; set; }

        // ==========================================================
        // Electronic Invoicing & Localization (ZATCA / Global)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.ZatcaRetInvoiceRef)]
        public string ZatcaRetInvoiceRef { get; set; } = string.Empty;
        [StringLength(FieldLengths.ZatcaRetReason)]
        public string ZatcaRetReason { get; set; } = string.Empty;

        // Enum Properties
        public NoYes EInvoiceLineSpec { get; set; }
        public int InvoiceType_W { get; set; }
        public int InvoiceComplementaryType { get; set; }

        // ==========================================================
        // Workflow & System Audit Statuses
        // ==========================================================
        // Basic Properties
        public long WorkerSalesTaker { get; set; }
        public long CorrectionReasonCode { get; set; }
        public long SourceDocumentHeader { get; set; }
        public long SourceDocumentLine { get; set; }
        public long ServiceCodeRefRecId { get; set; }
        public long FinTag { get; set; }
        public DateTime AdjustingInvoiceDate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ReleaseDateTzId { get; set; }
        public int Touched { get; set; }

        // Enum Properties
        public int WorkflowApprovalState { get; set; }
        public int WorkflowApprovalStatus { get; set; }
        public int CovStatus { get; set; }
        public NoYes ForInterestAdjustment { get; set; }
        public int GiroType { get; set; }
        public NoYes UseDefaultFromCustomer { get; set; }
        public NoYes IntercompanyPosted { get; set; }
        public ListCode ListCode { get; set; }
        public NoYes ManualNumbering_W { get; set; }
        public NoYes McrGiftCard { get; set; }
        public SysDataStateCode SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(OrderAccount))]
//         public virtual CustTable? OrderAccount_CustTable { get; set; }

//         [ForeignKey(nameof(InvoiceAccount))]
//         public virtual CustTable? InvoiceAccount_CustTable { get; set; }

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DimensionAttributeValueSet { get; set; }

//         [ForeignKey(nameof(DlvTerm))]
//         public virtual DlvTerm? DlvTermTable { get; set; }

//         [ForeignKey(nameof(CustGroup))]
//         public virtual CustGroup? CustGroupTable { get; set; }

//         [ForeignKey(nameof(PostingProfile))]
//         public virtual CustLedger? CustLedger { get; set; }

//         [ForeignKey(nameof(Payment))]
//         public virtual PaymTerm? PaymTerm { get; set; }

//         [ForeignKey(nameof(PaymMode))]
//         public virtual CustPaymModeTable? CustPaymModeTable { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(TaxItemGroup))]
//         public virtual TaxItemGroupHeading? TaxItemGroupHeading { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(PostalAddress))]
//         public virtual LogisticsPostalAddress? MainPostalAddress { get; set; }

//         [ForeignKey(nameof(DeliveryLocation))]
//         public virtual LogisticsLocation? DeliveryLocationMap { get; set; }

//         [ForeignKey(nameof(PaymentSched))]
//         public virtual PaymSched? PaymentSchedule { get; set; }

//         [ForeignKey(nameof(WorkerSalesTaker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesTakerEmployee { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

//         public virtual ICollection<CustInvoiceLine> InvoiceLines { get; set; } = new List<CustInvoiceLine>();

        #endregion
    }
}

