using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    public class LogisticsLocationRoleConfiguration : IEntityTypeConfiguration<LogisticsLocationRole>
    {
        public void Configure(EntityTypeBuilder<LogisticsLocationRole> builder)
        {
            builder.ToTable("LogisticsLocationRole");
            builder.HasKey(x => x.RecId);

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
