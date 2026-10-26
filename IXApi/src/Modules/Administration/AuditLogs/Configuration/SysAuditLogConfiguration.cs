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
        }
    }
}
