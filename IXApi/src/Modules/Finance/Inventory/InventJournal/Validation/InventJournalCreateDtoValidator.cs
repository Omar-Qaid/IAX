using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalCreateDtoValidator : BaseValidator<InventJournalCreateDto>
{
    public InventJournalCreateDtoValidator()
    {
        RuleFor(x => x.JournalNameId).NotEmpty().MaximumLength(FieldLengths.JournalNameId);
    }
}
