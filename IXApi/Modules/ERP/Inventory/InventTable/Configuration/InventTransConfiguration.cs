using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.Inventory.InventTableFeature
{
    public class InventTransConfiguration : IEntityTypeConfiguration<InventTrans>
    {
        public void Configure(EntityTypeBuilder<InventTrans> builder)
        {
            builder.HasOne<IAX.IXApi.Modules.ERP.Entities.InventTable>()
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .HasPrincipalKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

     
        }
    }
}
