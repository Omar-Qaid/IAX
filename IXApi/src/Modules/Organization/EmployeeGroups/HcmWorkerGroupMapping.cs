using Mapster;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    /// <summary>
    /// Maps the legacy DTO field names onto the BaseEntity-based EmployeeGroup entity.
    /// The entity PK/name are Id/Name; the DTO (and frontend) still use UserGroupID/UserGroupName.
    /// </summary>
    public class HcmWorkerGroupMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<HcmWorkerGroup, HcmWorkerGroupDto>()
                .Map(dest => dest.UserGroupID, src => src.RecId)
                .Map(dest => dest.UserGroupName, src => src.Name);

            config.NewConfig<HcmWorkerGroupDto, HcmWorkerGroup>()
                .Map(dest => dest.Name, src => src.UserGroupName)
                .Ignore(dest => dest.HcmWorkerGroupDetails);
            // Id is [AdaptIgnore(Destination)] — set by identity on create, route id on update.
        }
    }
}


