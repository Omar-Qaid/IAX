using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendPaymModeConfiguration : IEntityTypeConfiguration<VendPaymModeTable>
    {
        public void Configure(EntityTypeBuilder<VendPaymModeTable> builder)
        {
            builder.ToTable("VendPaymModeTable");
            builder.HasIndex(x => x.DataAreaId);
            builder.HasIndex(x => new { x.DataAreaId, x.PaymMode }).IsUnique();

            builder.Property(x => x.PaymMode).HasMaxLength(FieldLengths.PaymModeId).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(FieldLengths.Name);
            builder.Property(x => x.PaymJournalNameId).HasMaxLength(FieldLengths.JournalNameId);
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
