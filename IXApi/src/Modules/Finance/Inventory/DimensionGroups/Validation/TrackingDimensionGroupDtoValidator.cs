using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class TrackingDimensionGroupDtoValidator : BaseValidator<TrackingDimensionGroupDto>
{
    public TrackingDimensionGroupDtoValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("Group ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}
