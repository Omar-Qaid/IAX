using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class PurchTableDtoValidator : BaseValidator<PurchTableDto>
{
    public PurchTableDtoValidator()
    {
        RuleFor(x => x.PurchId).MaximumLength(FieldLengths.PurchId);
        RuleFor(x => x.OrderAccount).NotEmpty().MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.InvoiceAccount).MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.ReturnItemNum).MaximumLength(50);
        RuleFor(x => x.InventSiteId).NotEmpty().MaximumLength(FieldLengths.InventSiteId);
        RuleFor(x => x.InventLocationId).NotEmpty().MaximumLength(FieldLengths.InventLocationId);
    }
}
