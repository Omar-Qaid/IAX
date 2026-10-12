namespace IAX.IXApi.Modules.Communication.Notifications.Services;

public interface ISysNotificationPreferenceService
{
    Task<IReadOnlyList<SysNotificationPreferenceDto>> GetMyPreferencesAsync(string userId, CancellationToken cancellationToken = default);
    Task SavePreferencesAsync(string userId, IEnumerable<SysNotificationPreferenceDto> preferences, CancellationToken cancellationToken = default);
}
