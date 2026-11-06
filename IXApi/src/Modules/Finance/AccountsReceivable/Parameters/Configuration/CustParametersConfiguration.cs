using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustParametersConfiguration : IEntityTypeConfiguration<CustParameters>
    {
        public void Configure(EntityTypeBuilder<CustParameters> builder)
        {
            builder.ToTable("CustParameters");
            builder.HasIndex(x => new { x.DataAreaId, x.RecId }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.Key }).IsUnique();

            builder.Property(x => x.CustPostingProfile).HasMaxLength(10);
            builder.Property(x => x.CreditLimit).HasColumnType("decimal(32, 6)");
            builder.Property(x => x.PaymTermId).HasMaxLength(10);
            builder.Property(x => x.PaymMode).HasMaxLength(10);
            builder.Property(x => x.DlvMode).HasMaxLength(10);
            builder.Property(x => x.DlvReasonId).HasMaxLength(10);
            builder.Property(x => x.CurrencyCode).HasMaxLength(3);

            builder.Property(x => x.PriceDiscJournalNamePrice).HasMaxLength(10);
            builder.Property(x => x.PriceDiscJournalNameLineDisc).HasMaxLength(10);
            builder.Property(x => x.PriceDiscJournalNameMultilineDisc).HasMaxLength(10);
            builder.Property(x => x.PriceDiscJournalNameTotalDisc).HasMaxLength(10);

            builder.Property(x => x.TaxGroup).HasMaxLength(10);
            builder.Property(x => x.InvoiceJournalName).HasMaxLength(10);
            builder.Property(x => x.PackingSlipJournalName).HasMaxLength(10);
            builder.Property(x => x.CustNumSeqGroup).HasMaxLength(10);

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();
        }
    }
}
