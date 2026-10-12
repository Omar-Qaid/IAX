namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public enum BatchSetupStatus { Ok, NotFound, Invalid, Conflict }

public sealed record BatchSetupResult<T>(BatchSetupStatus Status, T? Value = default, string? Error = null);
