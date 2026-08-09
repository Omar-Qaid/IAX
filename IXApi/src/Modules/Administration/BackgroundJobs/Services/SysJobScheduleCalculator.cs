using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Pure scheduling math shared by the manager and the execution engine.
    /// Computes the next UTC run time for a job from a reference point.
    /// </summary>
    public static class SysJobScheduleCalculator
    {
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
                    return job.RunCount > 0 ? null : job.RunAt;

                case SysJobScheduleType.Recurring:
                    if (job.IntervalSeconds is null or <= 0) return null;
                    return fromUtc.AddSeconds(job.IntervalSeconds.Value);

                case SysJobScheduleType.Cron:
                    if (string.IsNullOrWhiteSpace(job.CronExpression)) return null;
                    return SysCronExpression.TryParse(job.CronExpression, out var cron)
                        ? cron!.GetNextOccurrence(fromUtc)
                        : null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Validates a create/update schedule combination, returning an error message or null.
        /// </summary>
        public static string? ValidateSchedule(
            SysJobScheduleType type, string? cron, int? intervalSeconds, DateTime? runAt, int? delaySeconds)
        {
            return type switch
            {
                SysJobScheduleType.Cron when string.IsNullOrWhiteSpace(cron)
                    => "CronExpression is required for Cron jobs.",
                SysJobScheduleType.Cron when !SysCronExpression.TryParse(cron!, out _)
                    => $"Invalid CRON expression: '{cron}'.",
                SysJobScheduleType.Recurring when intervalSeconds is null or <= 0
                    => "IntervalSeconds must be greater than 0 for Recurring jobs.",
                SysJobScheduleType.OneTime when runAt is null
                    => "RunAt is required for OneTime jobs.",
                SysJobScheduleType.Delayed when (delaySeconds is null or <= 0) && runAt is null
                    => "DelaySeconds (or RunAt) is required for Delayed jobs.",
                _ => null
            };
        }
    }
}
