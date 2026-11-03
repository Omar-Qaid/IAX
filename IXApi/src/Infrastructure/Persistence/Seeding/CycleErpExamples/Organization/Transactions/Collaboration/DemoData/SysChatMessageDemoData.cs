using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysChatMessageDemoData
{
    internal static SysChatMessage Create(string userId, string room, string content, DateTime date) =>
        new SysChatMessage { RoomId = room, SenderId = userId, Content = content, SentAt = date,
                    DataAreaId = "dat", CreatedBy = userId, OwnerAccountId = userId };
}
