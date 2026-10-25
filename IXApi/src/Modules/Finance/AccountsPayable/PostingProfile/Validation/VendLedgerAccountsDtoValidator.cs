using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendLedgerAccountsDtoValidator : BaseValidator<VendLedgerAccountsDto>
{
    public VendLedgerAccountsDtoValidator()
    {
        RuleFor(x => x.PostingProfile).NotEmpty().MaximumLength(FieldLengths.PostingProfile);
        RuleFor(x => x.Num).MaximumLength(FieldLengths.Num);
    }
}
