namespace IAX.IXApi.Modules.Administration.Settings
{
    public class SysUserSettingsDto
    {
        public int RecId { get; set; }
        public string UserId { get; set; } = null!;
        public string Theme { get; set; } = null!;
        public string Language { get; set; } = null!;
        public int PageSize { get; set; }
        public bool NotificationEnabled { get; set; }
        public string DashboardLayout { get; set; } = null!;
    }
}

