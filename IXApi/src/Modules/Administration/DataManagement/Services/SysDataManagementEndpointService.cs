using System.Reflection;
using System.Text.RegularExpressions;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Modules.Administration.DataManagement.Dtos;
using IAX.IXApi.Modules.Administration.DataManagement.Interfaces;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services;

public sealed class SysDataManagementEndpointService : ISysDataManagementEndpointService
{
    private readonly Interfaces.ISysDataManagementService _dataManagement;

    public SysDataManagementEndpointService(Interfaces.ISysDataManagementService dataManagement)
    {
        _dataManagement = dataManagement;
    }

    public async Task<SysImportResult?> ImportAsync(Type entityType, Stream stream, CancellationToken cancellationToken)
    {
        var method = typeof(ISysDataManagementService).GetMethod(nameof(ISysDataManagementService.ImportAsync))
            ?.MakeGenericMethod(entityType);
        if (method == null) return null;
        var task = (Task<SysImportResult>)method.Invoke(_dataManagement, [stream, cancellationToken])!;
        return await task;
    }

    public async Task<Stream?> ExportAsync(Type entityType, CancellationToken cancellationToken)
    {
        var method = typeof(ISysDataManagementService)
            .GetMethod(nameof(ISysDataManagementService.ExportAsync), [typeof(CancellationToken)])
            ?.MakeGenericMethod(entityType);
        if (method == null) return null;
        var task = (Task<Stream>)method.Invoke(_dataManagement, [cancellationToken])!;
        return await task;
    }

    public async Task<bool> ExportViewAsync(Type entityType, SysExportRequest request, Stream output,
        CancellationToken cancellationToken)
    {
        var method = typeof(ISysDataManagementService).GetMethod(nameof(ISysDataManagementService.ExportAsync),
            [typeof(SysExportRequest), typeof(Stream), typeof(CancellationToken)])?.MakeGenericMethod(entityType);
        if (method == null) return false;
        var task = (Task)method.Invoke(_dataManagement, [request, output, cancellationToken])!;
        await task;
        return true;
    }

    public async Task<Stream?> GenerateTemplateAsync(Type entityType, CancellationToken cancellationToken)
    {
        var method = typeof(ISysDataManagementService).GetMethod(nameof(ISysDataManagementService.GenerateTemplateAsync))
            ?.MakeGenericMethod(entityType);
        if (method == null) return null;
        var task = (Task<Stream>)method.Invoke(_dataManagement, [cancellationToken])!;
        return await task;
    }

    public IEnumerable<object> GetFields(Type entityType)
    {
        var assembly = entityType.Assembly;
        var dtoTypeName = $"{entityType.Namespace?.Replace("Models", "DTOs")}.{entityType.Name}Dto";
        var dtoType = assembly.GetType(dtoTypeName) ?? entityType;
        return dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => new
        {
            field = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(property.Name),
            headerName = PascalToHeaderName(property.Name),
            type = MapToDataGridType(property.PropertyType),
            nullable = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null
        }).Cast<object>().ToList();
    }

    private static string PascalToHeaderName(string name)
    {
        var spaced = Regex.Replace(name, @"(?<=[a-z])([A-Z])", " $1");
        return Regex.Replace(spaced, @"([A-Z]+)([A-Z][a-z])", "$1 $2");
    }

    private static string MapToDataGridType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying == typeof(bool)) return "boolean";
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset)) return "dateTime";
        if (underlying == typeof(DateOnly)) return "date";
        if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short) ||
            underlying == typeof(byte) || underlying == typeof(decimal) || underlying == typeof(float) ||
            underlying == typeof(double)) return "number";
        if (typeof(UserDto).IsAssignableFrom(underlying)) return "user";
        return "string";
    }
}
