using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Configuration
{
    public class SysExceptionLogConfiguration : IEntityTypeConfiguration<SysExceptionLog>
    {
        public void Configure(EntityTypeBuilder<SysExceptionLog> builder)
        {
            builder.ToTable("SysExceptionLogs");
            builder.HasKey(x => x.RecId);
        }
    }
}
