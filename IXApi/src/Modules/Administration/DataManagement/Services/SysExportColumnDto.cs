using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    public class SysExportColumnDto
    {
        /// <summary>The entity property name (case-insensitive). Nested paths like "Owner.Name" are supported.</summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>Optional display header. Falls back to a humanised property name.</summary>
        public string? HeaderName { get; set; }
    }
}