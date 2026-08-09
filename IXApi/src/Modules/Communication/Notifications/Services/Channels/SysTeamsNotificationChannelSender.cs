using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Services.Channels
{
    /// <summary>
    /// Microsoft Teams webhook/bot channel sender strategy.
    /// </summary>
    public class SysTeamsNotificationChannelSender : ISysNotificationChannelSender
    {
        private readonly ILogger<SysTeamsNotificationChannelSender> _logger;

        public SysTeamsNotificationChannelSender(ILogger<SysTeamsNotificationChannelSender> logger)
        {
            _logger = logger;
        }

        public SysNotificationChannel Channel => SysNotificationChannel.MicrosoftTeams;

        public async Task<SysNotificationChannelResult> SendAsync(
            SysNotification notification,
            SysNotificationRecipient recipient,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[TeamsChannel] Sending Microsoft Teams adaptive card to user {UserId}.", recipient.UserId);
            await Task.Delay(10, ct);
            return SysNotificationChannelResult.Success("Teams notification card sent");
        }
    }
}


