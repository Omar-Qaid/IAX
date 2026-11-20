using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PostingProfile;

public class CustLedgerAccountsDtoValidator : BaseValidator<CustLedgerAccountsDto>
{
    public CustLedgerAccountsDtoValidator()
    {
        RuleFor(x => x.PostingProfile).NotEmpty().MaximumLength(10);
        RuleFor(x => x.AccountCode).IsInEnum();
        RuleFor(x => x.Num).NotEmpty().When(x => x.AccountCode != AccountCode.All);
        RuleFor(x => x.Num).MaximumLength(20);
        RuleFor(x => x.CollectionLetterCourse).MaximumLength(10);
        RuleFor(x => x.SummaryLedgerDimension).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ClearingLedgerDimension).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VatPrepaymentsLedgerDimension).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LiabilitiesForDiscountLedgerDimension).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CustInterest).GreaterThanOrEqualTo(0);
    }
}
