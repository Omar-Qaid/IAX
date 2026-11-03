using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class AnnouncementDemoData
{
    internal static Announcement Create(string code, string name, string alias, string text, DateTime expires, bool active, string userId) =>
        new Announcement { Code = code, Name = name, NameAlias = alias,
                    Description = text, ExpiryDate = expires, IsActive = active,
                    DataAreaId = "dat", CreatedBy = userId, OwnerAccountId = userId };
}
