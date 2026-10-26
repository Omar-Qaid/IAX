using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.Settings.Entities;

namespace IAX.IXApi.Modules.Administration.Settings.Interfaces
{
    public interface ISysSettingsService : IBaseService<SysSettings>
    {
        Task<SysSettings> GetGlobalSettingsAsync(CancellationToken ct = default);
        Task<SysSettings> UpdateGlobalSettingsAsync(SysSettings settings, CancellationToken ct = default);
        Task<SysUserSettings> GetUserSettingsAsync(string userId, CancellationToken ct = default);
        Task<SysUserSettings> UpdateUserSettingsAsync(string userId, SysUserSettings settings, CancellationToken ct = default);
    }
}
