using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Genders
{
    public class OrgGenderConfiguration : IEntityTypeConfiguration<OrgGender>
    {
        public void Configure(EntityTypeBuilder<OrgGender> builder)
        {
            builder.ToTable("OrgGenders");
        }
    }
}
