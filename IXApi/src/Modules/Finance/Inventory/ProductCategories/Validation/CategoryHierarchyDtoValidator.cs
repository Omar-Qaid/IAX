using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

public sealed class CategoryHierarchyDtoValidator : BaseValidator<CategoryHierarchyDto>
{
    public CategoryHierarchyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category hierarchy name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Category hierarchy name cannot exceed {FieldLengths.Name} characters.");
    }
}
