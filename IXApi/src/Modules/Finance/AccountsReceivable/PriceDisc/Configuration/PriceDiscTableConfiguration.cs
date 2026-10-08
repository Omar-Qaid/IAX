using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscTableConfiguration : IEntityTypeConfiguration<PriceDiscTable>
    {
        public void Configure(EntityTypeBuilder<PriceDiscTable> builder)
        {
            builder.ToTable("PriceDiscTable");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
            builder.Property(x => x.AccountRelation).HasMaxLength(20);
            builder.Property(x => x.ItemRelation).HasMaxLength(20);
            builder.Property(x => x.Currency).HasMaxLength(3);
            builder.Property(x => x.UnitId).HasMaxLength(10);
            builder.Property(x => x.InventDimId).HasMaxLength(100);
            builder.Property(x => x.Agreement).HasMaxLength(10);
            builder.Property(x => x.PriceGroup).HasMaxLength(10);

            builder.HasOne(p => p.CustTable)
                .WithMany()
                .HasForeignKey(p => p.AccountRelation)
                .HasPrincipalKey(c => c.AccountNum)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.InventTable)
                .WithMany()
                .HasForeignKey(p => p.ItemRelation)
                .HasPrincipalKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.InventDim)
                .WithMany()
                .HasForeignKey(p => p.InventDimId)
                .HasPrincipalKey(d => d.InventDimId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
