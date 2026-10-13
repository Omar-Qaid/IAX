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
            builder.HasIndex(x => x.AccountRelation);
            builder.HasIndex(x => x.ItemRelation);
            builder.HasIndex(x => x.InventDimId);

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
            builder.Property(x => x.AccountRelation).HasMaxLength(20);
            builder.Property(x => x.ItemRelation).HasMaxLength(20);
            builder.Property(x => x.Currency).HasMaxLength(3);
            builder.Property(x => x.UnitId).HasMaxLength(10);
            builder.Property(x => x.InventDimId).HasMaxLength(100);
            builder.Property(x => x.Agreement).HasMaxLength(10);
            builder.Property(x => x.PriceGroup).HasMaxLength(10);

            // These are conditional lookup keys. AccountRelation and ItemRelation can
            // contain a group code or mean "all", so unconditional SQL FKs reject
            // valid trade agreements. InventDimId is optional and commonly empty.
            builder.Ignore(p => p.CustTable);
            builder.Ignore(p => p.InventTable);
            builder.Ignore(p => p.InventDim);
        }
    }
}
