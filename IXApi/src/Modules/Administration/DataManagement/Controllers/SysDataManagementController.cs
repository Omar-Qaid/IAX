using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Modules.Administration.DataManagement.Dtos;
using IAX.IXApi.Modules.Administration.DataManagement.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IAX.IXApi.Modules.Administration.DataManagement.Controllers
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
        private readonly ISysDataManagementEndpointService _dataManagementService;
        private readonly ISysDataManagementEntityProvider _entityProvider;
        private readonly ILogger<SysDataManagementController> _logger;

        public SysDataManagementController(
            ISysDataManagementEndpointService dataManagementService,
            ISysDataManagementEntityProvider entityProvider,
            ILogger<SysDataManagementController> logger)
        {
            _dataManagementService = dataManagementService;
            _entityProvider = entityProvider;
            _logger = logger;
        }

        [HttpPost("{entityName}/import")]
        [DomainPermission("System", "DataManagement", "Import")]
        public async Task<ActionResult<APIResponse<SysImportResult>>> Import(string entityName, IFormFile file, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[DataManagement] - Importing data for entity {EntityName}", entityName);
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(APIResponse<SysImportResult>.Fail($"Entity '{entityName}' is not allowed for data management."));

            if (file == null || file.Length == 0)
                return BadRequest(APIResponse<SysImportResult>.Fail("No file uploaded."));

            using var stream = file.OpenReadStream();

            var result = await _dataManagementService.ImportAsync(entityType, stream, cancellationToken);
            if (result == null) return StatusCode(500, new { message = "Internal Server Error: Could not resolve import method." });

            return Ok(APIResponse<SysImportResult>.Ok(result, "Import completed successfully."));
        }

        [HttpGet("{entityName}/export")]
        [DomainPermission("System", "DataManagement", "Export")]
        public async Task<IActionResult> Export(string entityName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[DataManagement] - Exporting data for entity {EntityName}", entityName);
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(new { message = $"Entity '{entityName}' is not allowed for data management." });

            var stream = await _dataManagementService.ExportAsync(entityType, cancellationToken);
            if (stream == null) return StatusCode(500, "Internal Server Error: Could not resolve export method.");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{entityName}_Export_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        /// <summary>
        /// Server-side export of the current grid view. The body carries the same filter / sort / search
        /// the paged list endpoints use, plus the visible columns. The XLSX is streamed straight to the
        /// HTTP response — no buffering of the full result set in memory.
        /// </summary>
        [HttpPost("{entityName}/export")]
        [DomainPermission("System", "DataManagement", "Export")]
        public async Task<IActionResult> ExportView(string entityName, [FromBody] SysExportRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[DataManagement] - Exporting view for entity {EntityName}", entityName);
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(new { message = $"Entity '{entityName}' is not allowed for data management." });

            request ??= new SysExportRequest();

            // Set headers BEFORE writing — once any byte is on the wire, headers are frozen.
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.ContentDisposition = $"attachment; filename=\"{entityName}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx\"";

            if (!await _dataManagementService.ExportViewAsync(entityType, request, Response.Body, cancellationToken))
                return StatusCode(500, "Internal Server Error: Could not resolve export method.");

            return new EmptyResult(); // body already written
        }

        [HttpGet("{entityName}/template")]
        [DomainPermission("System", "DataManagement", "View")]
        public async Task<IActionResult> GetTemplate(string entityName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[DataManagement] - Generating template for entity {EntityName}", entityName);
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(new { message = $"Entity '{entityName}' is not allowed for data management." });

            var stream = await _dataManagementService.GenerateTemplateAsync(entityType, cancellationToken);
            if (stream == null) return StatusCode(500, "Internal Server Error: Could not resolve template method.");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{entityName}_Template.xlsx");
        }

        [HttpGet("allowed-entities")]
        [DomainPermission("System", "DataManagement", "View")]
        public ActionResult<APIResponse<IEnumerable<string>>> GetAllowedEntities()
        {
            _logger.LogInformation("[DataManagement] - Fetching allowed entities");
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
            _logger.LogInformation("[DataManagement] - Fetching fields for entity {EntityName}", entityName);
            var entityType = _entityProvider.GetEntityType(entityName);
            if (entityType == null)
                return NotFound(APIResponse<IEnumerable<object>>.Fail($"Entity '{entityName}' is not allowed for data management."));

            var fields = _dataManagementService.GetFields(entityType);

            return Ok(APIResponse<IEnumerable<object>>.Ok(fields));
        }
    }
}
