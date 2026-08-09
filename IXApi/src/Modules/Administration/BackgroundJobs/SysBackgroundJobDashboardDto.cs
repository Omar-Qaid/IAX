using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class SysBackgroundJobDashboardDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int PausedJobs { get; set; }
        public int CancelledJobs { get; set; }

        public int RunningNow { get; set; }
        public int ExecutionsLast24h { get; set; }
        public int SucceededLast24h { get; set; }
        public int FailedLast24h { get; set; }
        public double SuccessRatePct { get; set; }
        public double AvgDurationMsLast24h { get; set; }

        public List<SysBackgroundJobDto> NextDueJobs { get; set; } = new();
        public List<SysBackgroundJobExecutionDto> RecentExecutions { get; set; } = new();
        public List<string> RegisteredHandlerKeys { get; set; } = new();
    }
}