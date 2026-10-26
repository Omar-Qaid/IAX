using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Configuration
{
    public class SysAuditLogConfiguration : IEntityTypeConfiguration<SysAuditLog>
    {
        public void Configure(EntityTypeBuilder<SysAuditLog> builder)
        {
            builder.ToTable("SysAuditLogs");
            builder.HasKey(x => x.RecId);
            builder.Property(x => x.TableName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.RecordId).HasMaxLength(128).IsRequired();
            builder.Property(x => x.ColumnName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.ChangedBy).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Operation).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => new { x.TableName, x.RecordId });
        }
    }
}
