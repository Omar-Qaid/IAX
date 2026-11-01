using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;

public sealed class LedgerJournalTableDtoValidator : BaseValidator<LedgerJournalTableDto>
{
    public LedgerJournalTableDtoValidator()
    {
        RuleFor(x => x.JournalName).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.JournalType).IsInEnum();
        RuleFor(x => x.VoucherAllocatedAtPosting).IsInEnum();
        RuleFor(x => x.OffsetAccountType).IsInEnum();
        RuleFor(x => x.FixedOffsetAccount).IsInEnum();
        RuleFor(x => x.FixedExchRate).IsInEnum();
        RuleFor(x => x.Posted).IsInEnum();
        RuleFor(x => x.SystemBlocked).IsInEnum();
        RuleFor(x => x.ReverseEntry).IsInEnum();
        RuleFor(x => x.ReverseDate)
            .NotEqual(default(DateTime))
            .When(x => x.ReverseEntry == NoYes.Yes);
    }
}
