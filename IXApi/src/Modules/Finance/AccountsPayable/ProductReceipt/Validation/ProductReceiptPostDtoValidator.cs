using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class ProductReceiptPostDtoValidator : BaseValidator<ProductReceiptPostDto>
{
    public ProductReceiptPostDtoValidator()
    {
        RuleFor(x => x.PackingSlipId).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A product receipt number is required.");
        RuleFor(x => x.Lines).Must(x => x is { Count: > 0 })
            .WithMessage("At least one line is required.");
    }
}
