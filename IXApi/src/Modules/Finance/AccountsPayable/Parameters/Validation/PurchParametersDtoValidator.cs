using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class PurchParametersDtoValidator : BaseValidator<PurchParametersDto>
{
    public PurchParametersDtoValidator()
    {
        RuleFor(x => x.PurchaseType).IsInEnum();
        RuleFor(x => x.PriceDateType).IsInEnum();
    }
}
