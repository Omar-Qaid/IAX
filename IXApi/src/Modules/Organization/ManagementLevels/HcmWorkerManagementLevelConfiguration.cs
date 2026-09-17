using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    public class HcmWorkerManagementLevelConfiguration : IEntityTypeConfiguration<HcmWorkerManagementLevel>
    {
        public void Configure(EntityTypeBuilder<HcmWorkerManagementLevel> builder)
        {
            builder.ToTable("HcmWorkerManagementLevels");
            builder.Property(x => x.RecId).ValueGeneratedNever();
        }
    }
}
