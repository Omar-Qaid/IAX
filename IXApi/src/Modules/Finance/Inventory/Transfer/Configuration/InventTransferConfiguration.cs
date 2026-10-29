using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.Transfer
{
    public class InventTransferTableConfiguration : IEntityTypeConfiguration<InventTransferTable>
    {
        public void Configure(EntityTypeBuilder<InventTransferTable> builder)
        {
            builder.ToTable("InventTransferTable");
            builder.HasIndex(x => new { x.DataAreaId, x.TransferId }).IsUnique();
        }
    }

    public class InventTransferLineConfiguration : IEntityTypeConfiguration<InventTransferLine>
    {
        public void Configure(EntityTypeBuilder<InventTransferLine> builder)
        {
            builder.ToTable("InventTransferLine");
            builder.HasIndex(x => new { x.DataAreaId, x.TransferId, x.LineNum }).IsUnique();
            builder.Property(x => x.LineNum).HasPrecision(28, 16);
            foreach (var quantity in new[] { "QtyTransfer", "QtyShipNow", "QtyShipped", "QtyRemainShip",
                "QtyReceiveNow", "QtyReceived", "QtyRemainReceive", "QtyScrapped" })
                builder.Property<decimal>(quantity).HasPrecision(28, 6);
            builder.Property(x => x.OverDeliveryPct).HasPrecision(28, 6);
            builder.Property(x => x.UnderDeliveryPct).HasPrecision(28, 6);
        }
    }
}
