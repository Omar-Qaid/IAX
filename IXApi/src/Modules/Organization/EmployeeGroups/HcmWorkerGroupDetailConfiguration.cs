using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    public class HcmWorkerGroupDetailConfiguration : IEntityTypeConfiguration<HcmWorkerGroupDetail>
    {
        public void Configure(EntityTypeBuilder<HcmWorkerGroupDetail> builder)
        {
            builder.ToTable("EmployeeGroupDetails");

            builder.HasKey(x => new { x.UserGroupID, x.UserID });

            builder.Property(x => x.UserID)
                .HasMaxLength(256);

            builder.HasOne(x => x.HcmWorkerGroup)
                .WithMany(x => x.HcmWorkerGroupDetails)
                .HasForeignKey(x => x.UserGroupID)
                .OnDelete(DeleteBehavior.Cascade);

            // Membership now references a real AspNetUser (string key).
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

