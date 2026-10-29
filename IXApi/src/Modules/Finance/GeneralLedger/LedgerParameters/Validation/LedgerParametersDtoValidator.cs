using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters.Validation;

public sealed class LedgerParametersDtoValidator : BaseValidator<LedgerParametersDto>
{
    public LedgerParametersDtoValidator()
    {
        RuleFor(x => x.MaxRoundingDifferenceMst)
            .GreaterThanOrEqualTo(0m).WithMessage("The rounding tolerance cannot be negative.");
        RuleFor(x => x.AbortErrorAccount)
            .GreaterThanOrEqualTo(0).WithMessage("The error account reference is invalid.");
    }
}
