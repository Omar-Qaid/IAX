using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Steps;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Activities
{
public class WfActivity : WfMasterEntity<long>
    {
        public byte ActivityTypeId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityTypeId))]
        public virtual WfActivityType ActivityType { get; set; } = null!;

        public long StepId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(StepId))]
        public virtual WfStep Step { get; set; } = null!;

        public long PerformerId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(PerformerId))]
        public virtual WfPerformer Performer { get; set; } = null!;

        public decimal Score { get; set; }

        public int? SysNotificationTemplateId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(SysNotificationTemplateId))]
        public virtual SysNotificationTemplate? SysNotificationTemplate { get; set; }

        public bool AlertingBySystem { get; set; }
        public bool AlertingByEmail { get; set; }
        public bool AlertingBySms { get; set; }
        public bool AlertingByWhatsApp { get; set; }

        public bool ShowPreviousSteps { get; set; }
        public bool ShowPreviousDocs { get; set; }
        public bool MandatoryDocs { get; set; }

        /// <summary>
        /// When true, an open assignment for this activity is auto-passed (auto-finished)
        /// by the background sweep once <see cref="AutoPassingHrs"/> have elapsed since assignment.
        /// </summary>
        public bool AutoPassEnabled { get; set; }
        public byte AutoPassingHrs { get; set; }

        public string? ExtendedProperties { get; set; }

    }
}


