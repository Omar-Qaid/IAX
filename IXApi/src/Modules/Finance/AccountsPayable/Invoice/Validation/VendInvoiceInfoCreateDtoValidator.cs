using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendInvoiceInfoCreateDtoValidator : BaseValidator<VendInvoiceInfoCreateDto>
{
    public VendInvoiceInfoCreateDtoValidator()
    {
        RuleFor(x => x.Num).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A vendor invoice number is required.");

        // An empty line list is allowed and means the default selection: everything received and
        // not yet invoiced. A list that is present must not carry a non-positive quantity.
        RuleFor(x => x.Lines).Must(x => x == null || x.All(line => line.Qty > 0))
            .WithMessage("An invoice quantity must be greater than zero.");
    }
}

public sealed class VendInvoiceInfoTableDtoValidator : BaseValidator<VendInvoiceInfoTableDto>
{
    public VendInvoiceInfoTableDtoValidator()
    {
        RuleFor(x => x.Num).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A vendor invoice number is required.");
        RuleFor(x => x.InvoiceAccount).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("An invoice vendor is required.");
    }
}
