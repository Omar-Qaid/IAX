using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustomerDto : EntityDto<long>
    {
        [StringLength(FieldLengths.AccountNum)]
        public string AccountNum { get; set; } = null!;
        public string? CustGroup { get; set; }
        [StringLength(FieldLengths.Phone)]
        public string? Phone { get; set; }
        public string DataAreaId { get; set; } = "dat";
        public string? Currency { get; set; }
        [StringLength(FieldLengths.PaymTermId)]
        public string? PaymTermId { get; set; }
        [StringLength(FieldLengths.TaxGroup)]
        public string? TaxGroup { get; set; }
        public decimal CreditMax { get; set; }
        public int MandatoryCreditLimit { get; set; }
        public CustVendorBlocked Blocked { get; set; }
        public string? Status { get; set; }
        public int AccountStatement { get; set; }
        [StringLength(FieldLengths.LineDisc)]
        public string? LineDisc { get; set; }
        public string? BankAccount { get; set; }
        public string? BankCentralBankPurposeCode { get; set; }
        [StringLength(FieldLengths.BankCentralBankPurposeText)]
        public string? BankCentralBankPurposeText { get; set; }
        public long BankCustPaymIdTable { get; set; }
        [StringLength(FieldLengths.CashDisc)]
        public string? CashDisc { get; set; }
        public int CashDiscBaseDays { get; set; }
        public string? CommissionGroup { get; set; }
        public int CompanyType { get; set; }
        [StringLength(FieldLengths.ContactPersonId)]
        public string? ContactPersonId { get; set; }
        public string? CustItemGroupId { get; set; }
        public long DefaultDimension { get; set; }
        public string? DlvMode { get; set; }
        public string? DlvReason { get; set; }
        public string? DlvTerm { get; set; }
        public string? InventLocation { get; set; }
        public string? InventProfileId { get; set; }
        public InventProfileType InventProfileType { get; set; }
        [StringLength(FieldLengths.InventSiteId)]
        public string? InventSiteId { get; set; }
        [StringLength(FieldLengths.InvoiceAccount)]
        public string? InvoiceAccount { get; set; }
        public int InvoiceAddress { get; set; }
        public int InvoicePostingType { get; set; }
        [StringLength(FieldLengths.MarkupGroup)]
        public string? MarkupGroup { get; set; }
        public string? MultiLineDisc { get; set; }
        public string? PaymIdType { get; set; }
        [StringLength(FieldLengths.PaymMode)]
        public string? PaymMode { get; set; }
        [StringLength(FieldLengths.PaymDayId)]
        public string? PaymDayId { get; set; }
        [StringLength(FieldLengths.PaymentReference)]
        public string? PaymentReference { get; set; }
        [StringLength(FieldLengths.VendAccount)]
        public string? VendAccount { get; set; }
        [StringLength(FieldLengths.PaymSched)]
        public string? PaymSched { get; set; }
        [StringLength(FieldLengths.PaymSpec)]
        public string? PaymSpec { get; set; }
        public string? PriceGroup { get; set; }
        public string? SalesGroup { get; set; }
        [StringLength(FieldLengths.SalesPoolId)]
        public string? SalesPoolId { get; set; }
        [StringLength(FieldLengths.VatNum)]
        public string? VatNum { get; set; }
        public int InvoiceType { get; set; }
        [StringLength(FieldLengths.CustCategory)]
        public string? CustCategory { get; set; }
        public int VatFileAttachment { get; set; }
        public string? VatRegNum { get; set; }
    }

    public class CustGroupDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        [StringLength(FieldLengths.PaymTermId)]
        public string? PaymTermId { get; set; }
        [StringLength(FieldLengths.CustGroupId)]
        public string? CustGroupId { get; set; }
        [StringLength(FieldLengths.Name)]
        public string? Name { get; set; }
        public string? ClearingPeriod { get; set; }
        public int PriceIncludeSalesTax { get; set; }
        [StringLength(FieldLengths.TaxGroupId)]
        public string? TaxGroupId { get; set; }
        public string? TaxPeriodPaymentCode { get; set; }
        public long CustWriteOffRefRecId { get; set; }
        public long CustAccountNumSeq { get; set; }
        public int IsPublicSector { get; set; }
        public long AccountingCurrencyExchangeRateType { get; set; }
        public long ReportingCurrencyExchangeRateType { get; set; }
        public long BankCustPaymIdTable { get; set; }
        public long DefaultDimension { get; set; }
    }
}
