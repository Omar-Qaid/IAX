using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalLineInputDtoValidator : BaseValidator<InventJournalLineInputDto>
{
    public InventJournalLineInputDtoValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().MaximumLength(FieldLengths.ItemId);
    }
}
