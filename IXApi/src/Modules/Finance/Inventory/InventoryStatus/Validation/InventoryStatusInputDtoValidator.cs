using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

public sealed class InventoryStatusInputDtoValidator : BaseValidator<InventoryStatusInputDto>
{
    public InventoryStatusInputDtoValidator()
    {
        RuleFor(x => x.InventStatusId)
            .NotEmpty().WithMessage("Inventory status ID is required.")
            .MaximumLength(FieldLengths.InventStatusId).WithMessage($"Inventory status ID cannot exceed {FieldLengths.InventStatusId} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Name cannot exceed {FieldLengths.Name} characters.");
    }
}
