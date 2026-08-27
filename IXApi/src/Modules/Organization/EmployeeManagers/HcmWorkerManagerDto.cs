namespace IAX.IXApi.Modules.Organization.HcmWorkerManagers
{
    /// <summary>
    /// One manager assignment for an employee at a given management level.
    /// Names are populated on read for display; ignored on write.
    /// </summary>
    public class HcmWorkerManagerDto
    {
        public long EmployeeId { get; set; }
        public byte ManagementLevelId { get; set; }
        public long ManagerId { get; set; }

        public string? EmployeeName { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagementLevelName { get; set; }
    }
}

