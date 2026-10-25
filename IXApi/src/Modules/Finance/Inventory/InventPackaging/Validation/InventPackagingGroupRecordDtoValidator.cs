using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventPackagingGroupRecordDtoValidator : BaseValidator<InventPackagingGroupRecordDto>
{
    public InventPackagingGroupRecordDtoValidator()
    {
        RuleFor(x => x.PackagingGroupId)
            .NotEmpty().WithMessage("Packaging group ID is required.")
            .MaximumLength(FieldLengths.PackagingGroupId).WithMessage($"Packaging group ID cannot exceed {FieldLengths.PackagingGroupId} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Packaging group name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Packaging group name cannot exceed {FieldLengths.Name} characters.");
    }
}
