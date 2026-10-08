using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed class MarkupTransValidator : BaseValidator<MarkupTransDto>
{
    public MarkupTransValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(input => input.MarkupCode).NotEmpty().WithMessage("Charges code is required.");
        RuleFor(input => input.MarkupCategory).Must(category => Enum.IsDefined(category))
            .WithMessage("Charge category must be Fixed, Pcs, or Percentage.");
        RuleFor(input => input.Value).GreaterThanOrEqualTo(0)
            .WithMessage("Charges value cannot be negative.");
        RuleFor(input => input.Value).LessThanOrEqualTo(100)
            .When(input => input.MarkupCategory == MarkupCategory.Percent)
            .WithMessage("Percentage charges value cannot exceed 100.");
    }
}
