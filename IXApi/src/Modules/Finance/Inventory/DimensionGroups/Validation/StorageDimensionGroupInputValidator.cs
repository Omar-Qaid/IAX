using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class StorageDimensionGroupInputValidator : BaseValidator<StorageDimensionGroupInputDto>
{
    public StorageDimensionGroupInputValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("Group ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.UseSite).Equal(true).WithMessage("Site is required in the basic inventory scope.");
        RuleFor(x => x.UseWarehouse).Equal(true).WithMessage("Warehouse is required in the basic inventory scope.");
        RuleFor(x => x.UseLocation).Equal(false).WithMessage("Location tracking is outside the basic inventory scope.");
    }
}
