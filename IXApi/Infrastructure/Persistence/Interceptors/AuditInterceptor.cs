using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Services;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Infrastructure.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;
        private readonly ISysAuditService _auditService;
        private readonly List<PendingAudit> _pendingAudits = new();
        private bool _buildingAudit;
        private bool _suppressAudit;

        public AuditInterceptor(ICurrentUserService currentUser, ISysAuditService auditService)
        {
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is null) return result;

            UpdateAuditableEntities(eventData.Context);
            ProcessSoftDeletes(eventData.Context);

            if (!_suppressAudit)
            {
                HydrateOriginalValues(eventData.Context);
                BuildPendingAuditsBeforeSave(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null) return result;

            UpdateAuditableEntities(eventData.Context);
            ProcessSoftDeletes(eventData.Context);

            if (!_suppressAudit)
            {
                await HydrateOriginalValuesAsync(eventData.Context, cancellationToken);
                BuildPendingAuditsBeforeSave(eventData.Context);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (eventData.Context is null) return result;

            if (!_suppressAudit)
            {
                PersistAuditsAfterSave(eventData.Context);
            }

            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null) return result;

            if (!_suppressAudit)
            {
                await PersistAuditsAfterSaveAsync(eventData.Context, cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            var userId = _currentUser?.GetCurrentUserId();
            var nowUtc = DateTime.UtcNow;
            var dataAreaId = _currentUser?.GetDataAreaId() ?? "dat";

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is AuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (string.IsNullOrEmpty(auditable.CreatedBy))
                            auditable.CreatedBy = userId;
                        auditable.CreatedAt = nowUtc;
                        if (string.IsNullOrEmpty(auditable.OwnerAccountId))
                            auditable.OwnerAccountId = userId;
                    }

                    auditable.LastModifiedAt = nowUtc;
                    auditable.LastModifiedBy = userId;
                }

                if (entry.State == EntityState.Added && entry.Entity is IMultiCompany multiCompany)
                {
                    if (string.IsNullOrWhiteSpace(multiCompany.DataAreaId))
                    {
                        multiCompany.DataAreaId = dataAreaId;
                    }
                }
            }
        }

        private void ProcessSoftDeletes(DbContext context)
        {
            var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted);
            foreach (var entry in entries)
            {
                var isDeletedProp = entry.Metadata.FindProperty("IsDeleted");
                if (isDeletedProp != null && isDeletedProp.ClrType == typeof(bool))
                {
                    entry.State = EntityState.Modified;
                    entry.CurrentValues["IsDeleted"] = true;
                }
            }
        }

        private void HydrateOriginalValues(DbContext context)
        {
            var candidates = context.ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Deleted)
                            && e.Entity is not SysAuditLog && e.Entity is not IAuditExempt);

            foreach (var entry in candidates)
            {
                var dbValues = entry.GetDatabaseValues();
                if (dbValues != null) entry.OriginalValues.SetValues(dbValues);
            }
        }

        private async Task HydrateOriginalValuesAsync(DbContext context, CancellationToken ct)
        {
            var candidates = context.ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Deleted)
                            && e.Entity is not SysAuditLog && e.Entity is not IAuditExempt);

            foreach (var entry in candidates)
            {
                var dbValues = await entry.GetDatabaseValuesAsync(ct);
                if (dbValues != null) entry.OriginalValues.SetValues(dbValues);
            }
        }

        private void BuildPendingAuditsBeforeSave(DbContext context)
        {
            if (_buildingAudit) return;
            _buildingAudit = true;

            try
            {
                var userId = _currentUser?.GetCurrentUserId() ?? "SYSTEM";
                var nowUtc = DateTime.UtcNow;

                var modifiedEntries = context.ChangeTracker.Entries()
                    .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                && e.Entity is not SysAuditLog
                                && e.Entity is not IAuditExempt);

                foreach (var entry in modifiedEntries)
                {
                    var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
                    var (pkProps, recordIdNow, _) = _auditService.GetPkInfo(entry);
                    var pkNamesJoined = pkProps.Count == 0 ? null : (pkProps.Count == 1 ? pkProps[0].Name : string.Join("|", pkProps.Select(p => p.Name)));

                    if (entry.State == EntityState.Modified)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            if (!prop.IsModified || _auditService.ShouldSkip(prop)) continue;
                            if (_auditService.ValuesEqual(prop.OriginalValue, prop.CurrentValue)) continue;

                            _pendingAudits.Add(new PendingAudit
                            {
                                Entry = entry,
                                PkNames = pkNamesJoined,
                                Log = new SysAuditLog
                                {
                                    TableName = tableName,
                                    RecordId = recordIdNow ?? string.Empty,
                                    ColumnName = prop.Metadata.Name,
                                    OldValue = _auditService.SafeToString(prop.OriginalValue),
                                    NewValue = _auditService.SafeToString(prop.CurrentValue),
                                    Operation = "Update",
                                    ChangedBy = userId,
                                    ChangedOn = nowUtc
                                }
                            });
                        }
                    }
                    else if (entry.State == EntityState.Added)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            if (_auditService.ShouldSkip(prop)) continue;
                            _pendingAudits.Add(new PendingAudit
                            {
                                Entry = entry,
                                PkNames = pkNamesJoined,
                                Log = new SysAuditLog
                                {
                                    TableName = tableName,
                                    RecordId = recordIdNow ?? string.Empty,
                                    ColumnName = prop.Metadata.Name,
                                    NewValue = _auditService.SafeToString(prop.CurrentValue),
                                    Operation = "Insert",
                                    ChangedBy = userId,
                                    ChangedOn = nowUtc
                                }
                            });
                        }
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            if (_auditService.ShouldSkip(prop)) continue;
                            _pendingAudits.Add(new PendingAudit
                            {
                                Entry = entry,
                                PkNames = pkNamesJoined,
                                Log = new SysAuditLog
                                {
                                    TableName = tableName,
                                    RecordId = recordIdNow ?? string.Empty,
                                    ColumnName = prop.Metadata.Name,
                                    OldValue = _auditService.SafeToString(prop.OriginalValue),
                                    Operation = "Delete",
                                    ChangedBy = userId,
                                    ChangedOn = nowUtc
                                }
                            });
                        }
                    }
                }
            }
            finally
            {
                _buildingAudit = false;
            }
        }

        private void PersistAuditsAfterSave(DbContext context)
        {
            if (_pendingAudits.Count == 0) return;

            foreach (var pa in _pendingAudits)
            {
                if (string.IsNullOrEmpty(pa.Log.RecordId) && pa.Entry is not null)
                {
                    var (_, id, _) = _auditService.GetPkInfo(pa.Entry);
                    pa.Log.RecordId = id ?? string.Empty;
                }
            }

            _suppressAudit = true;
            try
            {
                context.Set<SysAuditLog>().AddRange(_pendingAudits.Select(p => p.Log));
                context.SaveChanges();
            }
            finally
            {
                _suppressAudit = false;
                _pendingAudits.Clear();
            }
        }

        private async Task PersistAuditsAfterSaveAsync(DbContext context, CancellationToken ct)
        {
            if (_pendingAudits.Count == 0) return;

            foreach (var pa in _pendingAudits)
            {
                if (string.IsNullOrEmpty(pa.Log.RecordId) && pa.Entry is not null)
                {
                    var (_, id, _) = _auditService.GetPkInfo(pa.Entry);
                    pa.Log.RecordId = id ?? string.Empty;
                }
            }

            _suppressAudit = true;
            try
            {
                context.Set<SysAuditLog>().AddRange(_pendingAudits.Select(p => p.Log));
                await context.SaveChangesAsync(ct);
            }
            finally
            {
                _suppressAudit = false;
                _pendingAudits.Clear();
            }
        }
    }
}
