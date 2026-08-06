using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupConfiguration : IEntityTypeConfiguration<OrgEmployeeGroup>
    {
        public void Configure(EntityTypeBuilder<OrgEmployeeGroup> builder)
        {
            builder.ToTable("OrgEmployeeGroups");

            builder.Property(x => x.Code)
                .HasMaxLength(20);

        }
    }
}

