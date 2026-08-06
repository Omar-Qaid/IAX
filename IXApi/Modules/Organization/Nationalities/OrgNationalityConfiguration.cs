using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Nationalities
{
    public class OrgNationalityConfiguration : IEntityTypeConfiguration<OrgNationality>
    {
        public void Configure(EntityTypeBuilder<OrgNationality> builder)
        {
            builder.ToTable("OrgNationalities");
        }
    }
}
