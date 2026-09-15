using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class UpdateSysBackgroundJobScheduleDto
    {
        [MaxLength(200)] public string? Caption { get; set; }
        public SysJobScheduleType ScheduleType { get; set; }
        
        public byte[]? RecurrenceData { get; set; }
        
        public DateTime? StartDateTime { get; set; }
        public int? StartDateTimeTzId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? StartTime { get; set; }
        
        public int? DelaySeconds { get; set; }

        public bool? IsEnabled { get; set; }
        public bool? PreventOverlap { get; set; }
        
        public int? SchedulingPriority { get; set; }
        public int? Critical { get; set; }
        public int? MonitoringCategory { get; set; }
        public int? Managed { get; set; }
        public int? EmitBusinessEvent { get; set; }

        [MaxLength(10)]
        public string? BatchGroup { get; set; }

        [MaxLength(10)]
        public string? ActivePeriod { get; set; }

        public int? MaxRetryCount { get; set; }
        public int? RetryDelaySeconds { get; set; }
        public int? TimeoutSeconds { get; set; }
        public string? PayloadJson { get; set; }
        public string? Description { get; set; }
    }
}
