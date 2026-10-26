using System.Collections.Generic;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.DataManagement.Contracts
{
    public class SysImportError
    {
        public string Row { get; set; } = string.Empty;
        public string? Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error"; // Error, Warning
    }

    public class SysImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<SysImportError> Errors { get; set; } = new();

        public void AddError(string row, string message, string? column = null)
        {
            Errors.Add(new SysImportError { Row = row, Message = message, Column = column });
        }
    }
}

namespace IAX.IXApi.Modules.Administration.DataManagement.Dtos
{
    public class SysExportColumnDto
    {
        /// <summary>The entity property name (case-insensitive). Nested paths like "Owner.Name" are supported.</summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>Optional display header. Falls back to a humanised property name.</summary>
        public string? HeaderName { get; set; }
    }

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
