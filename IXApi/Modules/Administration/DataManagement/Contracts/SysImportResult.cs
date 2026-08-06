namespace IAX.IXApi.Modules.Administration.DataManagement.Contracts
{
    /// <summary>
    /// Represents a single error during the import process.
    /// </summary>
    public class SysImportError
    {
        public string Row { get; set; } = string.Empty;
        public string? Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error"; // Error, Warning
    }

    /// <summary>
    /// Final result of the import operation.
    /// </summary>
    public class SysImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<SysImportError> Errors { get; set; } = new();
        
        // Helper legacy support (if needed, but we'll try to stick to structured)
        public void AddError(string row, string message, string? column = null)
        {
            Errors.Add(new SysImportError { Row = row, Message = message, Column = column });
        }
    }
}
