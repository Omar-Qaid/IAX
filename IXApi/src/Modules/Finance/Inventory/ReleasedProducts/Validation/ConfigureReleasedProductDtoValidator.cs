using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ConfigureReleasedProductDtoValidator : BaseValidator<ConfigureReleasedProductDto>
{
    public ConfigureReleasedProductDtoValidator()
    {
        RuleFor(x => x.InventoryPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Inventory price cannot be negative.");

        RuleFor(x => x.SalesPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sales price cannot be negative.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price cannot be negative.");
    }
}
