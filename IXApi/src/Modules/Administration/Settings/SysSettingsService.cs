using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.Settings
{
    public class SysSettingsService : ISysSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SysSettingsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SysSettings> GetGlobalSettingsAsync(CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<SysSettings>();
            var settings = await repo.GetQueryable().AsNoTracking().FirstOrDefaultAsync(ct);
            if (settings == null)
            {
                settings = new SysSettings
                {
                    AppName = "HBMC ERP",
                    DefaultLanguage = "en",
                    TimeZone = "UTC",
                    Currency = "USD",
                    DateFormat = "YYYY-MM-DD",
                    EnableAuditLog = true,
                    MaxUploadSize = 10485760,
                    PaginationSize = 10,
                    IsActive = true
                };
                await repo.AddAsync(settings, ct);
                await _unitOfWork.CompleteAsync(ct);
            }
            return settings;
        }

        public async Task<SysSettings> UpdateGlobalSettingsAsync(SysSettings settings, CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<SysSettings>();
            var existing = await repo.GetQueryable().FirstOrDefaultAsync(ct);
            if (existing == null)
            {
                settings.IsActive = true;
                await repo.AddAsync(settings, ct);
                await _unitOfWork.CompleteAsync(ct);
                return settings;
            }

            existing.AppName = settings.AppName;
            existing.DefaultLanguage = settings.DefaultLanguage;
            existing.TimeZone = settings.TimeZone;
            existing.Currency = settings.Currency;
            existing.DateFormat = settings.DateFormat;
            existing.EnableAuditLog = settings.EnableAuditLog;
            existing.MaxUploadSize = settings.MaxUploadSize;
            existing.PaginationSize = settings.PaginationSize;
            existing.IsActive = true;
            existing.LastModifiedAt = System.DateTime.Now;

            await repo.UpdateAsync(existing);
            await _unitOfWork.CompleteAsync(ct);
            return existing;
        }

        public async Task<SysUserSettings> GetUserSettingsAsync(string userId, CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<SysUserSettings>();
            var settings = await repo.GetQueryable().IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
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
                    CreatedBy= "sys",
                    OwnerAccountId= "sys",
                    LastModifiedBy = "sys",
                    LastModifiedAt =DateTime.Now,
                   
                };
                await repo.AddAsync(settings, ct);
                await _unitOfWork.CompleteAsync(ct);
            }
            return settings;
        }

        public async Task<SysUserSettings> UpdateUserSettingsAsync(string userId, SysUserSettings settings, CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<SysUserSettings>();
            var existing = await repo.GetQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (existing == null)
            {
                settings.UserId = userId;
                settings.IsActive = true;
                await repo.AddAsync(settings, ct);
                await _unitOfWork.CompleteAsync(ct);
                return settings;
            }

            existing.Theme = settings.Theme;
            existing.Language = settings.Language;
            existing.PageSize = settings.PageSize;
            existing.NotificationEnabled = settings.NotificationEnabled;
            existing.DashboardLayout = settings.DashboardLayout;
            existing.IsActive = true;
            existing.LastModifiedAt = System.DateTime.Now;

            await repo.UpdateAsync(existing);
            await _unitOfWork.CompleteAsync(ct);
            return existing;
        }
    }
}

