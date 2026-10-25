using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.Validation;

public sealed class FiscalCalendarYearDtoValidator : BaseValidator<FiscalCalendarYearDto>
{
    public FiscalCalendarYearDtoValidator()
    {
        RuleFor(x => x.FiscalCalendar).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
