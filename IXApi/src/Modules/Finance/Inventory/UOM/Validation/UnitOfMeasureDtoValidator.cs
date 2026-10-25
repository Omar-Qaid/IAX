using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class UnitOfMeasureDtoValidator : BaseValidator<UnitOfMeasureDto>
{
    public UnitOfMeasureDtoValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required.")
            .MaximumLength(10).WithMessage("Symbol cannot exceed 10 characters.");

        RuleFor(x => x.DecimalPrecision)
            .InclusiveBetween(0, 6).WithMessage("Decimal precision must be between 0 and 6.");
    }
}
