using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterTransConfiguration : IEntityTypeConfiguration<CustCollectionLetterTrans>
{
    public void Configure(EntityTypeBuilder<CustCollectionLetterTrans> builder)
    {
        builder.ToTable("CustCollectionLetterTrans");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CollectionLetterNum).HasMaxLength(20);
        builder.Property(x => x.AccountNum).HasMaxLength(20);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.TransDate).HasColumnType("date");
        builder.Property(x => x.Invoice).HasMaxLength(20);
        builder.Property(x => x.Voucher).HasMaxLength(20);
        builder.Property(x => x.RemainAmount).HasColumnType("decimal(28, 8)");
        builder.Property(x => x.CalculationAmount).HasColumnType("decimal(28, 8)");
        builder.Property(x => x.Fee).HasColumnType("decimal(28, 8)");
        builder.HasIndex(x => new { x.DataAreaId, x.CollectionLetterNum, x.CustTransId }).IsUnique();
    }
}
