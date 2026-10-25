using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.Validation;

public sealed class FiscalCalendarDtoValidator : BaseValidator<FiscalCalendarDto>
{
    public FiscalCalendarDtoValidator()
    {
        RuleFor(x => x.CalendarId).NotEmpty().MaximumLength(50);
    }
}
