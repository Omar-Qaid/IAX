using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications
{
    public class SysNotificationPreferenceController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public SysNotificationPreferenceController(ApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Gets the current user's preferences for all categories.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> GetMyPreferences(CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            var prefs = await _db.Set<SysNotificationPreference>()
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => new SysNotificationPreferenceDto
                {
                    Category = p.Category,
                    EnableInApp = p.EnableInApp,
                    EnableEmail = p.EnableEmail,
                    EnableSms = p.EnableSms,
                    EnablePush = p.EnablePush
                })
                .ToListAsync(ct);

            // Seed defaults if no preferences configured yet
            if (!prefs.Any())
            {
                var defaultCategories = new[] { "Workflow Notifications", "Finance Notifications", "HR Notifications", "System Notifications" };
                var list = defaultCategories.Select(c => new SysNotificationPreferenceDto
                {
                    Category = c,
                    EnableInApp = true,
                    EnableEmail = true,
                    EnableSms = false,
                    EnablePush = true
                }).ToList();

                return Ok(APIResponse<IEnumerable<SysNotificationPreferenceDto>>.Ok(list));
            }

            return Ok(APIResponse<IEnumerable<SysNotificationPreferenceDto>>.Ok(prefs));
        }

        /// <summary>
        /// Saves/updates the current user's channel preferences.
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<APIResponse<bool>>> SavePreferences(
            [FromBody] IEnumerable<SysNotificationPreferenceDto> dtos,
            CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();

            var existingPrefs = await _db.Set<SysNotificationPreference>()
                .Where(p => p.UserId == userId)
                .ToListAsync(ct);

            foreach (var dto in dtos)
            {
                var pref = existingPrefs.FirstOrDefault(p => p.Category == dto.Category);
                if (pref == null)
                {
                    pref = new SysNotificationPreference
                    {
                        UserId = userId,
                        Category = dto.Category
                    };
                    _db.Set<SysNotificationPreference>().Add(pref);
                }

                pref.EnableInApp = dto.EnableInApp;
                pref.EnableEmail = dto.EnableEmail;
                pref.EnableSms = dto.EnableSms;
                pref.EnablePush = dto.EnablePush;
            }

            await _db.SaveChangesAsync(ct);
            return Ok(APIResponse<bool>.Ok(true, "Preferences saved"));
        }
    }
}