using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    public class SysNotificationChannelResult
    {
        public bool IsSuccess { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }

        public static SysNotificationChannelResult Success(string? response = null) =>
            new SysNotificationChannelResult { IsSuccess = true, Response = response };

        public static SysNotificationChannelResult Failure(string errorMessage, string? response = null) =>
            new SysNotificationChannelResult { IsSuccess = false, ErrorMessage = errorMessage, Response = response };
    }
}