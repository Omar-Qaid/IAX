using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscAdmTableConfiguration : IEntityTypeConfiguration<PriceDiscAdmTable>
    {
        public void Configure(EntityTypeBuilder<PriceDiscAdmTable> builder)
        {
            builder.ToTable("PriceDiscAdmTable");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.JournalNum }).IsUnique();

            builder.Property(x => x.JournalNum).HasMaxLength(100).IsRequired();
            builder.Property(x => x.JournalName).HasMaxLength(10);
            builder.Property(x => x.Name).HasMaxLength(150);
            builder.Property(x => x.PriceGroup).HasMaxLength(10);
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
