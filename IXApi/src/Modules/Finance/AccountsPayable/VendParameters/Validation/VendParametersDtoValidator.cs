using IAX.IXApi.Modules.Finance.Common;
using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendParametersDtoValidator : BaseValidator<VendParametersDto>
{
    public VendParametersDtoValidator()
    {
        RuleFor(x => x.VendPostingProfile).MaximumLength(FieldLengths.PostingProfile);
        RuleFor(x => x.TotalPriceTolerancePercentage).InclusiveBetween(0m, 100m)
            .WithMessage("The invoice price match tolerance must be a percentage between 0 and 100.");
        RuleFor(x => x.MatchingPolicy).IsInEnum()
            .WithMessage("The matching policy must be None, TwoWay or ThreeWay.");
        RuleFor(x => x.UseInvoiceMatching).IsInEnum();
        RuleFor(x => x.UseTotalPriceMatching).IsInEnum();
        RuleFor(x => x.MatchingPolicyAllowOverride).IsInEnum();
    }
}
