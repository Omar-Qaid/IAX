using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityDetail : Entity<long>
    {
        public long ProcessId { get; set; }
        public long AssignmentID { get; set; }
        public byte ControlId { get; set; }
        public long ControlDataId { get; set; }
        public string? Name { get; set; }
        public string? NameAlias { get; set; }
        public string? ControlValue { get; set; }
        public byte SortOrder { get; set; }
        public string? ValueAlias { get; set; }
        public string? Value { get; set; }
        public decimal Score { get; set; } = 0;
        public decimal EarnedScore { get; set; } = 0;
    }
}
