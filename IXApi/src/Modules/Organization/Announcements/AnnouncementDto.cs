using System;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class AnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool Activated { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public long CreatedBy { get; set; }
        public string? PhotoURL { get; set; }
        public string? ContentAR { get; set; }
        public string? TitleAR { get; set; }
    }
}
