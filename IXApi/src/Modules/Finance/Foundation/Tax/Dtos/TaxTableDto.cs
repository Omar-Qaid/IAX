using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxTableDto : EntityDto<long>
    {

        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxName)]
        public string TaxName { get; set; } = string.Empty;

        [StringLength(FieldLengths.TaxPeriod)]
        public string TaxPeriod { get; set; } = "Monthly";

        [StringLength(FieldLengths.TaxAccountGroup)]
        public string TaxAccountGroup { get; set; } = "STANDARD";

        [StringLength(FieldLengths.TaxCurrencyCode)]
        public string TaxCurrencyCode { get; set; } = "SAR";

        public TaxGroupSource Source { get; set; }

        public string TaxOnTax { get; set; } = string.Empty;

        public string TaxUnit { get; set; } = string.Empty;

        public TaxBase TaxBase { get; set; }
        public TaxCalcMethod TaxCalcMethod { get; set; }
        public TaxLimitBase TaxLimitBase { get; set; }
        public NoYes TaxIncludeInTax { get; set; }
        public NoYes NegativeTax { get; set; }
        public NoYes UnrealizedTax { get; set; }
        public NoYes TaxAllowLineDiscountOnTaxPerUnit { get; set; }

        public decimal TaxRoundOff { get; set; }
        public RoundingType TaxRoundOffType { get; set; }
        public NoYes RoundDeductibleFirst { get; set; }

        public string PrintCode { get; set; } = string.Empty;
        public string PaymentTaxCode { get; set; } = string.Empty;
        public string TaxJurisdictionCode { get; set; } = string.Empty;

        public TaxType_W TaxType_W { get; set; }
        public TaxCountryRegionType TaxCountryRegionType { get; set; }
        public NoYes NotEuSalesList { get; set; }
        public NoYes ExcludeFromInvoice { get; set; }
        public NoYes TaxPurchaseTax { get; set; }
        public NoYes TaxPackagingTax { get; set; }
        public TaxWriteSelection TaxWriteSelection { get; set; }
        public TaxReconcileAmountOrigin ReconcileAmountOrigin { get; set; }

        public int RepFieldBaseOutgoing { get; set; }
        public int RepFieldBaseOutgoingCreditNote { get; set; }
        public int RepFieldTaxOutgoing { get; set; }
        public int RepFieldTaxOutgoingCreditNote { get; set; }

        public int RepFieldBaseIncoming { get; set; }
        public int RepFieldBaseIncomingCreditNote { get; set; }
        public int RepFieldTaxIncoming { get; set; }
        public int RepFieldTaxIncomingCreditNote { get; set; }

        public int RepFieldBaseUseTax { get; set; }
        public int RepFieldBaseUseTaxCreditNote { get; set; }
        public int RepFieldUseTax { get; set; }
        public int RepFieldUseTaxCreditNote { get; set; }

        public int RepFieldBaseUseTaxOffset { get; set; }
        public int RepFieldBaseUseTaxOffsetCreditNote { get; set; }
        public int RepFieldUseTaxOffset { get; set; }
        public int RepFieldUseTaxOffsetCreditNote { get; set; }

        public int RepFieldTaxFreeSales { get; set; }
        public int RepFieldTaxFreeSalesCreditNote { get; set; }
        public int RepFieldTaxFreeBuy { get; set; }
        public int RepFieldTaxFreeBuyCreditNote { get; set; }

        // Joined / Computed property for active rate
        public decimal TaxValue { get; set; }
    }
}