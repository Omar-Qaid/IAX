namespace IAX.IXApi.Modules.Communication.Chat.Dtos;

public class SysChatConversationDto
{
    public string RoomId { get; set; } = null!;
    public string? LastMessage { get; set; }
    public string? LastSenderId { get; set; }
    public DateTime? LastSentAt { get; set; }
    public int UnreadCount { get; set; }
}
