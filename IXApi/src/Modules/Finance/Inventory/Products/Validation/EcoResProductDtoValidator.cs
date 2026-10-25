using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class EcoResProductDtoValidator : BaseValidator<EcoResProductDto>
{
    public EcoResProductDtoValidator()
    {
        RuleFor(x => x.ProductNumber).NotEmpty().WithMessage("Product number is required.")
            .MaximumLength(FieldLengths.ProductNumber);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(FieldLengths.Name);
        RuleFor(x => x.NameAlias).MaximumLength(FieldLengths.Name);
        RuleFor(x => x.SearchName).MaximumLength(FieldLengths.ProductSearchName);
        RuleFor(x => x.Description).MaximumLength(FieldLengths.Memo);
    }
}
