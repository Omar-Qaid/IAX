using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendBankAccountDtoValidator : BaseValidator<VendBankAccountDto>
{
    public VendBankAccountDtoValidator()
    {
        RuleFor(x => x.AccountID).NotEmpty().MaximumLength(FieldLengths.PaymModeId);
        RuleFor(x => x.VendAccount).NotEmpty().MaximumLength(FieldLengths.VendAccount);
        RuleFor(x => x.Name).MaximumLength(FieldLengths.Name);
        RuleFor(x => x.AccountNum).NotEmpty().MaximumLength(48);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(FieldLengths.CurrencyCode);
    }
}
