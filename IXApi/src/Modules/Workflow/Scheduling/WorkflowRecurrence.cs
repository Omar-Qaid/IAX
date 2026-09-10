namespace IAX.IXApi.Modules.Workflow.Scheduling;

/// <summary>Calendar recurrence anchored to the original local date, avoiding month-end drift.</summary>
public static class WorkflowRecurrence
{
    public static DateTime NextUtc(DateTime startsAt, string timeZone, string frequency, DateTime afterUtc)
    {
        if (frequency is not ("daily" or "weekly" or "monthly" or "yearly"))
            throw new ArgumentException("Unsupported schedule frequency.");
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        var anchor = DateTime.SpecifyKind(startsAt, DateTimeKind.Unspecified);
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(afterUtc, DateTimeKind.Utc), zone);
        var index = Math.Max(0, frequency switch
        {
            "daily" => (localNow.Date - anchor.Date).Days,
            "weekly" => (localNow.Date - anchor.Date).Days / 7,
            "monthly" => (localNow.Year - anchor.Year) * 12 + localNow.Month - anchor.Month,
            _ => localNow.Year - anchor.Year
        });
        for (; index < 100000; index++)
        {
            var local = frequency switch
            {
                "daily" => anchor.AddDays(index),
                "weekly" => anchor.AddDays(index * 7),
                "monthly" => anchor.AddMonths(index),
                _ => anchor.AddYears(index)
            };
            // Skip nonexistent daylight-saving wall times. Ambiguous times use standard time.
            if (zone.IsInvalidTime(local)) continue;
            var utc = TimeZoneInfo.ConvertTimeToUtc(local, zone);
            if (utc > afterUtc) return utc;
        }
        throw new ArgumentException("Schedule has no representable future occurrence.");
    }
}
