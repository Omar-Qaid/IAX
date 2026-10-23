using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class TrackingDimensionGroupInputValidator : BaseValidator<TrackingDimensionGroupInputDto>
{
    public TrackingDimensionGroupInputValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("Group ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.UseBatchNumber).Equal(false).WithMessage("Batch tracking is outside the basic inventory scope.");
        RuleFor(x => x.UseSerialNumber).Equal(false).WithMessage("Serial tracking is outside the basic inventory scope.");
    }
}
