using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public sealed class ReleasedProductStatusInputDtoValidator : BaseValidator<ReleasedProductStatusInputDto>
{
    public ReleasedProductStatusInputDtoValidator()
    {
        RuleFor(x => x.StateId)
            .MaximumLength(FieldLengths.Name).WithMessage($"State ID cannot exceed {FieldLengths.Name} characters.");
    }
}
