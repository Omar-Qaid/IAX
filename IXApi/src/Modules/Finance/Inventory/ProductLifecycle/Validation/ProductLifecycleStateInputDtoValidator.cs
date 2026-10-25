using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public sealed class ProductLifecycleStateInputDtoValidator : BaseValidator<ProductLifecycleStateInputDto>
{
    public ProductLifecycleStateInputDtoValidator()
    {
        RuleFor(x => x.StateId)
            .NotEmpty().WithMessage("State ID is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"State ID cannot exceed {FieldLengths.Name} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(FieldLengths.Description).WithMessage($"Description cannot exceed {FieldLengths.Description} characters.");
    }
}
