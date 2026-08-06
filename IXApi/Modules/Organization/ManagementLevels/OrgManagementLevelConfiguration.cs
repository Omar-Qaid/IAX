using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    public class OrgManagementLevelConfiguration : IEntityTypeConfiguration<OrgManagementLevel>
    {
        public void Configure(EntityTypeBuilder<OrgManagementLevel> builder)
        {
            builder.ToTable("OrgManagementLevels");
            builder.Property(x => x.RecId).ValueGeneratedNever();
        }
    }
}


