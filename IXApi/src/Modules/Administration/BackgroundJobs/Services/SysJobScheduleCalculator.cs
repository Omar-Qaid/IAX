using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Pure scheduling math shared by the manager and the execution engine.
    /// Computes the next UTC run time for a job from a reference point.
    /// </summary>
    public static class SysJobScheduleCalculator
    {
        private sealed class RecurrenceSpec
        {
            public string Unit { get; set; } = "seconds";
            public int Interval { get; set; } = 1;
            public int? EndAfter { get; set; }
            public DateTime? EndBy { get; set; }
        }

        private static bool TryReadSpec(byte[]? data, out RecurrenceSpec spec)
        {
            spec = new RecurrenceSpec();
            var text = System.Text.Encoding.UTF8.GetString(data ?? []);
            if (int.TryParse(text, out var seconds) && seconds > 0)
            {
                spec.Interval = seconds;
                return true;
            }
            if (data?.Length == 4)
            {
                seconds = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data);
                if (seconds > 0) { spec.Interval = seconds; return true; }
            }
            try
            {
                var parsed = System.Text.Json.JsonSerializer.Deserialize<RecurrenceSpec>(text,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed == null || parsed.Interval <= 0 ||
                    parsed.Unit is not ("seconds" or "minutes" or "hours" or "days" or "weeks" or "months" or "years") ||
                    parsed.EndAfter is <= 0)
                    return false;
                spec = parsed;
                return true;
            }
            catch (System.Text.Json.JsonException) { return false; }
        }

        /// <summary>
        /// Computes the next run time strictly after <paramref name="fromUtc"/> for the job's
        /// schedule. Returns null when the job has no further runs (e.g. a fired one-time job).
        /// </summary>
        public static DateTime? ComputeNextRun(SysBackgroundJob job, DateTime fromUtc)
        {
            var firstRun = job.RunCount == 0 && job.StartDateTime > fromUtc
                ? job.StartDateTime : null;

            switch (job.ScheduleType)
            {
                case SysJobScheduleType.OneTime:
                case SysJobScheduleType.Delayed:
                    // A one-shot only has a "next run" until it has run once.
                    return job.RunCount > 0 ? null : job.StartDateTime;

                case SysJobScheduleType.Recurring:
                    if (!TryReadSpec(job.RecurrenceData, out var recurrence) ||
                        (recurrence.EndAfter.HasValue && job.RunCount >= recurrence.EndAfter.Value))
                        return null;
                    var next = firstRun ?? recurrence.Unit switch
                    {
                        "minutes" => fromUtc.AddMinutes(recurrence.Interval),
                        "hours" => fromUtc.AddHours(recurrence.Interval),
                        "days" => fromUtc.AddDays(recurrence.Interval),
                        "weeks" => fromUtc.AddDays(7 * recurrence.Interval),
                        "months" => fromUtc.AddMonths(recurrence.Interval),
                        "years" => fromUtc.AddYears(recurrence.Interval),
                        _ => fromUtc.AddSeconds(recurrence.Interval),
                    };
                    return recurrence.EndBy.HasValue && next > recurrence.EndBy.Value.ToUniversalTime()
                        ? null : next;
                case SysJobScheduleType.Cron:
                    if (firstRun.HasValue) return firstRun;
                    return SysCronExpression.TryParse(System.Text.Encoding.UTF8.GetString(job.RecurrenceData ?? []), out var cron)
                        ? cron!.GetNextOccurrence(fromUtc) : null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Validates a create/update schedule combination, returning an error message or null.
        /// </summary>
        public static string? ValidateSchedule(
            SysJobScheduleType type, byte[]? recurrenceData, DateTime? startDateTime, int? delaySeconds)
        {
            return type switch
            {
                SysJobScheduleType.Recurring when !TryReadSpec(recurrenceData, out _)
                    => "RecurrenceData must contain a positive interval in seconds encoded as UTF-8.",
                SysJobScheduleType.Cron when !SysCronExpression.TryParse(System.Text.Encoding.UTF8.GetString(recurrenceData ?? []), out _)
                    => "RecurrenceData must contain a valid five-field CRON expression encoded as UTF-8.",
                SysJobScheduleType.Cron or SysJobScheduleType.Recurring when recurrenceData == null || recurrenceData.Length == 0
                    => "RecurrenceData is required for Cron and Recurring jobs.",
                SysJobScheduleType.OneTime when startDateTime is null
                    => "StartDateTime is required for OneTime jobs.",
                SysJobScheduleType.Delayed when (delaySeconds is null or <= 0) && startDateTime is null
                    => "DelaySeconds (or StartDateTime) is required for Delayed jobs.",
                _ => null
            };
        }
    }
}
