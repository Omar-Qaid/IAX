using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemGroupRecordDtoValidator : BaseValidator<InventItemGroupRecordDto>
{
    public InventItemGroupRecordDtoValidator()
    {
        RuleFor(x => x.ItemGroupId)
            .NotEmpty().WithMessage("Item group ID is required.")
            .MaximumLength(FieldLengths.ItemGroupId).WithMessage($"Item group ID cannot exceed {FieldLengths.ItemGroupId} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item group name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Item group name cannot exceed {FieldLengths.Name} characters.");
    }
}
