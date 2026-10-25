using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventOnHandDtoValidator : BaseValidator<InventOnHandDto>
{
    public InventOnHandDtoValidator()
    {
        RuleFor(x => x.ItemNumber)
            .NotEmpty().WithMessage("Item number is required.")
            .MaximumLength(FieldLengths.ItemId);
    }
}
