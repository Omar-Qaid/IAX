using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendPackingSlipJourDtoValidator : BaseValidator<VendPackingSlipJourDto>
{
    public VendPackingSlipJourDtoValidator()
    {
        RuleFor(x => x.PackingSlipId).NotEmpty().MaximumLength(FieldLengths.PackingSlipId);
        RuleFor(x => x.InternalPackingSlipId).MaximumLength(FieldLengths.PackingSlipId);
        RuleFor(x => x.PurchId).MaximumLength(FieldLengths.PurchId);
        RuleFor(x => x.OrderAccount).NotEmpty().MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.InvoiceAccount).MaximumLength(FieldLengths.VendAccount);
    }
}
