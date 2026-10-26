namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Governs whether a seeder runs on every startup or once per version/scope.
    /// </summary>
    public enum SeederExecutionPolicy
    {
        /// <summary>
        /// Seeder is idempotent and runs whenever eligible to reconcile state.
        /// </summary>
        Repeatable = 1,

        /// <summary>
        /// Seeder runs only once per unique (Id, Version, Scope) tuple unless a version change is registered.
        /// </summary>
        RunOnce = 2
    }
}
