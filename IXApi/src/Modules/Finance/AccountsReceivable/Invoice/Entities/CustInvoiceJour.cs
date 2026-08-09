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
    [Table("CustInvoiceJour")]
    public class CustInvoiceJour : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.InvoiceId)]
        public string InvoiceId { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        [StringLength(FieldLengths.SalesId)]
        public string SalesId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Voucher)]
        public string LedgerVoucher { get; set; } = string.Empty;
        [StringLength(FieldLengths.ParmId)]
        public string ParmId { get; set; } = string.Empty;
        [StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;

        // Enum Properties
        public SalesType SalesType { get; set; }
        public DocumentStatus DocumentStatus { get; set; } // Mapping for proforma / updated tracking

        // ==========================================================
        // Customer & Accounts
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.OrderAccount)]
        public string OrderAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.CustGroupId)]
        public string CustGroup { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.Name)]
        public string InvoicingName { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.CompanyId)]
        public string IntercompanyCompanyId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.Email)]
        public string McrEmail { get; set; } = string.Empty;

        // Enum Properties
        public NoYes OneTimeCustomer { get; set; }

        // ==========================================================
        // Delivery & Addressing
        // ==========================================================
        // Basic Properties
        public long DeliveryPostalAddress { get; set; }
        [StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public long InvoicePostalAddress { get; set; }
        
        [StringLength(FieldLengths.DlvModeId)]
        public string DlvMode { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.DlvTermId)]
        public string DlvTerm { get; set; } = string.Empty;

        // ==========================================================
        // Financials, Totals & Exchanges (Transaction Currency)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal InvoiceAmount { get; set; }
        public decimal SalesBalance { get; set; }
        public decimal SumLineDisc { get; set; }
        public decimal EndDisc { get; set; }
        public decimal SumMarkup { get; set; }
        public decimal SumTax { get; set; }
        public decimal InvoiceRoundOff { get; set; }
        public decimal HeaderTax { get; set; }
        public decimal CashDisc { get; set; }
        public decimal CashDiscPercent { get; set; }
        [StringLength(FieldLengths.CashDiscCode)]
        public string CashDiscCode { get; set; } = string.Empty;
        public decimal ExchRate { get; set; }
        public decimal ExchRateSecondary { get; set; }
        public long DefaultDimension { get; set; }
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;

        // Enum Properties
        public int InclTax { get; set; } // Map to NoYes enum if preferred

        // ==========================================================
        // Financials & Totals (Accounting Currency - MST)
        // ==========================================================
        // Basic Properties
        public decimal InvoiceAmountMst { get; set; }
        public decimal SalesBalanceMst { get; set; }
        public decimal SumLineDiscMst { get; set; }
        public decimal EndDiscMst { get; set; }
        public decimal SumMarkupMst { get; set; }
        public decimal SumTaxMst { get; set; }
        public decimal InvoiceRoundOffMst { get; set; }

        // ==========================================================
        // Financials & Totals (Reporting Currency)
        // ==========================================================
        // Basic Properties
        public decimal ReportingCurrencyExchangeRate { get; set; }
        public decimal ReportingCurrencyExchangeRateSecondary { get; set; }

        // ==========================================================
        // Payment Terms & Logistics
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.Payment)]
        public string Payment { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymentSched)]
        public string PaymentSched { get; set; } = string.Empty;
        [StringLength(FieldLengths.PaymDayId)]
        public string PaymDayId { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime FixedDueDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime CashDiscDate { get; set; }
        public DateTime CashDiscBaseDate { get; set; }
        public long DirectDebitMandate { get; set; }

        // ==========================================================
        // Tax & Localization
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;
        public long TaxId { get; set; }
        [StringLength(FieldLengths.VatNum)]
        public string VatNum { get; set; } = string.Empty;
        public long PartyTaxId { get; set; }
        public decimal ReverseChargeAmount { get; set; }

        // Enum Properties
        public int GiroType { get; set; }
        public NoYes TaxPrintOnInvoice { get; set; }
        public NoYes TaxSpecifyByLine { get; set; }

        // ==========================================================
        // Physical Quantities & Logistics
        // ==========================================================
        // Basic Properties
        public decimal Qty { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
        [StringLength(FieldLengths.InventSiteId)]
        public string PrintMgmtSiteId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;
        
        [StringLength(FieldLengths.DocumentNum)]
        public string BillOfLadingId { get; set; } = string.Empty;
        public long TransportationDocument { get; set; }
        public long BankLcExportLine { get; set; }

        // Enum Properties
        public NoYes Backorder { get; set; }
        public int CovStatus { get; set; }
        public NoYes ShipCarrierBlindShipment { get; set; }

        // ==========================================================
        // Sales Management & Returns
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.SalesOriginId)]
        public string SalesOriginId { get; set; } = string.Empty;
        public long RetailStoreIdTable { get; set; }
        [StringLength(FieldLengths.Num)]
        public string OfferId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Num)]
        public string ReturnItemNum { get; set; } = string.Empty;

        // Enum Properties
        public int ReturnStatus { get; set; }

        // ==========================================================
        // Call Center / Retail (MCR)
        // ==========================================================
        // Basic Properties
        public decimal McrDueAmount { get; set; }
        public decimal McrPaymAmount { get; set; }
        public decimal OnAccountAmount { get; set; }

        // ==========================================================
        // Electronic Invoicing (ZATCA / Global)
        // ==========================================================
        // Basic Properties
        [StringLength(FieldLengths.ZatcaRetInvoiceRef)]
        public string ZatcaRetInvoiceRef { get; set; } = string.Empty;
        [StringLength(FieldLengths.ZatcaRetReason)]
        public string ZatcaRetReason { get; set; } = string.Empty;

        // Enum Properties
        public NoYes EInvoiceLineSpecific { get; set; }
        public NoYes SentElectronically { get; set; }

        // ==========================================================
        // System Flags & Audit Trailing
        // ==========================================================
        // Basic Properties
        public long WorkerSalesTaker { get; set; }
        public long ReasonTableRef { get; set; }
        public long ReversedRecId { get; set; }
        public long SourceDocumentHeader { get; set; }
        public long SourceDocumentLine { get; set; }
        public long ServiceCodeRefRecId { get; set; }
        public long FinTag { get; set; }
        public int RefNum { get; set; }
        public int PrintedOriginals { get; set; }

        // Enum Properties
        public int InvoiceType_W { get; set; }
        public NoYes Proforma { get; set; }
        public NoYes IsCorrection { get; set; }
        public NoYes Updated { get; set; }
        public NoYes IntercompanyPosted { get; set; }
        public NoYes Triangulation { get; set; }
        public NoYes PostedState { get; set; }
        public NoYes Prepayment { get; set; }
        public NoYes SubBillSuppressChildItems { get; set; }
        public SysDataStateCode SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(SalesId))]
//         public virtual SalesTable? SalesTable { get; set; }

//         [ForeignKey(nameof(OrderAccount))]
//         public virtual CustTable? OrderAccount_CustTable { get; set; }

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

//         [ForeignKey(nameof(CustGroup))]
//         public virtual CustGroup? CustGroupTable { get; set; }

//         [ForeignKey(nameof(PostingProfile))]
//         public virtual CustLedger? CustLedger { get; set; }

//         [ForeignKey(nameof(Payment))]
//         public virtual PaymTerm? PaymTerm { get; set; }

//         [ForeignKey(nameof(TaxGroup))]
//         public virtual TaxGroupHeading? TaxGroupHeading { get; set; }

//         [ForeignKey(nameof(InventLocationId))]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey(nameof(WorkerSalesTaker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? SalesTakerEmployee { get; set; }

//         [ForeignKey(nameof(DeliveryPostalAddress))]
//         public virtual LogisticsPostalAddress? DeliveryAddress { get; set; }

//         [ForeignKey(nameof(InvoicePostalAddress))]
//         public virtual LogisticsPostalAddress? InvoiceAddressMap { get; set; }

//         [ForeignKey(nameof(PaymentSched))]
//         public virtual PaymSched? PaymentSchedule { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

//         public virtual ICollection<CustInvoiceTrans> InvoiceLines { get; set; } = new List<CustInvoiceTrans>();

        #endregion
    }
}

