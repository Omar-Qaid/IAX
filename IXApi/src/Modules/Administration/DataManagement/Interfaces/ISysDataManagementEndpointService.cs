using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Modules.Administration.DataManagement.Dtos;

namespace IAX.IXApi.Modules.Administration.DataManagement.Interfaces
{
    public interface ISysDataManagementEndpointService
    {
        Task<SysImportResult?> ImportAsync(Type entityType, Stream stream, CancellationToken cancellationToken);
        Task<Stream?> ExportAsync(Type entityType, CancellationToken cancellationToken);
        Task<bool> ExportViewAsync(Type entityType, SysExportRequest request, Stream output, CancellationToken cancellationToken);
        Task<Stream?> GenerateTemplateAsync(Type entityType, CancellationToken cancellationToken);
        IEnumerable<object> GetFields(Type entityType);
    }
}
