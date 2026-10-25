using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class WMSLocationDtoValidator : BaseValidator<WMSLocationDto>
{
    public WMSLocationDtoValidator()
    {
        RuleFor(x => x.InventLocationId)
            .NotEmpty().WithMessage("Warehouse is required.")
            .MaximumLength(FieldLengths.InventLocationId);

        RuleFor(x => x.WMSLocationId)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(FieldLengths.WmsLocationId);
    }
}
