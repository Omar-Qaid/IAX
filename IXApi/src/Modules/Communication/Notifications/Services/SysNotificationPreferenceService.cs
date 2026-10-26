using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Services;

public sealed class SysNotificationPreferenceService : BaseService<SysNotificationPreference>, ISysNotificationPreferenceService
{
    public SysNotificationPreferenceService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }

    public async Task<IReadOnlyList<SysNotificationPreferenceDto>> GetMyPreferencesAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _unitOfWork.Context.Set<SysNotificationPreference>()
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
        var existing = await _unitOfWork.Context.Set<SysNotificationPreference>()
            .Where(preference => preference.UserId == userId).ToListAsync(cancellationToken);

        foreach (var dto in preferences)
        {
            var preference = existing.FirstOrDefault(item => item.Category == dto.Category);
            if (preference == null)
            {
                preference = new SysNotificationPreference { UserId = userId, Category = dto.Category };
                _unitOfWork.Context.Set<SysNotificationPreference>().Add(preference);
            }
            dto.Adapt(preference);
        }

        await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
    }
}
