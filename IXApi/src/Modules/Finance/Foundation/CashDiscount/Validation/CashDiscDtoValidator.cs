using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.CashDiscount;

public sealed class CashDiscDtoValidator : BaseValidator<CashDiscDto>
{
    public CashDiscDtoValidator()
    {
        RuleFor(x => x.CashDiscCode).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A cash discount code is required.");
        RuleFor(x => x.DiscPct).InclusiveBetween(0m, 100m)
            .WithMessage("The cash discount percentage must be between 0 and 100.");
        RuleFor(x => x.NumOfMonths).GreaterThanOrEqualTo(0)
            .WithMessage("The cash discount period cannot be negative.");
        RuleFor(x => x.NumOfDays).GreaterThanOrEqualTo(0)
            .WithMessage("The cash discount period cannot be negative.");
    }
}
