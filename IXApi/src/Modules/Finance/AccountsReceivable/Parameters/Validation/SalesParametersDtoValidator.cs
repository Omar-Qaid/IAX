using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public class SalesParametersDtoValidator : BaseValidator<SalesParametersDto>
{
    public SalesParametersDtoValidator()
    {
        RuleFor(x => x.SalesType).IsInEnum();
        RuleFor(x => x.PriceDateType).IsInEnum();
    }
}
