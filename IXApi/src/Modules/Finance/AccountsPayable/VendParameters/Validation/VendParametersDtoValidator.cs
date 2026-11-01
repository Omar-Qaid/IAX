using IAX.IXApi.Modules.Finance.Common;
using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendParametersDtoValidator : BaseValidator<VendParametersDto>
{
    public VendParametersDtoValidator()
    {
        RuleFor(x => x.VendPostingProfile).MaximumLength(FieldLengths.PostingProfile);
    }
}
