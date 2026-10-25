using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventParametersDtoValidator : BaseValidator<InventParametersDto>
{
    public InventParametersDtoValidator()
    {
        RuleFor(x => x.DefaultUnitId)
            .MaximumLength(FieldLengths.Unit).WithMessage($"Default unit ID cannot exceed {FieldLengths.Unit} characters.");

        RuleFor(x => x.MultiSiteDefaultInventSiteId)
            .MaximumLength(FieldLengths.InventSiteId).WithMessage($"Default site ID cannot exceed {FieldLengths.InventSiteId} characters.");

        RuleFor(x => x.MultiSiteDefaultInventLocationId)
            .MaximumLength(FieldLengths.InventLocationId).WithMessage($"Default warehouse ID cannot exceed {FieldLengths.InventLocationId} characters.");

        RuleFor(x => x.MovementJournalNameId)
            .MaximumLength(FieldLengths.JournalNameId).WithMessage($"Movement journal name cannot exceed {FieldLengths.JournalNameId} characters.");

        RuleFor(x => x.LossProfitJournalNameId)
            .MaximumLength(FieldLengths.JournalNameId).WithMessage($"Loss/profit journal name cannot exceed {FieldLengths.JournalNameId} characters.");

        RuleFor(x => x.TransferJournalNameId)
            .MaximumLength(FieldLengths.JournalNameId).WithMessage($"Transfer journal name cannot exceed {FieldLengths.JournalNameId} characters.");

        RuleFor(x => x.CountJournalNameId)
            .MaximumLength(FieldLengths.JournalNameId).WithMessage($"Count journal name cannot exceed {FieldLengths.JournalNameId} characters.");
    }
}
