using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime;

public sealed class SysRealtimeHub(ILogger<SysRealtimeHub> logger) : SysHubBase(logger)
{
    protected override string HubName => "RealtimeHub";

    public Task SendTyping(string groupName) => Clients.OthersInGroup(groupName).SendAsync(
        "ReceiveMessage",
        SysRealtimeMessage.Create(SysRealtimeEventType.ChatTyping, new { UserId = GetUserId() }, GetUserId()),
        Context.ConnectionAborted);

    public Task SendToGroup(string groupName, SysRealtimeMessage message)
    {
        message.SenderId ??= GetUserId();
        message.Timestamp = DateTime.UtcNow;
        return Clients.Group(groupName).SendAsync("ReceiveMessage", message, Context.ConnectionAborted);
    }
}
