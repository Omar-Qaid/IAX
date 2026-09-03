using IAX.IXApi.Modules.Communication.Chat.Services;
using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime;

public sealed class SysChatHub(ILogger<SysChatHub> logger, ISysChatService chat) : SysHubBase(logger)
{
    protected override string HubName => "ChatHub";

    public Task JoinRoom(string roomId)
    {
        var userId = GetUserId();
        return chat.CanAccessRoom(userId, roomId)
            ? JoinGroup(ChatGroup(roomId))
            : throw new HubException("Chat room not found.");
    }
    public Task LeaveRoom(string roomId) => LeaveGroup(ChatGroup(roomId));

    public async Task SendMessage(string roomId, string content)
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrWhiteSpace(content) && chat.CanAccessRoom(userId, roomId))
            await chat.SendAsync(roomId, userId, content, Context.ConnectionAborted);
    }

    public Task Typing(string roomId)
    {
        var userId = GetUserId();
        return chat.CanAccessRoom(userId, roomId)
            ? Clients.OthersInGroup(ChatGroup(roomId)).SendAsync(
                "ReceiveMessage",
                SysRealtimeMessage.Create(SysRealtimeEventType.ChatTyping, new { RoomId = roomId, UserId = userId }, userId),
                Context.ConnectionAborted)
            : throw new HubException("Chat room not found.");
    }

    internal static string ChatGroup(string roomId) => $"chat_{roomId}";
}
