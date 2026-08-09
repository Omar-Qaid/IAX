using IAX.IXApi.Modules.Communication.Chat.Services;
using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime;

public sealed class SysChatHub(ILogger<SysChatHub> logger, ISysChatService chat) : SysHubBase(logger)
{
    protected override string HubName => "ChatHub";

    public Task JoinRoom(string roomId) => JoinGroup(ChatGroup(roomId));
    public Task LeaveRoom(string roomId) => LeaveGroup(ChatGroup(roomId));

    public async Task SendMessage(string roomId, string content)
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrWhiteSpace(content))
            await chat.SendAsync(roomId, userId, content, Context.ConnectionAborted);
    }

    public Task Typing(string roomId) => Clients.OthersInGroup(ChatGroup(roomId)).SendAsync(
        "ReceiveMessage",
        SysRealtimeMessage.Create(SysRealtimeEventType.ChatTyping, new { RoomId = roomId, UserId = GetUserId() }, GetUserId()),
        Context.ConnectionAborted);

    internal static string ChatGroup(string roomId) => $"chat_{roomId}";
}
