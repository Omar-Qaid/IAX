using FluentValidation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderCopyValidator : AbstractValidator<SalesOrderCopyRequest>
{
    public SalesOrderCopyValidator()
    {
        RuleFor(request => request.Mode)
            .Must(mode => string.Equals(mode, "fromAll", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "fromJournal", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Copy mode must be fromAll or fromJournal.");
        RuleFor(request => request.LineIds)
            .NotEmpty().WithMessage("Select at least one source line.");
        RuleFor(request => request.LineIds)
            .Must(ids => ids != null && ids.Distinct().All(value => long.TryParse(value, out _)))
            .WithMessage("One or more source line identifiers are invalid.")
            .When(request => request.LineIds is { Count: > 0 });
        RuleFor(request => request.QuantityFactor)
            .GreaterThan(0).WithMessage("Quantity factor must be greater than zero.");
        RuleFor(request => request.InvertSign)
            .Equal(false).WithMessage("Reverse-sign copying requires a return-order flow and is not supported for sales orders.");
    }
}
