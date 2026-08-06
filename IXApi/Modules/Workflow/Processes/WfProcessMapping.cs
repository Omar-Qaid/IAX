using Mapster;
using IAX.IXApi.Modules.Workflow.Processes;
using System.Linq;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WfUsersProcessDto, WfUsersProcess>()
                .Map(dest => dest.DepartmentId, src => src.DepartmentId)
                .Map(dest => dest.OccupationId, src => src.OccupationId)
                .Map(dest => dest.EmployeeId, src => src.EmployeeId)
                .Ignore(dest => dest.Process)
                .Ignore(dest => dest.Department)
                .Ignore(dest => dest.Occupation)
                .Ignore(dest => dest.Employee);

            config.NewConfig<WfProcessDto, WfProcess>()
                .Map(dest => dest.UsersProcesses, src => src.UsersProcesses);
        }
    }
}
