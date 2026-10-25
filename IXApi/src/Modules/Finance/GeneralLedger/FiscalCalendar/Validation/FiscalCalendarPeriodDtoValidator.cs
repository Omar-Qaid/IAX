using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.Validation;

public sealed class FiscalCalendarPeriodDtoValidator : BaseValidator<FiscalCalendarPeriodDto>
{
    public FiscalCalendarPeriodDtoValidator()
    {
        RuleFor(x => x.FiscalCalendar).GreaterThan(0);
        RuleFor(x => x.FiscalCalendarYear).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
