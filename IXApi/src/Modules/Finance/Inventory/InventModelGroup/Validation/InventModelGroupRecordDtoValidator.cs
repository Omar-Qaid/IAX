using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventModelGroupRecordDtoValidator : BaseValidator<InventModelGroupRecordDto>
{
    public InventModelGroupRecordDtoValidator()
    {
        RuleFor(x => x.ModelGroupId)
            .NotEmpty().WithMessage("Item model group ID is required.")
            .MaximumLength(FieldLengths.ModelGroupId).WithMessage($"Item model group ID cannot exceed {FieldLengths.ModelGroupId} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Name cannot exceed {FieldLengths.Name} characters.");
    }
}
