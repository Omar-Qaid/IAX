using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications.Services
{
    /// <summary>
    /// Central notification service interface.
    /// Any module in the system can inject this service to send notifications.
    /// </summary>
    public interface ISysNotificationService
    {
        // ── Send Methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Creates and sends a notification using the full DTO (supports all recipient types).
        /// </summary>
        Task<SysNotificationDto> SendAsync(CreateSysNotificationDto dto, CancellationToken ct = default);

        /// <summary>
        /// Sends a notification to a single user.
        /// </summary>
        Task<SysNotificationDto> SendToUserAsync(
            string userId,
            string title,
            string message,
            string? url = null,
            string? icon = null,
            string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            SysNotificationChannel channel = SysNotificationChannel.InApp,
            string? entityType = null,
            string? entityId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Sends a notification to multiple users.
        /// </summary>
        Task<SysNotificationDto> SendToUsersAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            string? url = null,
            string? icon = null,
            string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            SysNotificationChannel channel = SysNotificationChannel.InApp,
            string? entityType = null,
            string? entityId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Sends a notification to all users in a specific role.
        /// </summary>
        Task<SysNotificationDto> SendToRoleAsync(
            string roleName,
            string title,
            string message,
            string? url = null,
            string? icon = null,
            string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            CancellationToken ct = default);

        /// <summary>
        /// Sends a notification to all employees in a department.
        /// </summary>
        Task<SysNotificationDto> SendToDepartmentAsync(
            long departmentId,
            string title,
            string message,
            string? url = null,
            string? icon = null,
            string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            CancellationToken ct = default);

        // ── Template-based ───────────────────────────────────────────────────

        /// <summary>
        /// Sends a notification generated from a template with dynamic placeholder substitution.
        /// </summary>
        Task<SysNotificationDto> SendFromTemplateAsync(
            string templateCode,
            Dictionary<string, string> placeholders,
            IEnumerable<string> userIds,
            string? url = null,
            string? category = null,
            SysNotificationPriority? priorityOverride = null,
            CancellationToken ct = default);

        // ── Read Operations ──────────────────────────────────────────────────

        /// <summary>
        /// Gets paged notifications for a specific user.
        /// </summary>
        Task<(IEnumerable<SysNotificationDto> Items, int TotalCount)> GetUserNotificationsAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 20,
            bool? isRead = null,
            string? category = null,
            CancellationToken ct = default);

        /// <summary>
        /// Gets the count of unread notifications for a user.
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

        // ── Status Mutations ─────────────────────────────────────────────────

        /// <summary>
        /// Marks a specific notification as read for a user.
        /// </summary>
        Task MarkAsReadAsync(long notificationId, string userId, CancellationToken ct = default);

        /// <summary>
        /// Marks all notifications as read for a user.
        /// </summary>
        Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Archives a notification for a user (hides from default view).
        /// </summary>
        Task ArchiveAsync(long notificationId, string userId, CancellationToken ct = default);

        /// <summary>
        /// Deletes a notification recipient entry for a user.
        /// </summary>
        Task DeleteAsync(long notificationId, string userId, CancellationToken ct = default);
    }
}
