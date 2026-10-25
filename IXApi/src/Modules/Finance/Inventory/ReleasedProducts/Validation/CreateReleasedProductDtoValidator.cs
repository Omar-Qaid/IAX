using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class CreateReleasedProductDtoValidator : BaseValidator<CreateReleasedProductDto>
{
    public CreateReleasedProductDtoValidator()
    {
        RuleFor(x => x.ProductNumber)
            .NotEmpty().WithMessage("Product number is required.")
            .MaximumLength(FieldLengths.ProductNumber);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(FieldLengths.Name);
    }
}
