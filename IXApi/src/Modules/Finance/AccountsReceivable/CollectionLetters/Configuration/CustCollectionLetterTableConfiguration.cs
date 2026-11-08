using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterTableConfiguration : IEntityTypeConfiguration<CustCollectionLetterTable>
{
    public void Configure(EntityTypeBuilder<CustCollectionLetterTable> builder)
    {
        builder.ToTable("CustCollectionLetterTable");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CollectionLetterCourse).HasMaxLength(10);
        builder.Property(x => x.Name).HasMaxLength(60);
        builder.Property(x => x.PaymTermId).HasMaxLength(10);
        builder.HasIndex(x => new { x.DataAreaId, x.CollectionLetterCourse }).IsUnique();
    }
}
