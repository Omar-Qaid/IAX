using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTableFeature
{
    public class InventTransConfiguration : IEntityTypeConfiguration<InventTrans>
    {
        public void Configure(EntityTypeBuilder<InventTrans> builder)
        {
            builder.HasOne<IAX.IXApi.Modules.Finance.Entities.InventTable>()
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .HasPrincipalKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

     
        }
    }
}

