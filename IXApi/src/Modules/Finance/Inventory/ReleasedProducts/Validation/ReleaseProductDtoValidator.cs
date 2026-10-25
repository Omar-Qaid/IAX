using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ReleaseProductDtoValidator : BaseValidator<ReleaseProductDto>
{
    public ReleaseProductDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ItemNumber)
            .NotEmpty().WithMessage("Item number is required.")
            .MaximumLength(FieldLengths.ItemId);
    }
}
