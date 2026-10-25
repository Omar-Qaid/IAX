using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ItemGroupAssignmentInputDtoValidator : BaseValidator<ItemGroupAssignmentInputDto>
{
    public ItemGroupAssignmentInputDtoValidator()
    {
        RuleFor(x => x.ItemGroupId)
            .NotEmpty().WithMessage("Item group ID is required.")
            .MaximumLength(FieldLengths.ItemGroupId);
    }
}
