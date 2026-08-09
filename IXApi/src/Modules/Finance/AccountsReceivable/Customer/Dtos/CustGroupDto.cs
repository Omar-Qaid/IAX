using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustGroupDto : EntityDto<long>
    {
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