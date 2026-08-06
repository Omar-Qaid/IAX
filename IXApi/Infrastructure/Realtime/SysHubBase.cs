using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// Shared base for all SignalR hubs in the system. Centralises the cross-cutting hub
    /// concerns — authenticated user resolution, automatic per-user group membership,
    /// presence broadcasting and custom group join/leave — so each concrete hub
    /// (realtime, chat, …) only adds its own domain methods.
    ///
    /// This is the multi-hub seam: add a new hub by deriving from this class and mapping a
    /// route in Program.cs; connection/group/presence behaviour is inherited, not copied.
    /// </summary>
    [Authorize]
    public abstract class SysHubBase : Hub
    {
        protected readonly ILogger Logger;

        protected SysHubBase(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>Logical hub name used in presence/log messages.</summary>
        protected virtual string HubName => GetType().Name;

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                Logger.LogInformation("[{Hub}] User {UserId} connected ({ConnectionId})",
                    HubName, userId, Context.ConnectionId);

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
                Logger.LogInformation("[{Hub}] User {UserId} disconnected ({ConnectionId})",
                    HubName, userId, Context.ConnectionId);

                await Clients.Others.SendAsync("ReceiveMessage",
                    SysRealtimeMessage.Create(SysRealtimeEventType.UserOffline, new { UserId = userId }));
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>Join a custom group (chat room, workflow process, dashboard channel…).</summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Logger.LogDebug("[{Hub}] {UserId} joined group {Group}", HubName, GetUserId(), groupName);
        }

        /// <summary>Leave a custom group.</summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Logger.LogDebug("[{Hub}] {UserId} left group {Group}", HubName, GetUserId(), groupName);
        }

        protected string? GetUserId() =>
            Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
