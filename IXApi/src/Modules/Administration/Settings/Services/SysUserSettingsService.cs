using System;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.Settings.Entities;
using IAX.IXApi.Modules.Administration.Settings.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.Settings.Services
{
    public class SysUserSettingsService : BaseService<SysUserSettings>, ISysUserSettingsService
    {
        public SysUserSettingsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        public async Task<SysUserSettings> GetUserSettingsAsync(string userId, CancellationToken ct = default)
        {
            var settings = await _repository.GetQueryable().IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (settings == null)
            {
                settings = new SysUserSettings
                {
                    UserId = userId,
                    Theme = "light",
                    Language = "en",
                    PageSize = 10,
                    NotificationEnabled = true,
                    DashboardLayout = "default",
                    IsActive = true,
                    CreatedBy = "sys",
                    OwnerAccountId = "sys",
                    LastModifiedBy = "sys",
                    LastModifiedAt = DateTime.Now
                };
                await _repository.AddAsync(settings, ct);
                await _unitOfWork.CompleteAsync(ct);
            }
            return settings;
        }

        public async Task<SysUserSettings> UpdateUserSettingsAsync(string userId, SysUserSettings settings, CancellationToken ct = default)
        {
            var existing = await _repository.GetQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (existing == null)
            {
                settings.UserId = userId;
                settings.IsActive = true;
                await _repository.AddAsync(settings, ct);
                await _unitOfWork.CompleteAsync(ct);
                return settings;
            }

            existing.Theme = settings.Theme;
            existing.Language = settings.Language;
            existing.PageSize = settings.PageSize;
            existing.NotificationEnabled = settings.NotificationEnabled;
            existing.DashboardLayout = settings.DashboardLayout;
            existing.IsActive = true;
            existing.LastModifiedAt = DateTime.Now;

            await _repository.UpdateAsync(existing);
            await _unitOfWork.CompleteAsync(ct);
            return existing;
        }
    }
}
