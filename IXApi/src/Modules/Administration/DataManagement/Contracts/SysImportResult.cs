namespace IAX.IXApi.Modules.Administration.DataManagement.Contracts
{
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