using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Validation;

public sealed class LedgerDtoValidator : BaseValidator<LedgerDto>
{
    public LedgerDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.ChartOfAccounts).GreaterThan(0);
        RuleFor(x => x.FiscalCalendar).GreaterThan(0);
        RuleFor(x => x.AccountingCurrency).NotEmpty();
        RuleFor(x => x.DefaultExchangeRateType).GreaterThan(0);
        RuleFor(x => x.ReportingCurrencyExchangeRateType)
            .GreaterThan(0)
            .When(x => !string.IsNullOrWhiteSpace(x.ReportingCurrency));
    }
}
