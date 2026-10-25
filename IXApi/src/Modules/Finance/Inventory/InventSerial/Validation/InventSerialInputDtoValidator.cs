using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventSerialInputDtoValidator : BaseValidator<InventSerialInputDto>
{
    public InventSerialInputDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item is required.")
            .MaximumLength(FieldLengths.ItemId).WithMessage($"Item ID cannot exceed {FieldLengths.ItemId} characters.");

        RuleFor(x => x.InventSerialId)
            .NotEmpty().WithMessage("Serial ID is required.")
            .MaximumLength(FieldLengths.InventSerialId).WithMessage($"Serial ID cannot exceed {FieldLengths.InventSerialId} characters.");
    }
}
