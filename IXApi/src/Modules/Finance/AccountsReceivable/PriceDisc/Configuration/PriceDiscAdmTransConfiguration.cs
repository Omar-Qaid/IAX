using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class PriceDiscAdmTransConfiguration : IEntityTypeConfiguration<PriceDiscAdmTrans>
    {
        public void Configure(EntityTypeBuilder<PriceDiscAdmTrans> builder)
        {
            builder.ToTable("PriceDiscAdmTrans");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.JournalNum, x.LineNum });

            builder.Property(x => x.JournalNum).HasMaxLength(100).IsRequired();
            builder.Property(x => x.AccountRelation).HasMaxLength(20);
            builder.Property(x => x.ItemRelation).HasMaxLength(20);
            builder.Property(x => x.Currency).HasMaxLength(3);
            builder.Property(x => x.UnitId).HasMaxLength(10);
            builder.Property(x => x.InventDimId).HasMaxLength(100);
            builder.Property(x => x.Agreement).HasMaxLength(10);
            builder.Property(x => x.PriceGroup).HasMaxLength(10);
            builder.Property(x => x.Log).HasMaxLength(255);
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            builder.HasOne(p => p.PriceDiscAdmTable)
                .WithMany()
                .HasForeignKey(p => p.JournalNum)
                .HasPrincipalKey(h => h.JournalNum)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.PriceDiscTable)
                .WithMany()
                .HasForeignKey(p => p.PriceDiscTableRef)
                .HasPrincipalKey(t => t.RecId)
                .OnDelete(DeleteBehavior.NoAction);

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
