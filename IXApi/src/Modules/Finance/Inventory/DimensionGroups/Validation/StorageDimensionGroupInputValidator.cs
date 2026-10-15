using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class StorageDimensionGroupInputValidator : BaseValidator<StorageDimensionGroupInputDto>
{
    public StorageDimensionGroupInputValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("Group ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x).Must(x => !x.UseLocation || x.UseWarehouse).WithMessage("Location requires Warehouse.");
        RuleFor(x => x).Must(x => !x.UseWarehouse || x.UseSite).WithMessage("Warehouse requires Site.");
    }
}
