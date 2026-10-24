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
        RuleFor(x => x.DetailSummary).Equal(DetailSummary.Detail)
            .WithMessage("Basic inventory journals require detail posting.");
        RuleFor(x => x.DeletePostedLines).Equal(false)
            .WithMessage("Basic inventory journals must retain posted lines for audit.");
        RuleFor(x => x.Reservation).Equal(ItemReservation.None)
            .WithMessage("Automatic journal reservation is outside the basic inventory scope.");
    }
}
