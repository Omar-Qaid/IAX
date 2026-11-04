using IAX.IXApi.Modules.Finance.Common;
using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendParametersDtoValidator : BaseValidator<VendParametersDto>
{
    public VendParametersDtoValidator()
    {
        RuleFor(x => x.VendPostingProfile).MaximumLength(FieldLengths.PostingProfile);
        RuleFor(x => x.PriceTotalMatchToleranceInPercent).InclusiveBetween(0m, 100m)
            .WithMessage("The invoice price match tolerance must be a percentage between 0 and 100.");
    }
}
