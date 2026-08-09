using System;
using Mapster;

namespace IAX.IXApi.Modules.Identity.Users
{
    public class AspNetUserMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AspNetUser, AspNetUserDto>()
                // EmployeeId on the DTO is the linked org party (OrgEntity) id.
                .Map(dest => dest.EmployeeId, src => src.OrgEntityId)
                // The user's linked org party (employee or showroom) name.
                .Map(dest => dest.EmployeeName, src => src.OrgEntity != null ? src.OrgEntity.Name : null)
                // Enabled = the account is not currently locked out.
                .Map(dest => dest.Enabled, src => !src.LockoutEnd.HasValue || src.LockoutEnd.Value <= DateTimeOffset.Now);
        }
    }
}
