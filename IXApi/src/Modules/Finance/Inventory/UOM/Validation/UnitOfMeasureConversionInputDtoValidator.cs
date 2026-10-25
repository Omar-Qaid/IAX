using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class UnitOfMeasureConversionInputDtoValidator : BaseValidator<UnitOfMeasureConversionInputDto>
{
    public UnitOfMeasureConversionInputDtoValidator()
    {
        RuleFor(x => x.FromUnitOfMeasure)
            .GreaterThan(0).WithMessage("From unit is required.");

        RuleFor(x => x.ToUnitOfMeasure)
            .GreaterThan(0).WithMessage("To unit is required.");
    }
}
