using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;

public sealed class LedgerJournalNameDtoValidator : BaseValidator<LedgerJournalNameDto>
{
    public LedgerJournalNameDtoValidator()
    {
        RuleFor(x => x.JournalName).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NumberSequenceTable).GreaterThan(0);
        RuleFor(x => x.JournalType).IsInEnum();
        RuleFor(x => x.NewVoucher).IsInEnum();
        RuleFor(x => x.VoucherAllocatedAtPosting).IsInEnum();
        RuleFor(x => x.DetailSummary).IsInEnum();
        RuleFor(x => x.OffsetAccountType).IsInEnum();
        RuleFor(x => x.FixedOffsetAccount).IsInEnum();
        RuleFor(x => x.CurrencyCode).NotEmpty();
        RuleFor(x => x.FixedExchRate).IsInEnum();
        RuleFor(x => x.OffsetLedgerDimension)
            .GreaterThan(0)
            .When(x => x.FixedOffsetAccount == NoYes.Yes);
    }
}
