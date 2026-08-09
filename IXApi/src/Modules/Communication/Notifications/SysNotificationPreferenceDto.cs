using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications
{
    public class SysNotificationPreferenceDto
    {
        public string Category { get; set; } = null!;
        public bool EnableInApp { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public bool EnablePush { get; set; }
    }
}