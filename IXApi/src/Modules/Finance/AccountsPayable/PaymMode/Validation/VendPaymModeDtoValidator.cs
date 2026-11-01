using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendPaymModeDtoValidator : BaseValidator<VendPaymModeDto>
{
    public VendPaymModeDtoValidator()
    {
        RuleFor(x => x.PaymMode).NotEmpty().MaximumLength(FieldLengths.PaymModeId);
        RuleFor(x => x.Name).MaximumLength(FieldLengths.Name);
        RuleFor(x => x.PaymJournalNameId).MaximumLength(FieldLengths.JournalNameId);
    }
}
