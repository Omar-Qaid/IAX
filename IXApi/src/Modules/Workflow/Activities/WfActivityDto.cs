using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Communication.Notifications;

namespace IAX.IXApi.Modules.Workflow.Activities
{
public class WfActivityDto : WfMasterEntityDto<long>
    {
        public byte ActivityTypeId { get; set; }
        public long StepId { get; set; }
        public long PerformerId { get; set; }
        public decimal Score { get; set; }

        public int? SysNotificationTemplateId { get; set; }
        public virtual SysNotificationTemplateDto? SysNotificationTemplate { get; set; }

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

