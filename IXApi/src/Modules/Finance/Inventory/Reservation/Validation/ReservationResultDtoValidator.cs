using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ReservationResultDtoValidator : BaseValidator<ReservationResultDto>
{
    public ReservationResultDtoValidator()
    {
        RuleFor(x => x.InventTransId)
            .NotEmpty().WithMessage("Inventory transaction ID is required.")
            .MaximumLength(FieldLengths.InventTransId);

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.")
            .MaximumLength(FieldLengths.ItemId);
    }
}
