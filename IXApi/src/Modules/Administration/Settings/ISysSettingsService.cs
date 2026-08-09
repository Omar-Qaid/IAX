using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Administration.Settings
{
    public interface ISysSettingsService
    {
        Task<SysSettings> GetGlobalSettingsAsync(CancellationToken ct = default);
        Task<SysSettings> UpdateGlobalSettingsAsync(SysSettings settings, CancellationToken ct = default);
        Task<SysUserSettings> GetUserSettingsAsync(string userId, CancellationToken ct = default);
        Task<SysUserSettings> UpdateUserSettingsAsync(string userId, SysUserSettings settings, CancellationToken ct = default);
    }
}
