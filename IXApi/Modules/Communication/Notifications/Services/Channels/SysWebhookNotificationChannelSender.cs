using System.Net.Http.Json;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    /// <summary>
    /// Generic outbound Webhook channel sender strategy.
    /// Delivers a notification by HTTP POSTing a JSON envelope to a target URL.
    ///
    /// The target URL is taken from <see cref="SysNotification.Url"/> when it is an
    /// absolute http(s) URL. This keeps the channel fully generic — any module can
    /// raise a webhook notification simply by setting the destination on the payload,
    /// with no coupling to business entities.
    /// </summary>
    [ScopedService]
    public class SysWebhookNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SysWebhookNotificationChannelSender> _logger;

        public SysWebhookNotificationChannelSender(
            IHttpClientFactory httpClientFactory,
            ILogger<SysWebhookNotificationChannelSender> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.Webhook;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            if (!Uri.TryCreate(notification.Url, UriKind.Absolute, out var target)
                || (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps))
            {
                _logger.LogWarning(
                    "[WebhookChannel] No absolute http(s) target URL on notification {NotificationId}; skipping.",
                    notification.RecId);
                return SysNotificationChannelResult.Failure("No valid webhook target URL configured.");
            }

            var envelope = new
            {
                notification.RecId,
                notification.Title,
                notification.Message,
                notification.Category,
                Priority = notification.Priority.ToString(),
                notification.EntityType,
                notification.EntityId,
                notification.ReferenceNumber,
                RecipientUserId = recipient.UserId,
                Timestamp = DateTime.UtcNow,
            };

            try
            {
                var client = _httpClientFactory.CreateClient(nameof(SysWebhookNotificationChannelSender));
                client.Timeout = TimeSpan.FromSeconds(15);

                var response = await client.PostAsJsonAsync(target, envelope, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    return SysNotificationChannelResult.Failure(
                        $"Webhook returned {(int)response.StatusCode}.", body);
                }

                _logger.LogInformation("[WebhookChannel] Delivered notification {NotificationId} to {Target}.",
                    notification.RecId, target);
                return SysNotificationChannelResult.Success($"Webhook delivered ({(int)response.StatusCode}).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WebhookChannel] Failed to deliver notification {NotificationId} to {Target}.",
                    notification.RecId, target);
                return SysNotificationChannelResult.Failure(ex.Message);
            }
        }
    }
}
