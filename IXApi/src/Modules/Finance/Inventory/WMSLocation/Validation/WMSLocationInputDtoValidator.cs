using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class WMSLocationInputDtoValidator : BaseValidator<WMSLocationInputDto>
{
    public WMSLocationInputDtoValidator()
    {
        RuleFor(x => x.InventLocationId)
            .NotEmpty().WithMessage("Warehouse is required.")
            .MaximumLength(FieldLengths.InventLocationId);

        RuleFor(x => x.WMSLocationId)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(FieldLengths.WmsLocationId);

        RuleFor(x => x.LocationType)
            .GreaterThanOrEqualTo(0).WithMessage("Location type must be non-negative.");

        RuleFor(x => x.SortCode)
            .GreaterThanOrEqualTo(0).WithMessage("Sort code must be non-negative.");
    }
}
