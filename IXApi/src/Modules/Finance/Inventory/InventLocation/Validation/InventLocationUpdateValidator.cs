using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventLocationUpdateValidation(string ExistingCode, LocationInputDto Input);

public sealed class InventLocationUpdateValidator : BaseValidator<InventLocationUpdateValidation>
{
    public InventLocationUpdateValidator()
    {
        RuleFor(request => request.Input.InventLocationId)
            .Must((request, code) => string.Equals(request.ExistingCode, code.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Warehouse ID cannot be changed after creation.");
    }
}
