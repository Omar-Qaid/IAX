using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class TaxTableDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";

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

    public class TaxGroupDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        public string TaxGroup { get; set; } = string.Empty;
        public string TaxGroupName { get; set; } = string.Empty;
        public TaxGroupSetup TaxGroupSetup { get; set; }
        public TaxGroupSource Source { get; set; }
        public TaxGroupRounding TaxGroupRounding { get; set; }
        public NoYes TaxReverseOnCashDisc { get; set; }
        public NoYes EuTrade_W { get; set; }
        public NoYes MandatorySalesDate_W { get; set; }
        public NoYes FillSalesDate_W { get; set; }
        public int FillVatDueDatePeriodNumber { get; set; }
        public NoYes FillVatDueDate_W { get; set; }
        public TaxPointBase FillVatDueDateBasedOn { get; set; }
        public TaxPeriodUnit FillVatDueDatePeriod { get; set; }
        public TaxPrintDetail TaxPrintDetail { get; set; }

        public List<TaxGroupDataDto> Lines { get; set; } = new List<TaxGroupDataDto>();
    }

    public class TaxGroupDataDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        public string? TaxGroup { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxExemptCode { get; set; }
        public NoYes ExemptTax { get; set; }
        public NoYes UseTax { get; set; }
        public NoYes IntracomVat { get; set; }
        public NoYes ReverseCharge_W { get; set; }

        // Navigation / Joined Display Attributes
        public string? TaxCodeName { get; set; }
        public decimal? TaxValue { get; set; }
    }

    public class TaxItemGroupDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        public string TaxItemGroup { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TaxGroupSource Source { get; set; }
        public EuSalesListType EuSalesListType { get; set; }

        public List<TaxOnItemDto> Lines { get; set; } = new List<TaxOnItemDto>();
    }

    public class TaxOnItemDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        public string TaxItemGroup { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxExemptCode { get; set; } = string.Empty;

        // Navigation / Joined Display Attributes
        public string? TaxCodeName { get; set; }
        public decimal? TaxValue { get; set; }
    }

    public class TaxDataDto : EntityDto<long>
    {
    }

    public class TaxJournalTransDto : EntityDto<long>
    {
    }

    public class TaxTransDto : EntityDto<long>
    {
    }

    public class TaxAuthorityAddressDto : EntityDto<long>
    {
        public string TaxAuthority { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxAuthorityId { get; set; } = string.Empty;
        public string AccountNum { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Sms { get; set; } = string.Empty;
        public string Telex { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Pager { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal RoundOff { get; set; }
        public TaxRoundOffType RoundOffType { get; set; }
        public TaxReportLayout TaxReportLayout { get; set; }
        public NoYes UseDefaultLayout { get; set; }
        public NoYes SeparateTaxSummary { get; set; }
        public NoYes PrintBlankPage { get; set; }
    }

    public class TaxPeriodHeadDto : EntityDto<long>
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxAuthority { get; set; } = string.Empty;
        public string PaymentCode { get; set; } = string.Empty;
        public int QtyUnit { get; set; } = 1;
        public TaxPeriodUnit PeriodUnit { get; set; } = TaxPeriodUnit.Day;
        public NoYes NotGenerateOffsetTaxTrans { get; set; } = NoYes.No;
        public NoYes ReportAdjustment { get; set; } = NoYes.No;
        public NoYes UseBatch { get; set; } = NoYes.No;
        public string ActivePeriodForBatchJobs { get; set; } = string.Empty;
        public List<TaxReportPeriodDto> Intervals { get; set; } = new();
    }

    public class TaxReportPeriodDto : EntityDto<long>
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public NoYes Closed { get; set; } = NoYes.No;
    }

    public class TaxLedgerAccountGroupDto : EntityDto<long>
    {
        public string TaxAccountGroup { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public long? TaxOutgoingLedgerDimension { get; set; }
        public long? TaxIncomingLedgerDimension { get; set; }
        public long? TaxReportLedgerDimension { get; set; }
        public long? TaxUseTaxLedgerDimension { get; set; }
        public long? TaxOffsetUseTaxLedgerDimension { get; set; }
        public long? TaxReverseOffsetIncLedgerDimension_W { get; set; }
        public long? TaxReverseOffsetOutLedgerDimension_W { get; set; }
        public long? TaxNonDeductibleTaxLedgerDimension { get; set; }
        public long? TaxFreePercentLedgerDimension { get; set; }
        public long? TaxInterimTransitLedgerDimension { get; set; }
        public long? TaxUnrealizedPayablesLedgerDimension { get; set; }
        public long? TaxUnrealizedReceivablesLedgerDimension { get; set; }
        public long? CashDiscountIncomingLedgerDimension { get; set; }
        public long? CashDiscountOutgoingLedgerDimension { get; set; }
        public long? TaxIncomingDifferenceLedgerDimension { get; set; }
        public long? TaxIncomingDiffOffsetLedgerDimension { get; set; }
        public long? TaxOutgoingDifferenceLedgerDimension { get; set; }
        public long? TaxOutgoingDiffOffsetLedgerDimension { get; set; }
        public long? PennyDifferenceCustomerLedgerDimension { get; set; }
        public long? PennyDifferenceVendorLedgerDimension { get; set; }
    }
    public class TaxExemptCodeDto : EntityDto<long>
    {
        public string ExemptCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
