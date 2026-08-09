using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
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
}