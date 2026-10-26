using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.Settings.Entities;

namespace IAX.IXApi.Modules.Administration.Settings.Interfaces
{
    public interface ISysUserSettingsService : IBaseService<SysUserSettings>
    {
        Task<SysUserSettings> GetUserSettingsAsync(string userId, CancellationToken ct = default);
        Task<SysUserSettings> UpdateUserSettingsAsync(string userId, SysUserSettings settings, CancellationToken ct = default);
    }
}
