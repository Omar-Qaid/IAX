using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount.Validation;

public sealed class MainAccountDtoValidator : BaseValidator<MainAccountDto>
{
    public MainAccountDtoValidator()
    {
        RuleFor(x => x.MainAccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.LedgerChartOfAccounts).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.PostingType).IsInEnum();
        RuleFor(x => x.DebitCreditProposal).IsInEnum();
        RuleFor(x => x.DebitCreditCheck).IsInEnum();
        RuleFor(x => x.DebitCreditBalanceDemand).IsInEnum();
        RuleFor(x => x.MandatoryPaymentReference).IsInEnum();
        RuleFor(x => x.ValidateCurrency).IsInEnum();
        RuleFor(x => x.ValidatePosting).IsInEnum();
        RuleFor(x => x.ValidateUser).IsInEnum();
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .When(x => x.ValidateCurrency == FieldControl.Table);
    }
}
