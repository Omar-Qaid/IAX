using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Services;

public sealed class SysNotificationPreferenceService(IUnitOfWork unitOfWork) : ISysNotificationPreferenceService
{
    public async Task<IReadOnlyList<SysNotificationPreferenceDto>> GetMyPreferencesAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var preferences = await unitOfWork.Context.Set<SysNotificationPreference>()
            .AsNoTracking().Where(preference => preference.UserId == userId)
            .ToListAsync(cancellationToken);
        if (preferences.Count > 0)
            return preferences.Adapt<List<SysNotificationPreferenceDto>>();

        return [
            new() { Category = "Workflow Notifications", EnableInApp = true, EnableEmail = true, EnablePush = true },
            new() { Category = "Finance Notifications", EnableInApp = true, EnableEmail = true, EnablePush = true },
            new() { Category = "HR Notifications", EnableInApp = true, EnableEmail = true, EnablePush = true },
            new() { Category = "System Notifications", EnableInApp = true, EnableEmail = true, EnablePush = true }
        ];
    }

    public async Task SavePreferencesAsync(string userId, IEnumerable<SysNotificationPreferenceDto> preferences,
        CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.Context.Set<SysNotificationPreference>()
            .Where(preference => preference.UserId == userId).ToListAsync(cancellationToken);

        foreach (var dto in preferences)
        {
            var preference = existing.FirstOrDefault(item => item.Category == dto.Category);
            if (preference == null)
            {
                preference = new SysNotificationPreference { UserId = userId, Category = dto.Category };
                unitOfWork.Context.Set<SysNotificationPreference>().Add(preference);
            }
            dto.Adapt(preference);
        }

        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
    }
}
