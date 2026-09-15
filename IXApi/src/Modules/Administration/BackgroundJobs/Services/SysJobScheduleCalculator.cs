using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Pure scheduling math shared by the manager and the execution engine.
    /// Computes the next UTC run time for a job from a reference point.
    /// </summary>
    public static class SysJobScheduleCalculator
    {
        private static bool TryReadInterval(byte[]? data, out int seconds)
        {
            if (int.TryParse(System.Text.Encoding.UTF8.GetString(data ?? []), out seconds)) return seconds > 0;
            // Preserve intervals written by the earlier BitConverter-based seeders.
            seconds = data?.Length == 4 ? System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data) : 0;
            return seconds > 0;
        }
        /// <summary>
        /// Computes the next run time strictly after <paramref name="fromUtc"/> for the job's
        /// schedule. Returns null when the job has no further runs (e.g. a fired one-time job).
        /// </summary>
        public static DateTime? ComputeNextRun(SysBackgroundJob job, DateTime fromUtc)
        {
            switch (job.ScheduleType)
            {
                case SysJobScheduleType.OneTime:
                case SysJobScheduleType.Delayed:
                    // A one-shot only has a "next run" until it has run once.
                    return job.RunCount > 0 ? null : job.StartDateTime;

                case SysJobScheduleType.Recurring:
                    return TryReadInterval(job.RecurrenceData, out var seconds)
                        ? fromUtc.AddSeconds(seconds) : null;
                case SysJobScheduleType.Cron:
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
                SysJobScheduleType.Recurring when !TryReadInterval(recurrenceData, out _)
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
