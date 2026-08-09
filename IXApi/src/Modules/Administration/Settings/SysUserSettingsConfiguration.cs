using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.Settings
{
    public class SysUserSettingsConfiguration : IEntityTypeConfiguration<SysUserSettings>
    {
        public void Configure(EntityTypeBuilder<SysUserSettings> builder)
        {
            builder.ToTable("SysUserSettings");

            builder.Property(x => x.UserId).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Theme).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Language).HasMaxLength(10).IsRequired();
            builder.Property(x => x.PageSize).IsRequired();
            builder.Property(x => x.NotificationEnabled).IsRequired();
            builder.Property(x => x.DashboardLayout).HasMaxLength(2000);

            // One settings record per user
            builder.HasIndex(x => x.UserId).IsUnique();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
