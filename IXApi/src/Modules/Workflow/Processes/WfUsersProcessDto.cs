using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Occupations;
using IAX.IXApi.Modules.Organization.Employees;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfUsersProcessDto : BaseEntityDto<long>
    {
        public long ProcessId { get; set; }
        public short? DepartmentId { get; set; }
        public short? OccupationId { get; set; }
        public long? EmployeeId { get; set; }

        public DepartmentDto? Department { get; set; }
        public OccupationDto? Occupation { get; set; }
        public HcmWorkerDto? Employee { get; set; }
    }
}
