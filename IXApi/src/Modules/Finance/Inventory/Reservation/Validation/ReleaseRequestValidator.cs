using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ReleaseRequestValidator : BaseValidator<InventoryReservationController.ReleaseRequest>
{
    public ReleaseRequestValidator()
    {
        RuleFor(x => x.InventTransId)
            .NotEmpty().WithMessage("Inventory transaction ID is required.")
            .MaximumLength(FieldLengths.InventTransId);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
