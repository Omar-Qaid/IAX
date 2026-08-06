using Mapster;

namespace IAX.IXApi.Modules.Organization.Employees
{
    public class HcmWorkerMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker, HcmWorkerDto>()
                .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null)
                .Map(dest => dest.OccupationName, src => src.Occupation != null ? src.Occupation.Name : null)
                .Map(dest => dest.ShowroomName, src => src.Showroom != null ? src.Showroom.Name : null);
        }
    }
}

