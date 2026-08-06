namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers
{
    /// <summary>
    /// Contract for a unit of background work. Each handler is identified by a unique
    /// <see cref="JobKey"/>; a <c>SysBackgroundJob</c> row references that key to decide
    /// what to run. Implement this interface and register it in DI to add a new job type —
    /// no changes to the scheduler are required (open/closed).
    /// </summary>
    public interface ISysBackgroundJobHandler
    {
        /// <summary>
        /// Stable, unique key used to bind a job definition to this handler
        /// (e.g. "SendInvoiceReminders"). Must match <c>SysBackgroundJob.JobKey</c>.
        /// </summary>
        string JobKey { get; }

        /// <summary>
        /// Executes the job. Throw to signal failure (the engine records the error
        /// and applies the retry policy). Honour the cancellation token for timeouts.
        /// </summary>
        Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken);
    }
}
