using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Notifications.Configuration
{
    public class SysNotificationTemplateConfiguration : IEntityTypeConfiguration<SysNotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<SysNotificationTemplate> builder)
        {
            builder.ToTable("SysNotificationTemplates");

            builder.HasIndex(e => e.Code).IsUnique();
        }
    }
}