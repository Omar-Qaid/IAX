using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityDetail : Entity<long>
    {
        public long ProcessId { get; set; }
        public long AssignmentID { get; set; }
        public byte ControlId { get; set; }
        public long ControlDataId { get; set; }
        [StringLength(255)]
        public string ControlLabel { get; set; } =null!;
        [StringLength(255)]
        public string ControlLabelAR { get; set; } = null!;
        [StringLength(255)]
        public string ControlValue { get; set; } = null!;
        [StringLength(255)]
        public string ControlValueAR { get; set; } = null!;
        [StringLength(255)]
        public string ControlValueEN { get; set; } = null!;
        public bool UsedAsCriteria { get; set; }
        public byte SortOrder { get; set; }

    }
}



