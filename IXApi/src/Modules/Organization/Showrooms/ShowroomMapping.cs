using Mapster;

namespace IAX.IXApi.Modules.Organization.Showrooms
{
    /// <summary>
    /// Null-safe projection of <see cref="Showroom"/> to <see cref="ShowroomDto"/>.
    /// Without this, Mapster auto-flattens <c>Department.Name</c> into
    /// <c>DepartmentName</c> and throws a <see cref="System.NullReferenceException"/>
    /// when the <c>Department</c> navigation is not eager-loaded (e.g. GetById).
    /// Mirrors <c>OrgEmployeeMapping</c>.
    /// </summary>
    public class ShowroomMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Showroom, ShowroomDto>()
                .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null)
                .Map(dest => dest.SellerCount, src => src.Sellers != null ? src.Sellers.Count : 0);
        }
    }
}

