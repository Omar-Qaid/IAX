using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendLedgerDtoValidator : BaseValidator<VendLedgerDto>
{
    public VendLedgerDtoValidator()
    {
        RuleFor(x => x.PostingProfile).NotEmpty().MaximumLength(FieldLengths.PostingProfile);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
        RuleFor(x => x.PostingProfileClose).MaximumLength(FieldLengths.PostingProfile);
    }
}
