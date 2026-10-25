using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendPurchOrderJourDtoValidator : BaseValidator<VendPurchOrderJourDto>
{
    public VendPurchOrderJourDtoValidator()
    {
        RuleFor(x => x.PurchId).NotEmpty().MaximumLength(FieldLengths.PurchId);
        RuleFor(x => x.PurchOrderDocNum).MaximumLength(FieldLengths.DocumentNum);
        RuleFor(x => x.OrderAccount).NotEmpty().MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.InvoiceAccount).MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.CurrencyCode).MaximumLength(FieldLengths.CurrencyCode);
    }
}
