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
    }
}
