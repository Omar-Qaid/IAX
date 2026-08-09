using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class UpdateSysBackgroundJobScheduleDto
    {
        public SysJobScheduleType ScheduleType { get; set; }
        public string? CronExpression { get; set; }
        public int? IntervalSeconds { get; set; }
        public DateTime? RunAt { get; set; }
        public int? DelaySeconds { get; set; }

        public bool? IsEnabled { get; set; }
        public bool? PreventOverlap { get; set; }
        public int? MaxRetryCount { get; set; }
        public int? RetryDelaySeconds { get; set; }
        public int? TimeoutSeconds { get; set; }
        public string? PayloadJson { get; set; }
        public string? Description { get; set; }
    }
}