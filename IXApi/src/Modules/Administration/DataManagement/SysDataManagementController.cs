using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Modules.Administration.DataManagement.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IAX.IXApi.Modules.Administration.DataManagement
{
    /// <summary>
    /// System entry point for generic data management.
    /// Supports Import, Export, and Template generation for any whitelisted entity.
    /// </summary>
    [Authorize]
    [EnableRateLimiting("tight")]
    [Route("api/v1/SysDataManagement")]
    [ApiController]
    public class SysDataManagementController : ControllerBase
    {
        private readonly ISysDataManagementService _dataManagementService;
        private readonly ISysDataManagementEntityProvider _entityProvider;

        public SysDataManagementController(
            ISysDataManagementService dataManagementService,
            ISysDataManagementEntityProvider entityProvider)
        {
            _dataManagementService = dataManagementService;
            _entityProvider = entityProvider;
        }

        [HttpPost("{entityName}/import")]
        [DomainPermission("System", "DataManagement", "Import")]
        public async Task<ActionResult<APIResponse<SysImportResult>>> Import(string entityName, IFormFile file, CancellationToken cancellationToken)
        {
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(APIResponse<SysImportResult>.Fail($"Entity '{entityName}' is not allowed for data management."));

            if (file == null || file.Length == 0)
                return BadRequest(APIResponse<SysImportResult>.Fail("No file uploaded."));

            using var stream = file.OpenReadStream();

            var method = typeof(ISysDataManagementService).GetMethod(nameof(ISysDataManagementService.ImportAsync))
                ?.MakeGenericMethod(entityType);

            if (method == null) return StatusCode(500, new { message = "Internal Server Error: Could not resolve import method." });

            var task = (Task<SysImportResult>)method.Invoke(_dataManagementService, new object[] { stream, cancellationToken })!;
            var result = await task;

            return Ok(APIResponse<SysImportResult>.Ok(result, "Import completed successfully."));
        }

        [HttpGet("{entityName}/export")]
        [DomainPermission("System", "DataManagement", "Export")]
        public async Task<IActionResult> Export(string entityName, CancellationToken cancellationToken)
        {
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(new { message = $"Entity '{entityName}' is not allowed for data management." });

            var method = typeof(ISysDataManagementService).GetMethod(nameof(ISysDataManagementService.ExportAsync), new[] { typeof(CancellationToken) })
                ?.MakeGenericMethod(entityType);

            if (method == null) return StatusCode(500, "Internal Server Error: Could not resolve export method.");

            var task = (Task<Stream>)method.Invoke(_dataManagementService, new object[] { cancellationToken })!;
            var stream = await task;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{entityName}_Export_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        /// <summary>
        /// Server-side export of the current grid view. The body carries the same filter / sort / search
        /// the paged list endpoints use, plus the visible columns. The XLSX is streamed straight to the
        /// HTTP response â€” no buffering of the full result set in memory.
        /// </summary>
        [HttpPost("{entityName}/export")]
        [DomainPermission("System", "DataManagement", "Export")]
        public async Task<IActionResult> ExportView(string entityName, [FromBody] SysExportRequest request, CancellationToken cancellationToken)
        {
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(new { message = $"Entity '{entityName}' is not allowed for data management." });

            request ??= new SysExportRequest();

            // Set headers BEFORE writing â€” once any byte is on the wire, headers are frozen.
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.ContentDisposition = $"attachment; filename=\"{entityName}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx\"";

            var method = typeof(ISysDataManagementService)
                .GetMethod(nameof(ISysDataManagementService.ExportAsync), new[] { typeof(SysExportRequest), typeof(Stream), typeof(CancellationToken) })
                ?.MakeGenericMethod(entityType);

            if (method == null) return StatusCode(500, "Internal Server Error: Could not resolve export method.");

            var task = (Task)method.Invoke(_dataManagementService, new object[] { request, Response.Body, cancellationToken })!;
            await task;

            return new EmptyResult(); // body already written
        }

        [HttpGet("{entityName}/template")]
        [DomainPermission("System", "DataManagement", "View")]
        public async Task<IActionResult> GetTemplate(string entityName, CancellationToken cancellationToken)
        {
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(new { message = $"Entity '{entityName}' is not allowed for data management." });

            var method = typeof(ISysDataManagementService).GetMethod(nameof(ISysDataManagementService.GenerateTemplateAsync))
                ?.MakeGenericMethod(entityType);

            if (method == null) return StatusCode(500, "Internal Server Error: Could not resolve template method.");

            var task = (Task<Stream>)method.Invoke(_dataManagementService, new object[] { cancellationToken })!;
            var stream = await task;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{entityName}_Template.xlsx");
        }

        [HttpGet("allowed-entities")]
        [DomainPermission("System", "DataManagement", "View")]
        public ActionResult<APIResponse<IEnumerable<string>>> GetAllowedEntities()
        {
            return Ok(APIResponse<IEnumerable<string>>.Ok(_entityProvider.GetAllowedEntities().Keys));
        }

        /// <summary>
        /// Returns all fields (properties) for the given entity.
        /// Used by the frontend DataGrid to populate the "Manage Columns" panel.
        /// </summary>
        [HttpGet("{entityName}/fields")]
        [DomainPermission("System", "DataManagement", "View")]
        public ActionResult<APIResponse<IEnumerable<object>>> GetFields(string entityName)
        {
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(APIResponse<IEnumerable<object>>.Fail($"Entity '{entityName}' is not allowed for data management."));

            // The API returns Dto classes (e.g. WfCategoryDto). We should extract fields
            // from the Dto to ensure exact property matches. If no Dto exists, fallback to Entity.
            var assembly = entityType.Assembly;
            var dtoTypeName = $"{entityType.Namespace?.Replace("Models", "DTOs")}.{entityType.Name}Dto";
            var dtoType = assembly.GetType(dtoTypeName) ?? entityType;

            var properties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var fields = properties.Select(p => new
            {
                field = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(p.Name), // exact match with JSON output
                headerName = PascalToHeaderName(p.Name),
                type = MapToDataGridType(p.PropertyType),
                nullable = IsNullable(p.PropertyType),
            }).ToList();

            return Ok(APIResponse<IEnumerable<object>>.Ok(fields));
        }

        // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static string PascalToHeaderName(string name)
        {
            // "NameAR" â†’ "Name AR", "SortOrder" â†’ "Sort Order", "IsActive" â†’ "Is Active"
            var spaced = Regex.Replace(name, @"(?<=[a-z])([A-Z])", " $1");
            spaced = Regex.Replace(spaced, @"([A-Z]+)([A-Z][a-z])", "$1 $2");
            return spaced;
        }

        private static string MapToDataGridType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying == typeof(bool)) return "boolean";
            if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset)) return "dateTime";
            if (underlying == typeof(DateOnly)) return "date";
            if (underlying == typeof(int) || underlying == typeof(long) ||
                underlying == typeof(short) || underlying == typeof(byte) ||
                underlying == typeof(decimal) || underlying == typeof(float) ||
                underlying == typeof(double)) return "number";

            // Nested user navigation properties (UserDto or subclasses)
            if (typeof(UserDto).IsAssignableFrom(underlying))
                return "user";

            return "string";
        }

        private static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }
}

