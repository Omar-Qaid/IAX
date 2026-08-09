namespace IAX.IXApi.Modules.Administration.Settings
{
    public class SysSettingsDto
    {
        public int RecId { get; set; }
        public string AppName { get; set; } = null!;
        public string DefaultLanguage { get; set; } = null!;
        public string TimeZone { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string DateFormat { get; set; } = null!;
        public bool EnableAuditLog { get; set; }
        public long MaxUploadSize { get; set; }
        public int PaginationSize { get; set; }
        public int DecimalPlaces { get; set; }
    }
}

