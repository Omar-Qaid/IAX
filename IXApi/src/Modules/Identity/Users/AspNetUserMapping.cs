using System;
using Mapster;

namespace IAX.IXApi.Modules.Identity.Users
{
    public class AspNetUserMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AspNetUser, AspNetUserDto>()
                // EmployeeId on the DTO is the linked org party (OrganizationEntity) id.
                .Map(dest => dest.EmployeeId, src => src.OrganizationEntityId)
                // The user's linked org party (employee or showroom) name.
                .Map(dest => dest.EmployeeName, src => src.OrganizationEntity != null ? src.OrganizationEntity.Name : null)
                // Enabled = the account is not currently locked out.
                .Map(dest => dest.Enabled, src => !src.LockoutEnd.HasValue || src.LockoutEnd.Value <= DateTimeOffset.Now);
        }
    }
}
