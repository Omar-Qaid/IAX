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

        public bool AlertingBySystem { get; set; }
        public bool AlertingByEmail { get; set; }
        public bool AlertingBySms { get; set; }
        public bool AlertingByWhatsApp { get; set; }

        public bool ShowPreviousSteps { get; set; }
        public bool ShowPreviousDocs { get; set; }
        public bool MandatoryDocs { get; set; }

        public bool AutoPassEnabled { get; set; }
        public byte AutoPassingHrs { get; set; }

        public string? ExtendedProperties { get; set; }
    }
}

