using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendInvoiceJourDtoValidator : BaseValidator<VendInvoiceJourDto>
{
    public VendInvoiceJourDtoValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty().MaximumLength(FieldLengths.InvoiceId);
        RuleFor(x => x.InternalInvoiceId).MaximumLength(FieldLengths.InvoiceId);
        RuleFor(x => x.PurchId).MaximumLength(FieldLengths.PurchId);
        RuleFor(x => x.OrderAccount).NotEmpty().MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.InvoiceAccount).MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.CurrencyCode).MaximumLength(FieldLengths.CurrencyCode);
    }
}
