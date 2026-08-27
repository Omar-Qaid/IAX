using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Identity.Users
{
    public class AspNetUserConfiguration : IEntityTypeConfiguration<AspNetUser>
    {
        public void Configure(EntityTypeBuilder<AspNetUser> builder)
        {
            // The optional link to an org party (employee or showroom) is configured from the
            // OrganizationEntity side — see OrganizationEntityConfiguration (TPH base, FK = AspNetUser.OrganizationEntityId).
        }
    }
}
