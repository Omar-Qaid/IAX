using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendorInvoicePostDtoValidator : BaseValidator<VendorInvoicePostDto>
{
    public VendorInvoicePostDtoValidator()
    {
        RuleFor(x => x.InvoiceId).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A vendor invoice number is required.");
        RuleFor(x => x.Lines).Must(x => x is { Count: > 0 })
            .WithMessage("At least one line is required.");
    }
}
