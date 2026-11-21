using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class MainAccountDto : EntityDto<long>
    {
        [StringLength(FieldLengths.MainAccountValue)]
        public string MainAccountId { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public long LedgerChartOfAccounts { get; set; }
        public int AccountCategoryRef { get; set; }
        public MainAccountType Type { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public long OffsetLedgerDimension { get; set; }
        public LedgerPostingType PostingType { get; set; }
        public DebitCreditProposal DebitCreditProposal { get; set; }
        public DebitCreditProposal DebitCreditCheck { get; set; }
        public DebitCreditDemand DebitCreditBalanceDemand { get; set; }
        public NoYes MandatoryPaymentReference { get; set; }
        public FieldControl ValidateCurrency { get; set; }
        public FieldControl ValidatePosting { get; set; }
        public FieldControl ValidateUser { get; set; }

        [StringLength(FieldLengths.UserId)]
        public string UserId { get; set; } = string.Empty;

        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public NoYes Suspended { get; set; }

        // The chart of accounts form shows these; they already exist on MainAccount and were
        // simply not projected, so the page had nothing to bind to.
        public long OpeningAccount { get; set; }
        public MainAccountCloseType CloseType { get; set; }
        public NoYes Closing { get; set; }
        public NoYes Monetary { get; set; }
        public long ExchangeAdjustmentRateType { get; set; }
        public long ReportingExchangeAdjustmentRateType { get; set; }
        public int ReportingAccountType { get; set; }

        [StringLength(FieldLengths.MainAccountValue)]
        public string ConsolidationMainAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string GroupLevel01 { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string GroupLevel02 { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string GroupLevel03 { get; set; } = string.Empty;
    }
}
