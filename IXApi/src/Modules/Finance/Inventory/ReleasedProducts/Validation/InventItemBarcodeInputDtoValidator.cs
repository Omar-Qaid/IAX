using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemBarcodeInputDtoValidator : BaseValidator<InventItemBarcodeInputDto>
{
    public InventItemBarcodeInputDtoValidator()
    {
        RuleFor(x => x.ItemBarcode)
            .NotEmpty().WithMessage("Barcode is required.")
            .MaximumLength(FieldLengths.ItemBarcode);

        RuleFor(x => x.UnitId)
            .NotEmpty().WithMessage("Unit ID is required.")
            .MaximumLength(FieldLengths.UnitId);

        RuleFor(x => x.Qty)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
