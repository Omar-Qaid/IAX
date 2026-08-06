using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public static class AutoCodeExtensions
    {
        /// <summary>
        /// Assigns Code automatically using the sequence registered for <paramref name="entityName"/>
        /// when the entity's Code is empty. Safe to call from any service's OnBeforeAddAsync.
        /// </summary>
        public static async Task EnsureCodeAsync<T>(
            this ISysNumberSequenceService sequences,
            T entity,
            string? entityName = null,
            string? tenantId = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (entity is not ICode baseEntity) return;
            if (!string.IsNullOrWhiteSpace(baseEntity.Code)) return;

            var key = entityName ?? typeof(T).Name;
            var result = await sequences.NextAsync(key, tenantId, cancellationToken);
            baseEntity.Code = result.Code;
        }
    }
}
