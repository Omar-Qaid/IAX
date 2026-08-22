using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestDto : MasterEntityDto<long>
    {
        public DateTime RequestDate { get; set; }
        public long ProcessId { get; set; }
        public long? EmployeeId { get; set; }
        public string? RequestDetails { get; set; }
        public bool IsFinished { get; set; }
        public DateTime? FinishedDate { get; set; }
        public bool IsStopped { get; set; }
        public DateTime? StoppedDate { get; set; }
        public decimal Score { get; set; }
        public decimal Progress { get; set; }
        public string? Notes { get; set; }
    }
}
