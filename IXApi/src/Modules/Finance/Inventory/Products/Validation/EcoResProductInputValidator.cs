using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class EcoResProductInputValidator : BaseValidator<EcoResProductInputDto>
{
    public EcoResProductInputValidator()
    {
        RuleFor(x => x.ProductNumber).NotEmpty().WithMessage("Product number is required.")
            .MaximumLength(FieldLengths.ProductNumber);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(FieldLengths.Name);
        RuleFor(x => x.NameAlias).NotEmpty().WithMessage("Arabic name is required.")
            .MaximumLength(FieldLengths.Name);
        RuleFor(x => x.SearchName).MaximumLength(FieldLengths.ProductSearchName);
        RuleFor(x => x.Description).MaximumLength(FieldLengths.Memo);
    }
}
