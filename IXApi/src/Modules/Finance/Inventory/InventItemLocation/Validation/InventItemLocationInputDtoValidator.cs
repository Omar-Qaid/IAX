using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemLocationInputDtoValidator : BaseValidator<InventItemLocationInputDto>
{
    public InventItemLocationInputDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.")
            .MaximumLength(FieldLengths.ItemId);

        RuleFor(x => x.InventDimId)
            .NotEmpty().WithMessage("Inventory dimension ID is required.")
            .MaximumLength(FieldLengths.InventDimId);
    }
}
