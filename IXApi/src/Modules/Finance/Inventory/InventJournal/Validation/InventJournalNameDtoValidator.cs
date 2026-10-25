using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalNameDtoValidator : BaseValidator<InventJournalNameDto>
{
    public InventJournalNameDtoValidator()
    {
        RuleFor(x => x.JournalNameId).NotEmpty().MaximumLength(FieldLengths.JournalNameId);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
    }
}
