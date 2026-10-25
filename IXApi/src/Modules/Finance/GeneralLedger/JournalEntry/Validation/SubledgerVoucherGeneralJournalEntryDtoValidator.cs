using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry.Validation;

public sealed class SubledgerVoucherGeneralJournalEntryDtoValidator : BaseValidator<SubledgerVoucherGeneralJournalEntryDto>
{
    public SubledgerVoucherGeneralJournalEntryDtoValidator()
    {
    }
}
