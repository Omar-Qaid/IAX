namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Tunable options for the background job execution engine.
    /// Bind from configuration section "BackgroundJobs" if desired.
    /// </summary>
    public class SysBackgroundJobOptions
    {
        /// <summary>How often the engine polls the store for due/pending work.</summary>
        public int PollIntervalSeconds { get; set; } = 10;

        /// <summary>Maximum number of jobs allowed to execute concurrently across the instance.</summary>
        public int MaxConcurrency { get; set; } = 4;

        /// <summary>Max pending executions claimed per poll cycle.</summary>
        public int BatchSize { get; set; } = 20;

        /// <summary>Whether the engine is enabled. Set false to run an API-only instance.</summary>
        public bool Enabled { get; set; } = true;
    }
}
