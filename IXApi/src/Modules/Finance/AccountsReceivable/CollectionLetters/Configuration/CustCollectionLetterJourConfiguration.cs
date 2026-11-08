using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterJourConfiguration : IEntityTypeConfiguration<CustCollectionLetterJour>
{
    public void Configure(EntityTypeBuilder<CustCollectionLetterJour> builder)
    {
        builder.ToTable("CustCollectionLetterJour");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CollectionLetterNum).HasMaxLength(20);
        builder.Property(x => x.AccountNum).HasMaxLength(20);
        builder.Property(x => x.CollectionLetterDate).HasColumnType("date");
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.PostingProfile).HasMaxLength(10);
        builder.Property(x => x.Fee).HasColumnType("decimal(28, 8)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(28, 8)");
        builder.Property(x => x.LedgerVoucher).HasMaxLength(20);
        builder.Property(x => x.PrintDate).HasColumnType("date");
        builder.Property(x => x.Updated).HasColumnType("date");
        builder.Property(x => x.Canceled).HasColumnType("date");
        builder.Property(x => x.PaymTermId).HasMaxLength(10);
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.HasIndex(x => new { x.DataAreaId, x.CollectionLetterNum }).IsUnique();
    }
}
