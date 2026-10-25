using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.Validation;

public sealed class LedgerFiscalCalendarPeriodDtoValidator : BaseValidator<LedgerFiscalCalendarPeriodDto>
{
    public LedgerFiscalCalendarPeriodDtoValidator()
    {
        RuleFor(x => x.Ledger).GreaterThan(0);
        RuleFor(x => x.FiscalCalendarPeriod).GreaterThan(0);
    }
}
