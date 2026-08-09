namespace IAX.IXApi.Modules.Communication.Chat.Services
{
    public interface ISysChatService
    {
        /// <summary>Persists a message and broadcasts it to the room. Returns the stored message.</summary>
        Task<SysChatMessageDto> SendAsync(string roomId, string senderId, string content, CancellationToken ct = default);

        /// <summary>Returns a page of room history, newest first.</summary>
        Task<(IReadOnlyList<SysChatMessageDto> Items, int TotalCount)> GetHistoryAsync(
            string roomId, int pageNumber = 1, int pageSize = 50, CancellationToken ct = default);

        /// <summary>
        /// Returns the user's conversations (rooms they participate in) with the last message
        /// and the count of unread messages, most-recent first.
        /// </summary>
        Task<IReadOnlyList<SysChatConversationDto>> GetConversationsAsync(string userId, CancellationToken ct = default);

        /// <summary>Marks a room as read up to now for the user.</summary>
        Task MarkReadAsync(string userId, string roomId, CancellationToken ct = default);
    }
}