using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class SysBackgroundJobExecutionDto
    {
        public long RecId { get; set; }
        public long JobId { get; set; }
        public string? JobName { get; set; }
        public int Attempt { get; set; }
        public SysJobTrigger Trigger { get; set; }
        public string? TriggeredByUserId { get; set; }
        public SysJobExecutionStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? DurationMs { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ServerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}