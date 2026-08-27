using IAX.IXApi.Shared.Domain.Entities;
using System;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class Announcement: MasterEntity<int>
    {
        public DateTime ExpiryDate { get; set; }
        public string? PhotoURL { get; set; }
    }
}

