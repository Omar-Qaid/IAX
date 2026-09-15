using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class SysBackgroundJobExecutionDto
    {
        public long RecId { get; set; }
        public long JobId { get; set; }
        public string? JobCaption { get; set; }
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

        public int AlertsProcessed { get; set; }
        public string? BatchCreatedBy { get; set; }
        public string? CanceledBy { get; set; }
        public string? Caption { get; set; }
        public string? DataPartition { get; set; }
        public int? EndDateTimeTzId { get; set; }
        public int Finishing { get; set; }
        public DateTime? OrigStartDateTime { get; set; }
        public int? OrigStartDateTimeTzId { get; set; }
        public int? StartDateTimeTzId { get; set; }
        public string? ExecutedBy { get; set; }
        public int RuntimeJob { get; set; }
        public string? BatchGroup { get; set; }
        public int GroupSchedulingPriority { get; set; }
        public int JobSchedulingPriority { get; set; }
        public int JobSchedulingPriorityIsOverridden { get; set; }
    }
}