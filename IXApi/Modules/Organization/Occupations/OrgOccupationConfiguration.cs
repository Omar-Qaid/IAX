using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Occupations
{
    public class OrgOccupationConfiguration : IEntityTypeConfiguration<OrgOccupation>
    {
        public void Configure(EntityTypeBuilder<OrgOccupation> builder)
        {
            builder.ToTable("OrgOccupations");
        }
    }
}
