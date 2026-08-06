using Mapster;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    /// <summary>
    /// Maps the legacy DTO field names onto the BaseEntity-based OrgEmployeeGroup entity.
    /// The entity PK/name are Id/Name; the DTO (and frontend) still use UserGroupID/UserGroupName.
    /// </summary>
    public class OrgEmployeeGroupMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrgEmployeeGroup, OrgEmployeeGroupDto>()
                .Map(dest => dest.UserGroupID, src => src.RecId)
                .Map(dest => dest.UserGroupName, src => src.Name);

            config.NewConfig<OrgEmployeeGroupDto, OrgEmployeeGroup>()
                .Map(dest => dest.Name, src => src.UserGroupName)
                .Map(dest => dest.NameAR, src => src.UserGroupName)
                .Ignore(dest => dest.OrgEmployeeGroupDetails);
            // Id is [AdaptIgnore(Destination)] — set by identity on create, route id on update.
        }
    }
}


