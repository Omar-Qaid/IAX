using System.Text.Json.Serialization;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications
{
    public class SysNotificationTemplateDto
    {
        public int RecId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameAR { get; set; }
        public string Subject { get; set; } = null!;
        public string? SubjectAR { get; set; }
        public string Body { get; set; } = null!;
        public string? BodyAR { get; set; }
        public string? Variables { get; set; }
        public string? Icon { get; set; }
        public SysNotificationPriority DefaultPriority { get; set; }
        public string? DefaultCategory { get; set; }
        public SysNotificationChannel DefaultChannel { get; set; }
        public string Language { get; set; } = "all";
        public bool IsActive { get; set; }
    }
}