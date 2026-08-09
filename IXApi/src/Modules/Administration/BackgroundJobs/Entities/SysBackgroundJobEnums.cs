namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Entities
{
    /// <summary>
    /// How a background job is scheduled to run.
    /// </summary>
    public enum SysJobScheduleType
    {
        /// <summary>Runs once at a specific point in time (RunAt).</summary>
        OneTime = 0,

        /// <summary>Runs once after a delay from creation (RunAt = now + delay).</summary>
        Delayed = 1,

        /// <summary>Runs repeatedly on a fixed interval (IntervalSeconds).</summary>
        Recurring = 2,

        /// <summary>Runs on a CRON schedule (CronExpression).</summary>
        Cron = 3
    }

    /// <summary>
    /// Lifecycle status of a job definition.
    /// </summary>
    public enum SysJobStatus
    {
        /// <summary>Job is active and will be picked up by the scheduler when due.</summary>
        Active = 0,

        /// <summary>Job is paused — it will not be executed until resumed.</summary>
        Paused = 1,

        /// <summary>Job has been cancelled and will never run again.</summary>
        Cancelled = 2,

        /// <summary>One-time/delayed job has finished its single run.</summary>
        Completed = 3
    }

    /// <summary>
    /// Status of an individual execution attempt.
    /// </summary>
    public enum SysJobExecutionStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    /// <summary>
    /// What caused an execution to start.
    /// </summary>
    public enum SysJobTrigger
    {
        /// <summary>The scheduler fired the job because it was due.</summary>
        Schedule = 0,

        /// <summary>A user manually triggered the job via the API.</summary>
        Manual = 1,

        /// <summary>An automatic retry after a previous failed attempt.</summary>
        Retry = 2
    }
}
