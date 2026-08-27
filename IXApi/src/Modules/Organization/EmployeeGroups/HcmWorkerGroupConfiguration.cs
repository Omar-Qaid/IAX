using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    public class HcmWorkerGroupConfiguration : IEntityTypeConfiguration<HcmWorkerGroup>
    {
        public void Configure(EntityTypeBuilder<HcmWorkerGroup> builder)
        {
            builder.ToTable("EmployeeGroups");

            builder.Property(x => x.Code)
                .HasMaxLength(20);

        }
    }
}

