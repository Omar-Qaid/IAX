using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Settlement.Validation;

/// <summary>
/// Shape of the request only. Whether the selected entries may actually be settled — one account,
/// one currency, balanced, not already settled — is decided by the settlement service.
/// </summary>
public sealed class LedgerSettlementRequestDtoValidator : BaseValidator<LedgerSettlementRequestDto>
{
    public LedgerSettlementRequestDtoValidator()
    {
        RuleFor(x => x.AccountEntryRecIds)
            .NotNull().WithMessage("Select the posted ledger entries to settle.")
            .Must(x => x.Count >= 2).WithMessage("Select at least two posted ledger entries to settle.")
            .Must(x => x.All(id => id > 0)).WithMessage("A selected ledger entry reference is invalid.");
    }
}
