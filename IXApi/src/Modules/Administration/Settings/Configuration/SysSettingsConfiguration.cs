using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IAX.IXApi.Modules.Administration.Settings.Entities;

namespace IAX.IXApi.Modules.Administration.Settings.Configuration
{
    public class SysSettingsConfiguration : IEntityTypeConfiguration<SysSettings>
    {
        public void Configure(EntityTypeBuilder<SysSettings> builder)
        {
            builder.ToTable("SysSettings");

            builder.Property(x => x.AppName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.DefaultLanguage).HasMaxLength(10).IsRequired();
            builder.Property(x => x.TimeZone).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            builder.Property(x => x.DateFormat).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EnableAuditLog).IsRequired();
            builder.Property(x => x.MaxUploadSize).IsRequired();
            builder.Property(x => x.PaginationSize).IsRequired();
        }
    }
}
