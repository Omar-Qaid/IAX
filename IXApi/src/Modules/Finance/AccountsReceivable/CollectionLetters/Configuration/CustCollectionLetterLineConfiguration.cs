using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterLineConfiguration : IEntityTypeConfiguration<CustCollectionLetterLine>
{
    public void Configure(EntityTypeBuilder<CustCollectionLetterLine> builder)
    {
        builder.ToTable("CustCollectionLetterLine");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CollectionLetterCourse).HasMaxLength(10);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.MinAmount).HasColumnType("decimal(28, 8)");
        builder.Property(x => x.Fee).HasColumnType("decimal(28, 8)");
        builder.Property(x => x.TaxItemGroup).HasMaxLength(10);
        builder.Property(x => x.Txt).HasMaxLength(60);
        builder.HasIndex(x => new { x.DataAreaId, x.CollectionLetterCourse, x.CurrencyCode, x.CollectionLetterCode }).IsUnique();
    }
}
