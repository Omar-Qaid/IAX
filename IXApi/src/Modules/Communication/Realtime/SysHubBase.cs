using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IAX.IXApi.Infrastructure.Realtime;

[Authorize]
public abstract class SysHubBase(ILogger logger) : Hub
{
    protected readonly ILogger Logger = logger;
    protected virtual string HubName => GetType().Name;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            Logger.LogInformation("[{Hub}] User {UserId} connected ({ConnectionId})", HubName, userId, Context.ConnectionId);
            await Clients.Others.SendAsync("ReceiveMessage",
                SysRealtimeMessage.Create(SysRealtimeEventType.UserOnline, new { UserId = userId }));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            Logger.LogInformation("[{Hub}] User {UserId} disconnected ({ConnectionId})", HubName, userId, Context.ConnectionId);
            await Clients.Others.SendAsync("ReceiveMessage",
                SysRealtimeMessage.Create(SysRealtimeEventType.UserOffline, new { UserId = userId }));
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task JoinGroup(string groupName) => Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    public Task LeaveGroup(string groupName) => Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

    protected string? GetUserId() =>
        Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
