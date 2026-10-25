using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventLocationResponseDtoValidator : BaseValidator<InventLocationResponseDto>
{
    public InventLocationResponseDtoValidator()
    {
        RuleFor(x => x.InventLocationId)
            .NotEmpty().WithMessage("Warehouse ID is required.")
            .MaximumLength(FieldLengths.InventLocationId).WithMessage($"Warehouse ID cannot exceed {FieldLengths.InventLocationId} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Name cannot exceed {FieldLengths.Name} characters.");

        RuleFor(x => x.InventSiteId)
            .NotEmpty().WithMessage("Site ID is required.")
            .MaximumLength(FieldLengths.InventSiteId).WithMessage($"Site ID cannot exceed {FieldLengths.InventSiteId} characters.");
    }
}
