using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalNameInputValidator : BaseValidator<InventJournalNameInputDto>
{
    public InventJournalNameInputValidator()
    {
        RuleFor(x => x.JournalNameId).NotEmpty().MaximumLength(FieldLengths.JournalNameId);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        RuleFor(x => x.JournalType).IsInEnum();
        RuleFor(x => x.DetailSummary).IsInEnum();
        RuleFor(x => x.Reservation).IsInEnum();
    }
}
