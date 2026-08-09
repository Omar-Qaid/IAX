using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupDetailConfiguration : IEntityTypeConfiguration<OrgEmployeeGroupDetail>
    {
        public void Configure(EntityTypeBuilder<OrgEmployeeGroupDetail> builder)
        {
            builder.ToTable("OrgEmployeeGroupDetails");

            builder.HasKey(x => new { x.UserGroupID, x.UserID });

            builder.Property(x => x.UserID)
                .HasMaxLength(256);

            builder.HasOne(x => x.OrgEmployeeGroup)
                .WithMany(x => x.OrgEmployeeGroupDetails)
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

