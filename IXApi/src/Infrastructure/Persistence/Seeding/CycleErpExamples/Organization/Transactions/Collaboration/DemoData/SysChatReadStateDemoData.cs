using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class SysChatReadStateDemoData
{
    internal static SysChatReadState Create(string userId, string room, DateTime date) =>
        new SysChatReadState { RoomId = room, UserId = userId, LastReadAt = date.AddMinutes(5),
                DataAreaId = "dat", CreatedBy = userId, OwnerAccountId = userId };
}
