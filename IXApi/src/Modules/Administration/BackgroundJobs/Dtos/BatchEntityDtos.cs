using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos
{
    public sealed class SysBackgroundJobGroupDto
    {
        public long RecId { get; set; }
        [Required, MaxLength(10)] public string GroupCode { get; set; } = string.Empty;
        [MaxLength(60)] public string? Description { get; set; }
        [Range(0, 2)] public int SchedulingPriority { get; set; } = 1;
        [Range(1, 100)] public int MaxConcurrency { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public byte[]? RowVersion { get; set; }
    }

    public sealed class SysBackgroundJobTaskDto
    {
        public long RecId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(200), System.Text.Json.Serialization.JsonPropertyName("serviceKey")]
        public string JobKey { get; set; } = string.Empty;
        public string? PayloadJson { get; set; }
        [Range(1, int.MaxValue)] public int ExecutionOrder { get; set; } = 1;
        public long? DependsOnTaskId { get; set; }
        public bool IsEnabled { get; set; } = true;
        [Range(0, 10)] public int MaxRetryCount { get; set; }
        [Range(1, 3600)] public int RetryDelaySeconds { get; set; } = 60;
    }

    public sealed class BatchJobActivePeriodDto
    {
        public long RecId { get; set; }
        [Required, MaxLength(10)] public string Id { get; set; } = string.Empty;
        [MaxLength(150)] public string? Name { get; set; }
        [Range(0, 86399)] public int FromTimeUtc { get; set; }
        [Range(0, 86399)] public int ToTimeUtc { get; set; }
        [Range(0, 86399)] public int FromTimeLocal { get; set; }
        [Range(0, 86399)] public int ToTimeLocal { get; set; }
        public int TimeZoneFollowed { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class BatchSettingsDto
    {
        public int Id { get; set; } = 1;
        public bool Enabled { get; set; } = true;
        public int PollIntervalSeconds { get; set; } = 5;
    }
}
