using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

public sealed class SalesOrderConfirmationValidator : BaseValidator<PostConfirmationRequest>
{
    public SalesOrderConfirmationValidator()
    {
        RuleFor(request => request.Posting)
            .Equal(true)
            .WithMessage("Posting must be enabled to create a confirmation.");
        RuleFor(request => request.ConfirmationDate)
            .Must(date => !date.HasValue || date.Value.Year >= 1900)
            .WithMessage("Confirmation date is outside the supported range.");
        RuleFor(request => request.RequestId)
            .Must(value => value == null || (!string.IsNullOrWhiteSpace(value)
                && value.Length <= IAX.IXApi.Modules.Finance.Common.FieldLengths.ParmId))
            .WithMessage("The confirmation request ID is invalid.");
    }
}
