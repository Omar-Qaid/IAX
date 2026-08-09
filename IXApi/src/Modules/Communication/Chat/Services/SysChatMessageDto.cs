namespace IAX.IXApi.Modules.Communication.Chat.Services
{
    public class SysChatMessageDto
    {
        public long RecId { get; set; }
        public string RoomId { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
    }
}