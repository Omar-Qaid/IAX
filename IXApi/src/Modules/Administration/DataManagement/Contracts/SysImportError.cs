namespace IAX.IXApi.Modules.Administration.DataManagement.Contracts
{
    public class SysImportError
    {
        public string Row { get; set; } = string.Empty;
        public string? Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error"; // Error, Warning
    }
}