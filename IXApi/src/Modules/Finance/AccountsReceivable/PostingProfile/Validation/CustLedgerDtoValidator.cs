using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PostingProfile;

public class CustLedgerDtoValidator : BaseValidator<CustLedgerDto>
{
    public CustLedgerDtoValidator()
    {
        RuleFor(x => x.PostingProfile).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Settlement).IsInEnum();
        RuleFor(x => x.Interest).IsInEnum();
        RuleFor(x => x.CollectionLetter).IsInEnum();
    }
}
