using System;
using Mapster;

namespace IAX.IXApi.Modules.Identity.Users
{
    public class AspNetUserMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AspNetUser, AspNetUserDto>()
                // Enabled = the account is not currently locked out.
                .Map(dest => dest.Enabled, src => !src.LockoutEnd.HasValue || src.LockoutEnd.Value <= DateTimeOffset.Now);
        }
    }
}
