using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    public interface ISysDataManagementService
    {
        /// <summary>
        /// Imports data from an Excel stream into the database.
        /// </summary>
        Task<SysImportResult> ImportAsync<T>(Stream stream, CancellationToken cancellationToken = default) where T : class, IBaseEntity, new();

        /// <summary>
        /// Exports the entire table to an Excel stream. Kept for backward compatibility —
        /// new callers should prefer the overload that takes <see cref="SysExportRequest"/>.
        /// </summary>
        Task<Stream> ExportAsync<T>(CancellationToken cancellationToken = default) where T : class, IBaseEntity, new();

        /// <summary>
        /// Streams a filtered / sorted / column-projected export directly to <paramref name="output"/>.
        /// No full in-memory materialisation; rows are pulled from EF in chunks and written as they arrive.
        /// </summary>
        Task ExportAsync<T>(SysExportRequest request, Stream output, CancellationToken cancellationToken = default)
            where T : class, IBaseEntity, new();

        /// <summary>
        /// Generates an empty Excel template stream for the entity.
        /// </summary>
        Task<Stream> GenerateTemplateAsync<T>(CancellationToken cancellationToken = default) where T : class, IBaseEntity, new();
    }
}
