using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    /// <summary>
    /// Describes a single column to include in an export.
    /// </summary>
    public class SysExportColumnDto
    {
        /// <summary>The entity property name (case-insensitive). Nested paths like "Owner.Name" are supported.</summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>Optional display header. Falls back to a humanised property name.</summary>
        public string? HeaderName { get; set; }
    }

    /// <summary>
    /// Server-side export request with Sys prefix.
    /// </summary>
    public class SysExportRequest
    {
        /// <summary>Filter / sort / search state from the grid. PageNumber/PageSize are ignored for export.</summary>
        public QueryFilterDto? Query { get; set; }

        /// <summary>Columns to include. If null or empty, every eligible property is exported.</summary>
        public List<SysExportColumnDto>? Columns { get; set; }

        /// <summary>Optional sheet name. Defaults to the entity type name.</summary>
        public string? SheetName { get; set; }

        /// <summary>
        /// Hard cap on rows written. Defaults to 100,000 — adjust per deployment.
        /// Prevents a single user from exhausting a worker on a multi-million row table.
        /// </summary>
        public int MaxRows { get; set; } = 100_000;
    }
}
