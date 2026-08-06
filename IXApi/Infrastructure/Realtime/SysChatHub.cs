using IAX.IXApi.Modules.Communication.Chat.Services;
using Microsoft.AspNetCore.SignalR;

namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// Dedicated chat hub (multi-hub architecture — see <see cref="SysHubBase"/>).
    /// Handles room join/leave, message send (persisted + broadcast via <see cref="ISysChatService"/>)
    /// and typing indicators. Mapped at <c>/hubs/chat</c>.
    /// </summary>
    public class SysChatHub : SysHubBase
    {
        private readonly ISysChatService _chat;

        public SysChatHub(ILogger<SysChatHub> logger, ISysChatService chat) : base(logger)
        {
            _chat = chat;
        }

        protected override string HubName => "ChatHub";

        public Task JoinRoom(string roomId) => JoinGroup(ChatGroup(roomId));
        public Task LeaveRoom(string roomId) => LeaveGroup(ChatGroup(roomId));

        /// <summary>Persists the message and broadcasts it to the room (handled by the service).</summary>
        public async Task SendMessage(string roomId, string content)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(content)) return;
            await _chat.SendAsync(roomId, userId, content);
        }

        public async Task Typing(string roomId)
        {
            var userId = GetUserId();
            await Clients.OthersInGroup(ChatGroup(roomId)).SendAsync("ReceiveMessage",
                SysRealtimeMessage.Create(SysRealtimeEventType.ChatTyping, new { RoomId = roomId, UserId = userId }, userId));
        }

        internal static string ChatGroup(string roomId) => $"chat_{roomId}";
    }
}
