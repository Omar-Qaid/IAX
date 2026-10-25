using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ReleasedProductListDtoValidator : BaseValidator<ReleasedProductListDto>
{
    public ReleasedProductListDtoValidator()
    {
        RuleFor(x => x.ItemNumber)
            .NotEmpty().WithMessage("Item number is required.")
            .MaximumLength(FieldLengths.ItemId);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(FieldLengths.Name);
    }
}
