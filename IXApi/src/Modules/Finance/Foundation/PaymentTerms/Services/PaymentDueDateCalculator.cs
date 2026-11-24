using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;

public static class PaymentDueDateCalculator
{
    public static DateTime Calculate(DateTime invoiceDate, PaymTerm term)
    {
        // A payment schedule (PaymSched) is applied after this: the due date here is the first installment's,
        // and PaymentScheduleOpenRows splits the open transaction into one row per installment.
        if (!string.IsNullOrWhiteSpace(term.PaymDayId) || term.CutOffDay != 0 || term.AdditionalMonths != 0)
            throw new InvalidOperationException("Payment-day calendars require a supported payment-day setup before invoicing.");
        if (term.NumOfMonths < 0 || term.NumOfDays < 0 || term.AdditionalMonths < 0)
            throw new InvalidOperationException("Payment-term offsets cannot be negative.");
        var date = invoiceDate.Date;
        return term.PaymMethod switch
        {
            PaymMethod.COD => date,
            PaymMethod.Net => date.AddMonths(term.NumOfMonths).AddDays(term.NumOfDays),
            PaymMethod.CurrentMonth => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))
                .AddMonths(term.NumOfMonths).AddDays(term.NumOfDays),
            _ => throw new InvalidOperationException("The configured payment-term method is not supported for invoice due-date calculation.")
        };
    }
}

