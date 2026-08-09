using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustLedgerConfiguration : IEntityTypeConfiguration<CustLedger>
    {
        public void Configure(EntityTypeBuilder<CustLedger> builder)
        {
            builder.ToTable("CustLedger");

            builder.HasIndex(t => t.PostingProfile).IsUnique();

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
        }
    }
}