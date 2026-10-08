using FluentValidation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

public sealed class SalesOrderConfirmationValidator : AbstractValidator<PostConfirmationRequest>
{
    public SalesOrderConfirmationValidator()
    {
        RuleFor(request => request.Posting)
            .Equal(true)
            .WithMessage("Posting must be enabled to create a confirmation.");
        RuleFor(request => request.ConfirmationDate)
            .Must(date => !date.HasValue || date.Value.Year >= 1900)
            .WithMessage("Confirmation date is outside the supported range.");
    }
}
