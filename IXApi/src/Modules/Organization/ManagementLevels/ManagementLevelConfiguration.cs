using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    public class ManagementLevelConfiguration : IEntityTypeConfiguration<ManagementLevel>
    {
        public void Configure(EntityTypeBuilder<ManagementLevel> builder)
        {
            builder.ToTable("ManagementLevels");
            builder.Property(x => x.RecId).ValueGeneratedNever();
        }
    }
}


