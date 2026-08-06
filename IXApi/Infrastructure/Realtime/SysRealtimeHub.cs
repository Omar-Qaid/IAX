using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// General-purpose real-time hub for ALL non-chat server-to-client communication:
    /// notifications, unread counts, workflow events, job progress, dashboards, system alerts.
    ///
    /// Connection, per-user group membership and presence are inherited from
    /// <see cref="SysHubBase"/>. All server-to-client messages use the single "ReceiveMessage"
    /// method with a <see cref="SysRealtimeMessage"/> envelope; the client discriminates by
    /// EventType. Mapped at <c>/hubs/realtime</c>.
    /// </summary>
    public class SysRealtimeHub : SysHubBase
    {
        public SysRealtimeHub(ILogger<SysRealtimeHub> logger) : base(logger) { }

        protected override string HubName => "RealtimeHub";

        /// <summary>Sends a typing indicator to a group (e.g. workflow room).</summary>
        public async Task SendTyping(string groupName)
        {
            var userId = GetUserId();
            await Clients.OthersInGroup(groupName).SendAsync("ReceiveMessage",
                SysRealtimeMessage.Create(SysRealtimeEventType.ChatTyping, new { UserId = userId }, userId));
        }

        /// <summary>Sends a message to a group (client-to-server-to-clients).</summary>
        public async Task SendToGroup(string groupName, SysRealtimeMessage message)
        {
            message.SenderId ??= GetUserId();
            message.Timestamp = DateTime.UtcNow;
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }
    }
}
