using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;

public sealed class LedgerJournalNameDtoValidator : BaseValidator<LedgerJournalNameDto>
{
    public LedgerJournalNameDtoValidator()
    {
    }
}
