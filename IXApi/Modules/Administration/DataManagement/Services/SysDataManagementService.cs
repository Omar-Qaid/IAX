using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    [ScopedService]
    public class SysDataManagementService : ISysDataManagementService
    {
        private readonly ISysExcelService _excelService;
        private readonly IUnitOfWork _unitOfWork;

        public SysDataManagementService(ISysExcelService excelService, IUnitOfWork unitOfWork)
        {
            _excelService = excelService;
            _unitOfWork = unitOfWork;
        }

        public async Task<SysImportResult> ImportAsync<T>(Stream stream, CancellationToken cancellationToken = default) where T : class, IBaseEntity, new()
        {
            var data = _excelService.ReadData(stream);
            var result = new SysImportResult();
            var entities = new List<T>();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                .ToDictionary(p => p.Name.ToLower(), p => p);

            foreach (var row in data)
            {
                var entity = new T();
                foreach (var kvp in row)
                {
                    if (properties.TryGetValue(kvp.Key.ToLower(), out var prop))
                    {
                        try
                        {
                            var value = Convert.ChangeType(kvp.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                            prop.SetValue(entity, value);
                        }
                        catch
                        {
                            // Log or add to errors
                        }
                    }
                }
                entities.Add(entity);
            }

            // Simple implementation for now
            var dbSet = _unitOfWork.Context.Set<T>();
            await dbSet.AddRangeAsync(entities, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            result.SuccessCount = entities.Count;
            return result;
        }

        public async Task<Stream> ExportAsync<T>(CancellationToken cancellationToken = default) where T : class, IBaseEntity, new()
        {
            var data = await _unitOfWork.Context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
            var props = typeof(T).GetProperties().Select(p => p.Name).ToList();
            return await _excelService.GenerateExcelAsync(data, props, props, cancellationToken);
        }

        public async Task ExportAsync<T>(SysExportRequest request, Stream output, CancellationToken cancellationToken = default) where T : class, IBaseEntity, new()
        {
            var query = _unitOfWork.Context.Set<T>().AsNoTracking();
            // Apply filtering/sorting from request if needed
            
            var headers = request.Columns.Select(c => c.HeaderName ?? c.Field).ToList();
            var props = request.Columns.Select(c => c.Field).ToList();

            await _excelService.WriteAsync(
                output,
                query.AsAsyncEnumerable(),
                headers,
                (item, index) => typeof(T).GetProperty(props[index])?.GetValue(item),
                typeof(T).Name,
                cancellationToken);
        }

        public async Task<Stream> GenerateTemplateAsync<T>(CancellationToken cancellationToken = default) where T : class, IBaseEntity, new()
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.CanWrite && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(DateTime)))
                .Select(p => p.Name)
                .ToList();

            return await _excelService.GenerateTemplateAsync(props, typeof(T).Name, null, cancellationToken);
        }
    }
}
