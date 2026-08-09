using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime;

public sealed class SysRealtimeManager(
    IHubContext<SysRealtimeHub> hubContext,
    ILogger<SysRealtimeManager> logger) : ISysRealtimeManager
{
    public Task SendToUserAsync(string userId, SysRealtimeMessage message) =>
        hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveMessage", message);

    public Task SendToGroupAsync(string groupName, SysRealtimeMessage message) =>
        hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);

    public Task SendToUsersAsync(IEnumerable<string> userIds, SysRealtimeMessage message) =>
        Task.WhenAll(userIds.Select(id => SendToUserAsync(id, message)));

    public Task SendToGroupsAsync(IEnumerable<string> groupNames, SysRealtimeMessage message) =>
        Task.WhenAll(groupNames.Select(group => SendToGroupAsync(group, message)));

    public Task BroadcastAsync(SysRealtimeMessage message) =>
        hubContext.Clients.All.SendAsync("ReceiveMessage", message);

    public Task SendAsync(SysRealtimeChannel channel, SysRealtimeMessage message) => channel.Type switch
    {
        SysRealtimeChannelType.User when !string.IsNullOrEmpty(channel.Target) => SendToUserAsync(channel.Target, message),
        SysRealtimeChannelType.Group when !string.IsNullOrEmpty(channel.Target) => SendToGroupAsync(channel.Target, message),
        SysRealtimeChannelType.Role when !string.IsNullOrEmpty(channel.Target) => SendToGroupAsync($"role_{channel.Target}", message),
        SysRealtimeChannelType.Broadcast => BroadcastAsync(message),
        SysRealtimeChannelType.Others => BroadcastWithWarningAsync(message),
        _ => Task.CompletedTask
    };

    public Task SendAsync(IEnumerable<SysRealtimeChannel> channels, SysRealtimeMessage message) =>
        Task.WhenAll(channels.Select(channel => SendAsync(channel, message)));

    public Task AddToGroupAsync(string userId, string groupName)
    {
        logger.LogInformation("[Realtime] AddToGroup requested for user {UserId} -> group {Group}", userId, groupName);
        return Task.CompletedTask;
    }

    public Task RemoveFromGroupAsync(string userId, string groupName)
    {
        logger.LogInformation("[Realtime] RemoveFromGroup requested for user {UserId} -> group {Group}", userId, groupName);
        return Task.CompletedTask;
    }

    private Task BroadcastWithWarningAsync(SysRealtimeMessage message)
    {
        logger.LogWarning("[Realtime] 'Others' used outside a hub context; broadcasting instead");
        return BroadcastAsync(message);
    }
}
