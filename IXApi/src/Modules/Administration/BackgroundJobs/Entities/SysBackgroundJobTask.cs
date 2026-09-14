using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

[Table("BatchJobTasks")]
public sealed class SysBackgroundJobTask
{
    [Key] public long RecId { get; set; }
    public long JobId { get; set; }
    public SysBackgroundJob Job { get; set; } = null!;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(200), Column("ServiceKey")] public string JobKey { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public int ExecutionOrder { get; set; }
    public long? DependsOnTaskId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int MaxRetryCount { get; set; }
    public int RetryDelaySeconds { get; set; } = 60;
}

[Table("BatchJobTaskHistory")]
public sealed class SysBackgroundJobTaskExecution
{
    [Key] public long RecId { get; set; }
    public long ExecutionId { get; set; }
    public SysBackgroundJobExecution Execution { get; set; } = null!;
    public long TaskId { get; set; }
    public int Attempt { get; set; } = 1;
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    [MaxLength(200)] public string TaskName { get; set; } = string.Empty;
    public SysJobExecutionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Output { get; set; }
    public string? ErrorMessage { get; set; }
}
