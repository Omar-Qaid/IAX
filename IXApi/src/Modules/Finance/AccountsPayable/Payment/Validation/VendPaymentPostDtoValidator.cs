using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Shape only. The vendor and bank account are required here so a blank request is refused at the
/// edge with a field error, instead of reaching the service and coming back as "the vendor was not
/// found". The business rules - a positive amount at four decimals, the request identifier and its
/// length, and the on-account versus selected-invoice conflict - stay in
/// <see cref="VendPaymentService"/>, which is the only place that can protect a non-HTTP caller;
/// they are deliberately not repeated here.
/// </summary>
public sealed class VendPaymentPostDtoValidator : BaseValidator<VendPaymentPostDto>
{
    public VendPaymentPostDtoValidator()
    {
        RuleFor(x => x.VendAccount).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A vendor account is required.");
        RuleFor(x => x.BankAccountId).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A bank account is required.");
    }
}
