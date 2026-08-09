using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Communication.Persistence;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Infrastructure.Realtime;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Services
{
    /// <summary>
    /// Central notification service implementation.
    /// Handles recipient resolution, notification persistence, template rendering,
    /// and real-time push via the generic ISysRealtimeManager.
    /// </summary>
    public class SysNotificationService : ISysNotificationService
    {
        private readonly ICommunicationDataContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly ISysRealtimeManager _realtime;
        private readonly IEnumerable<Channels.ISysNotificationChannelSender> _channelSenders;
        private readonly ILogger<SysNotificationService> _logger;

        public SysNotificationService(
            ICommunicationDataContext db,
            ICurrentUserService currentUser,
            ISysRealtimeManager realtime,
            IEnumerable<Channels.ISysNotificationChannelSender> channelSenders,
            ILogger<SysNotificationService> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _realtime = realtime;
            _channelSenders = channelSenders;
            _logger = logger;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Send Methods
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<SysNotificationDto> SendAsync(CreateSysNotificationDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("[SysNotification] Sending notification: {Title}", dto.Title);

            // 1. Resolve template if provided
            string title = dto.Title;
            string message = dto.Message;
            string? icon = dto.Icon;
            SysNotificationPriority priority = dto.Priority;
            string? category = dto.Category;
            SysNotificationChannel channel = dto.Channel;
            int? templateId = null;

            if (!string.IsNullOrWhiteSpace(dto.TemplateCode))
            {
                var template = await _db.Set<SysNotificationTemplate>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Code == dto.TemplateCode && !t.IsDeleted, ct);

                if (template != null)
                {
                    templateId = template.RecId;
                    title = RenderTemplate(template.Subject, dto.TemplatePlaceholders);
                    message = RenderTemplate(template.Body, dto.TemplatePlaceholders);
                    icon ??= template.Icon;
                    category ??= template.DefaultCategory;
                    priority = dto.Priority != SysNotificationPriority.Medium ? dto.Priority : template.DefaultPriority;
                    channel = dto.Channel != SysNotificationChannel.InApp ? dto.Channel : template.DefaultChannel;
                }
            }

            // 2. Resolve all target user IDs
            var recipientIds = await ResolveRecipientsAsync(dto, ct);

            if (!recipientIds.Any())
            {
                _logger.LogWarning("[SysNotification] No recipients resolved for notification: {Title}", title);
                return new SysNotificationDto { Title = title, Message = message };
            }

            // 2.5 Resolve user preferences
            var finalRecipients = new List<SysNotificationRecipient>();
            var categoryStr = category ?? "System";

            var userPrefs = await _db.Set<SysNotificationPreference>()
                .AsNoTracking()
                .Where(p => recipientIds.Contains(p.UserId) && p.Category == categoryStr)
                .ToListAsync(ct);

            foreach (var uid in recipientIds)
            {
                var pref = userPrefs.FirstOrDefault(p => p.UserId == uid);
                bool isChannelEnabled = channel switch
                {
                    SysNotificationChannel.InApp => pref?.EnableInApp ?? true,
                    SysNotificationChannel.Email => pref?.EnableEmail ?? true,
                    SysNotificationChannel.SMS => pref?.EnableSms ?? false,
                    SysNotificationChannel.Push => pref?.EnablePush ?? true,
                    _ => true
                };

                if (!isChannelEnabled)
                {
                    _logger.LogInformation("[SysNotification] Skipping channel {Channel} for user {UserId} due to preferences.", channel, uid);
                    continue;
                }

                finalRecipients.Add(new SysNotificationRecipient
                {
                    UserId = uid,
                    IsRead = false,
                    DeliveryStatus = SysDeliveryStatus.Pending,
                });
            }

            if (!finalRecipients.Any())
            {
                _logger.LogWarning("[SysNotification] No active recipients left after preference check: {Title}", title);
                return new SysNotificationDto { Title = title, Message = message };
            }

            // 3. Create the notification entity
            var notification = new SysNotification
            {
                TenantId = null,
                EntityId = dto.EntityId,
                EntityType = dto.EntityType,
                ReferenceNumber = dto.ReferenceNumber,
                Title = title,
                Message = message,
                Description = dto.Description,
                Icon = icon,
                ImageUrl = dto.ImageUrl,
                Url = dto.Url,
                Priority = priority,
                Category = categoryStr,
                Channel = channel,
                Status = SysNotificationStatus.Sent,
                ExpiryDate = dto.ExpiryDate,
                TemplateId = templateId,
                IsActive = true,
                Recipients = finalRecipients
            };

            _db.Set<SysNotification>().Add(notification);
            await _db.SaveChangesAsync(ct);

            // 4. Send through the strategy channel sender and log audit trial
            var sender = _channelSenders.FirstOrDefault(s => s.Channel == channel);
            if (sender != null)
            {
                foreach (var rec in finalRecipients)
                {
                    var result = await sender.SendAsync(notification, rec, ct);

                    // Update delivery status
                    rec.DeliveryStatus = result.IsSuccess ? SysDeliveryStatus.Delivered : SysDeliveryStatus.Failed;
                    rec.DeliveredDate = DateTime.UtcNow;

                    // Log audit trial
                    var auditLog = new SysNotificationAuditLog
                    {
                        NotificationId = notification.RecId,
                        UserId = rec.UserId,
                        Channel = channel,
                        DeliveryStatus = rec.DeliveryStatus,
                        ResponsePayload = result.Response,
                        ErrorMessage = result.ErrorMessage,
                        Timestamp = DateTime.UtcNow
                    };
                    _db.SysNotificationAuditLogs.Add(auditLog);
                }
                
                await _db.SaveChangesAsync(ct);
            }
            else
            {
                _logger.LogWarning("[SysNotification] Strategy sender not found for channel {Channel}", channel);
            }

            // 5. Update unread counts and real-time alerts
            try
            {
                var activeUserIds = finalRecipients.Select(r => r.UserId).ToList();

                foreach (var uid in activeUserIds)
                {
                    var unreadCount = await GetUnreadCountAsync(uid, ct);
                    await _realtime.SendToUserAsync(uid, SysRealtimeMessage.UnreadCount(unreadCount));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SysNotification] Failed to update realtime unread counts");
            }

            _logger.LogInformation("[SysNotification] Notification sent to {Count} recipients: {Title}",
                finalRecipients.Count, title);

            return MapToDto(notification, null);
        }

        public async Task<SysNotificationDto> SendToUserAsync(
            string userId, string title, string message,
            string? url = null, string? icon = null, string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            SysNotificationChannel channel = SysNotificationChannel.InApp,
            string? entityType = null, string? entityId = null,
            CancellationToken ct = default)
        {
            return await SendAsync(new CreateSysNotificationDto
            {
                Title = title, Message = message, Url = url, Icon = icon,
                Category = category, Priority = priority, Channel = channel,
                EntityType = entityType, EntityId = entityId,
                UserIds = new List<string> { userId }
            }, ct);
        }

        public async Task<SysNotificationDto> SendToUsersAsync(
            IEnumerable<string> userIds, string title, string message,
            string? url = null, string? icon = null, string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            SysNotificationChannel channel = SysNotificationChannel.InApp,
            string? entityType = null, string? entityId = null,
            CancellationToken ct = default)
        {
            return await SendAsync(new CreateSysNotificationDto
            {
                Title = title, Message = message, Url = url, Icon = icon,
                Category = category, Priority = priority, Channel = channel,
                EntityType = entityType, EntityId = entityId,
                UserIds = userIds.ToList()
            }, ct);
        }

        public async Task<SysNotificationDto> SendToRoleAsync(
            string roleName, string title, string message,
            string? url = null, string? icon = null, string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            CancellationToken ct = default)
        {
            return await SendAsync(new CreateSysNotificationDto
            {
                Title = title, Message = message, Url = url, Icon = icon,
                Category = category, Priority = priority,
                RoleNames = new List<string> { roleName }
            }, ct);
        }

        public async Task<SysNotificationDto> SendToDepartmentAsync(
            long departmentId, string title, string message,
            string? url = null, string? icon = null, string? category = null,
            SysNotificationPriority priority = SysNotificationPriority.Medium,
            CancellationToken ct = default)
        {
            return await SendAsync(new CreateSysNotificationDto
            {
                Title = title, Message = message, Url = url, Icon = icon,
                Category = category, Priority = priority,
                DepartmentIds = new List<long> { departmentId }
            }, ct);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Template-based
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<SysNotificationDto> SendFromTemplateAsync(
            string templateCode,
            Dictionary<string, string> placeholders,
            IEnumerable<string> userIds,
            string? url = null, string? category = null,
            SysNotificationPriority? priorityOverride = null,
            CancellationToken ct = default)
        {
            return await SendAsync(new CreateSysNotificationDto
            {
                Title = string.Empty, Message = string.Empty,
                TemplateCode = templateCode,
                TemplatePlaceholders = placeholders,
                UserIds = userIds.ToList(),
                Url = url, Category = category,
                Priority = priorityOverride ?? SysNotificationPriority.Medium,
            }, ct);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Read Operations
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<(IEnumerable<SysNotificationDto> Items, int TotalCount)> GetUserNotificationsAsync(
            string userId, int pageNumber = 1, int pageSize = 20,
            bool? isRead = null, string? category = null,
            CancellationToken ct = default)
        {
            var query = _db.Set<SysNotificationRecipient>()
                .AsNoTracking()
                .Include(r => r.Notification)
                .Where(r => r.UserId == userId && !r.IsArchived)
                .Where(r => !r.Notification.IsDeleted);

            query = query.Where(r => r.Notification.ExpiryDate == null || r.Notification.ExpiryDate > DateTime.UtcNow);

            if (isRead.HasValue)
                query = query.Where(r => r.IsRead == isRead.Value);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(r => r.Notification.Category == category);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(r => r.Notification.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new SysNotificationDto
                {
                    RecId = r.Notification.RecId,
                    TenantId = r.Notification.TenantId,
                    EntityId = r.Notification.EntityId,
                    EntityType = r.Notification.EntityType,
                    ReferenceNumber = r.Notification.ReferenceNumber,
                    Title = r.Notification.Title,
                    Message = r.Notification.Message,
                    Description = r.Notification.Description,
                    Icon = r.Notification.Icon,
                    ImageUrl = r.Notification.ImageUrl,
                    Url = r.Notification.Url,
                    Priority = r.Notification.Priority,
                    Category = r.Notification.Category,
                    Channel = r.Notification.Channel,
                    Status = r.Notification.Status,
                    ExpiryDate = r.Notification.ExpiryDate,
                    CreatedBy = r.Notification.CreatedBy,
                    CreatedAt = r.Notification.CreatedAt,
                    IsRead = r.IsRead,
                    ReadDate = r.ReadDate,
                    IsArchived = r.IsArchived,
                })
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
        {
            return await _db.Set<SysNotificationRecipient>()
                .AsNoTracking()
                .Where(r => r.UserId == userId && !r.IsRead && !r.IsArchived)
                .Where(r => !r.Notification.IsDeleted)
                .Where(r => r.Notification.ExpiryDate == null || r.Notification.ExpiryDate > DateTime.UtcNow)
                .CountAsync(ct);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Status Mutations
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task MarkAsReadAsync(long notificationId, string userId, CancellationToken ct = default)
        {
            var recipient = await _db.Set<SysNotificationRecipient>()
                .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId, ct);

            if (recipient != null && !recipient.IsRead)
            {
                recipient.IsRead = true;
                recipient.ReadDate = DateTime.UtcNow;
                recipient.DeliveryStatus = SysDeliveryStatus.Read;
                await _db.SaveChangesAsync(ct);

                var unreadCount = await GetUnreadCountAsync(userId, ct);
                await _realtime.SendToUserAsync(userId, SysRealtimeMessage.UnreadCount(unreadCount));
            }
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
        {
            var unread = await _db.Set<SysNotificationRecipient>()
                .Where(r => r.UserId == userId && !r.IsRead && !r.IsArchived)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            foreach (var r in unread)
            {
                r.IsRead = true;
                r.ReadDate = now;
                r.DeliveryStatus = SysDeliveryStatus.Read;
            }

            await _db.SaveChangesAsync(ct);
            await _realtime.SendToUserAsync(userId, SysRealtimeMessage.UnreadCount(0));
        }

        public async Task ArchiveAsync(long notificationId, string userId, CancellationToken ct = default)
        {
            var recipient = await _db.Set<SysNotificationRecipient>()
                .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId, ct);

            if (recipient != null)
            {
                recipient.IsArchived = true;
                recipient.IsRead = true;
                recipient.ReadDate ??= DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);

                var unreadCount = await GetUnreadCountAsync(userId, ct);
                await _realtime.SendToUserAsync(userId, SysRealtimeMessage.UnreadCount(unreadCount));
            }
        }

        public async Task DeleteAsync(long notificationId, string userId, CancellationToken ct = default)
        {
            var recipient = await _db.Set<SysNotificationRecipient>()
                .FirstOrDefaultAsync(r => r.NotificationId == notificationId && r.UserId == userId, ct);

            if (recipient != null)
            {
                _db.Set<SysNotificationRecipient>().Remove(recipient);
                await _db.SaveChangesAsync(ct);

                var unreadCount = await GetUnreadCountAsync(userId, ct);
                await _realtime.SendToUserAsync(userId, SysRealtimeMessage.UnreadCount(unreadCount));
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Private Helpers
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private async Task<List<string>> ResolveRecipientsAsync(CreateSysNotificationDto dto, CancellationToken ct)
        {
            var userIds = new HashSet<string>();

            if (dto.UserIds?.Any() == true)
                foreach (var id in dto.UserIds) userIds.Add(id);

            if (dto.RoleNames?.Any() == true)
            {
                var roleUserIds = await _db.UserRoles.AsNoTracking()
                    .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                    .Where(x => x.Name != null && dto.RoleNames.Contains(x.Name))
                    .Select(x => x.UserId)
                    .ToListAsync(ct);
                foreach (var id in roleUserIds) userIds.Add(id);
            }

            if (dto.DepartmentIds?.Any() == true)
            {
                var deptUserIds = await _db.HcmWorkers.AsNoTracking()
                    .Where(e => dto.DepartmentIds.Contains((short)e.DepartmentId))
                    .Join(_db.AspNetUser, e => e.RecId, u => u.OrgEntityId, (e, u) => u.Id)
                    .ToListAsync(ct);
                foreach (var id in deptUserIds) userIds.Add(id);
            }

            if (dto.GroupIds?.Any() == true)
            {
                var groupUserIds = await _db.OrgEmployeeGroupDetails.AsNoTracking()
                    .Where(gd => dto.GroupIds.Contains(gd.UserGroupID))
                    .Select(gd => gd.UserID)
                    .ToListAsync(ct);
                foreach (var id in groupUserIds) userIds.Add(id);
            }

            return userIds.ToList();
        }

        private static string RenderTemplate(string template, Dictionary<string, string>? placeholders)
        {
            if (string.IsNullOrWhiteSpace(template) || placeholders == null)
                return template ?? string.Empty;

            var result = template;
            foreach (var kvp in placeholders)
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            return result;
        }

        private static SysNotificationDto MapToDto(SysNotification entity, string? currentUserId)
        {
            var recipientEntry = currentUserId != null
                ? entity.Recipients?.FirstOrDefault(r => r.UserId == currentUserId)
                : null;

            return new SysNotificationDto
            {
                RecId = entity.RecId,
                TenantId = entity.TenantId,
                EntityId = entity.EntityId,
                EntityType = entity.EntityType,
                ReferenceNumber = entity.ReferenceNumber,
                Title = entity.Title,
                Message = entity.Message,
                Description = entity.Description,
                Icon = entity.Icon,
                ImageUrl = entity.ImageUrl,
                Url = entity.Url,
                Priority = entity.Priority,
                Category = entity.Category,
                Channel = entity.Channel,
                Status = entity.Status,
                ExpiryDate = entity.ExpiryDate,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                IsRead = recipientEntry?.IsRead ?? false,
                ReadDate = recipientEntry?.ReadDate,
                IsArchived = recipientEntry?.IsArchived ?? false,
            };
        }
    }
}



