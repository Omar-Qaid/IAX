using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class MarkupTableDto : EntityDto<long>
    {

        [Required]
        [StringLength(FieldLengths.MarkupCode)]
        public string MarkupCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Txt)]
        public string Txt { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = string.Empty;

        public ModuleInventPurchSales ModuleType { get; set; }

        public string TaxItemGroup { get; set; } = string.Empty;

        public string? ZatcaNominalCode { get; set; }

        public long TaxRateType { get; set; }

        public long TaxWithholdItemGroup { get; set; }

        // Debit Posting
        public int CustType { get; set; }
        public int CustPosting { get; set; }
        public long? CustomerLedgerDimension { get; set; }

        // Bank Document Charge
        public NoYes IsBankDocumentCharge { get; set; }

        // Credit Posting
        public int VendType { get; set; }
        public int VendPosting { get; set; }
        public long? VendorLedgerDimension { get; set; }


        // Foreign Trade
        public NoYes IncludeIntoIntrastatInvoiceValue { get; set; }
        public NoYes IncludeIntoIntrastatStatisticalValue { get; set; }

        // Logistics & MCR
        public NoYes IsShipping { get; set; }
        public NoYes Refundable { get; set; }
        public NoYes McrProrate { get; set; }
        public NoYes McrBrokerContractFee { get; set; }
    }
}