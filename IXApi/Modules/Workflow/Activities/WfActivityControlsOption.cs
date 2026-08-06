using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    /// <summary>
    /// A single selectable option for a closed-list Activity control (Drop Down List
    /// "fill manually", CheckBoxList, RadioButtonList). Mirrors the
    /// <see cref="WfActivityControlsValidation"/> pattern (one row per option).
    /// </summary>
    public class WfActivityControlsOption : Entity<long>
    {
        public long ActivityControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityControlId))]
        public virtual WfActivityControl ActivityControl { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Value { get; set; } = null!;   // value submitted when selected
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string NameEn { get; set; } = null!;   // English display label
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string NameAr { get; set; } = null!;   // Arabic display label
        public int SortOrder { get; set; }
    }
}

