namespace IAX.IXApi.Modules.Organization.EmployeeManagers
{
    /// <summary>
    /// One manager assignment for an employee at a given management level.
    /// Names are populated on read for display; ignored on write.
    /// </summary>
    public class OrgEmployeeManagerDto
    {
        public long EmployeeId { get; set; }
        public byte ManagementLevelId { get; set; }
        public long ManagerId { get; set; }

        public string? EmployeeName { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagementLevelName { get; set; }
    }
}

