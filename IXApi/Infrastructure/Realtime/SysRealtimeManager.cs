using IAX.IXApi.Shared.Application.Attributes;
using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// SignalR-backed implementation of the generic real-time manager.
    /// Routes messages to the correct SignalR group/client based on channel targeting.
    /// 
    /// Naming convention for automatic groups:
    ///   - User:  "user_{userId}"
    ///   - Role:  "role_{roleName}"
    ///   - Custom groups: any string name
    /// </summary>
    public class SysRealtimeManager : ISysRealtimeManager
    {
        private readonly IHubContext<SysRealtimeHub> _hubContext;
        private readonly ILogger<SysRealtimeManager> _logger;

        public SysRealtimeManager(
            IHubContext<SysRealtimeHub> hubContext,
            ILogger<SysRealtimeManager> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        // ── Single Target ────────────────────────────────────────────────

        public async Task SendToUserAsync(string userId, SysRealtimeMessage message)
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveMessage", message);
        }

        public async Task SendToGroupAsync(string groupName, SysRealtimeMessage message)
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveMessage", message);
        }

        // ── Multi-Target ─────────────────────────────────────────────────

        public async Task SendToUsersAsync(IEnumerable<string> userIds, SysRealtimeMessage message)
        {
            var groups = userIds.Select(id => $"user_{id}").ToList();
            // Send to all user groups in parallel
            var tasks = groups.Select(g => _hubContext.Clients.Group(g).SendAsync("ReceiveMessage", message));
            await Task.WhenAll(tasks);
        }

        public async Task SendToGroupsAsync(IEnumerable<string> groupNames, SysRealtimeMessage message)
        {
            var tasks = groupNames.Select(g => _hubContext.Clients.Group(g).SendAsync("ReceiveMessage", message));
            await Task.WhenAll(tasks);
        }

        // ── Broadcast ────────────────────────────────────────────────────

        public async Task BroadcastAsync(SysRealtimeMessage message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        // ── Channel-based (generic routing) ──────────────────────────────

        public async Task SendAsync(SysRealtimeChannel channel, SysRealtimeMessage message)
        {
            switch (channel.Type)
            {
                case SysRealtimeChannelType.User:
                    if (!string.IsNullOrEmpty(channel.Target))
                        await SendToUserAsync(channel.Target, message);
                    break;

                case SysRealtimeChannelType.Group:
                    if (!string.IsNullOrEmpty(channel.Target))
                        await SendToGroupAsync(channel.Target, message);
                    break;

                case SysRealtimeChannelType.Role:
                    if (!string.IsNullOrEmpty(channel.Target))
                        await SendToGroupAsync($"role_{channel.Target}", message);
                    break;

                case SysRealtimeChannelType.Broadcast:
                    await BroadcastAsync(message);
                    break;

                case SysRealtimeChannelType.Others:
                    // "Others" routing requires a connection context — only usable from within the Hub.
                    // From outside the hub, we fall back to broadcast.
                    _logger.LogWarning("[Realtime] 'Others' channel type used outside hub context — falling back to broadcast");
                    await BroadcastAsync(message);
                    break;
            }
        }

        public async Task SendAsync(IEnumerable<SysRealtimeChannel> channels, SysRealtimeMessage message)
        {
            var tasks = channels.Select(ch => SendAsync(ch, message));
            await Task.WhenAll(tasks);
        }

        // ── Group Management ─────────────────────────────────────────────

        public async Task AddToGroupAsync(string userId, string groupName)
        {
            // Note: SignalR group management requires a connectionId.
            // This method is primarily for documentation/interface completeness.
            // Actual group joining happens in the Hub's OnConnectedAsync or via client-invoked methods.
            _logger.LogInformation("[Realtime] AddToGroup requested for user {UserId} -> group {Group}", userId, groupName);
            await Task.CompletedTask;
        }

        public async Task RemoveFromGroupAsync(string userId, string groupName)
        {
            _logger.LogInformation("[Realtime] RemoveFromGroup requested for user {UserId} -> group {Group}", userId, groupName);
            await Task.CompletedTask;
        }
    }
}

