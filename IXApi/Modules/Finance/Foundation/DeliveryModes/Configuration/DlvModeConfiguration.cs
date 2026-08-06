using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class DlvModeConfiguration : IEntityTypeConfiguration<DlvMode>
    {
        public void Configure(EntityTypeBuilder<DlvMode> builder)
        {
            builder.ToTable("DlvMode");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => x.Code).IsUnique();
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}


