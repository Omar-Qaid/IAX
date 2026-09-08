using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Steps;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Workflow.Activities
{
public class WfActivity : WfMasterEntity<long>
    {
        public byte ActivityTypeId { get; set; }
        [ForeignKey(nameof(ActivityTypeId))]
        public virtual WfActivityType ActivityType { get; set; } = null!;

        public long StepId { get; set; }
        [ForeignKey(nameof(StepId))]
        public virtual WfStep Step { get; set; } = null!;

        public long PerformerId { get; set; }
        [ForeignKey(nameof(PerformerId))]
        public virtual WfPerformer Performer { get; set; } = null!;

        public decimal Score { get; set; }

        public int? SysNotificationTemplateId { get; set; }
        [ForeignKey(nameof(SysNotificationTemplateId))]
        public virtual SysNotificationTemplate? SysNotificationTemplate { get; set; }

        public bool IsSystemNotificationEnabled { get; set; }
        public bool IsEmailNotificationEnabled { get; set; }
        public bool IsSmsNotificationEnabled { get; set; }
        public bool IsWhatsAppNotificationEnabled { get; set; }
        public bool MandatoryDocuments { get; set; }
        public bool IsAutoPassEnabled { get; set; }
        public byte AutoPassAfterHours { get; set; }
        public bool CanViewPreviousSteps { get; set; }
        public bool CanViewPreviousDocuments { get; set; }
        public string? ExtendedProperties { get; set; }

    }
}


