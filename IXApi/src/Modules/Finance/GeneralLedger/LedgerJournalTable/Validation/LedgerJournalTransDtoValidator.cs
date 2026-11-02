using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;

public sealed class LedgerJournalTransDtoValidator : BaseValidator<LedgerJournalTransDto>
{
    public LedgerJournalTransDtoValidator()
    {
        RuleFor(x => x.JournalNum).NotEmpty();
        RuleFor(x => x.TransDate).NotEqual(default(DateTime));
        RuleFor(x => x.AccountType)
            .Must(x => x is LedgerJournalACType.Ledger or LedgerJournalACType.Cust
                         or LedgerJournalACType.Vend or LedgerJournalACType.Bank)
            .WithMessage("The basic general journal accepts Ledger, Cust, Vend and Bank account lines.");
        RuleFor(x => x.LedgerDimension).GreaterThan(0);
        RuleFor(x => x.AmountCurDebit).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.AmountCurCredit).GreaterThanOrEqualTo(0m);
        RuleFor(x => x).Must(x => (x.AmountCurDebit > 0m) != (x.AmountCurCredit > 0m))
            .WithMessage("Enter a nonzero debit or credit, not both.");
        RuleFor(x => x.OffsetAccountType).IsInEnum();
        RuleFor(x => x.OffsetAccountType)
            .Equal(LedgerJournalACType.Ledger)
            .When(x => x.OffsetLedgerDimension > 0)
            .WithMessage("The basic general journal accepts Ledger offset accounts only.");
        RuleFor(x => x.ReverseEntry).IsInEnum();
        RuleFor(x => x.ReverseDate)
            .NotEqual(default(DateTime))
            .When(x => x.ReverseEntry == NoYes.Yes);
        RuleFor(x => x.NoEdit).IsInEnum();
    }
}
