using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class PurchTableConfiguration : IEntityTypeConfiguration<PurchTable>
    {
        public void Configure(EntityTypeBuilder<PurchTable> builder)
        {
            builder.ToTable("PurchTable");
            builder.HasIndex(x => new { x.DataAreaId, x.PurchId }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.OrderAccount });
            builder.Property(x => x.PurchName).HasMaxLength(250);
            builder.Property(x => x.CurrencyCode).HasMaxLength(FieldLengths.CurrencyCode);
            builder.Property(x => x.Payment).HasMaxLength(50);
            builder.Property(x => x.InventSiteId).HasMaxLength(50);
            builder.Property(x => x.InventLocationId).HasMaxLength(50);
        }
    }

    public class PurchLineConfiguration : IEntityTypeConfiguration<PurchLine>
    {
        public void Configure(EntityTypeBuilder<PurchLine> builder)
        {
            builder.ToTable("PurchLine");
            builder.HasIndex(x => new { x.DataAreaId, x.PurchId, x.LineNumber });
            builder.HasIndex(x => new { x.DataAreaId, x.InventTransId });
            builder.Property(x => x.Name).HasMaxLength(1000);
            builder.Property(x => x.CurrencyCode).HasMaxLength(FieldLengths.CurrencyCode);
            builder.Property(x => x.PurchUnit).HasMaxLength(10);
            builder.Property(x => x.InventDimId).HasMaxLength(FieldLengths.InventDimId);
        }
    }
}
