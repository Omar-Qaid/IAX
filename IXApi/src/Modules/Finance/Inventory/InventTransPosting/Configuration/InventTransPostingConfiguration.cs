using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTransPosting
{
    public class InventTransPostingConfiguration : IEntityTypeConfiguration<Entities.InventTransPosting>
    {
        public void Configure(EntityTypeBuilder<Entities.InventTransPosting> builder)
        {
            builder.ToTable("InventTransPosting");
            builder.HasIndex(x => new { x.DataAreaId, x.InventTransOrigin, x.InventTransPostingType });
            builder.HasIndex(x => new { x.DataAreaId, x.Voucher });
            builder.HasOne<InventTransOrigin>().WithMany()
                .HasForeignKey(x => x.InventTransOrigin).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
