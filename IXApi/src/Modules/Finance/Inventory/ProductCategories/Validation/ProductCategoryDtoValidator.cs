using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

public sealed class ProductCategoryDtoValidator : BaseValidator<ProductCategoryDto>
{
    public ProductCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Category name cannot exceed {FieldLengths.Name} characters.");
    }
}
