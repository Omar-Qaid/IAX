using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalTableDtoValidator : BaseValidator<InventJournalTableDto>
{
    public InventJournalTableDtoValidator()
    {
        RuleFor(x => x.JournalId).NotEmpty().MaximumLength(FieldLengths.JournalId);
        RuleFor(x => x.JournalNameId).NotEmpty().MaximumLength(FieldLengths.JournalNameId);
    }
}
