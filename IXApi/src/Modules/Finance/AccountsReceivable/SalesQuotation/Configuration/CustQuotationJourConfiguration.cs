using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustQuotationJourConfiguration : IEntityTypeConfiguration<CustQuotationJour>
    {
        public void Configure(EntityTypeBuilder<CustQuotationJour> builder)
        {
            builder.ToTable("CustQuotationJour");

            builder.Property(t => t.CashDiscPercent).HasPrecision(18, 4);
            builder.Property(t => t.Qty).HasPrecision(18, 4);
            builder.Property(t => t.QuotationAmount).HasPrecision(18, 4);
            builder.Property(t => t.ExchRate).HasPrecision(18, 4);
            builder.Property(t => t.SumTax).HasPrecision(18, 4);

            builder.HasIndex(t => t.QuotationId).IsUnique();
            builder.HasIndex(t => t.OrderAccount);
            builder.HasIndex(t => t.InvoiceAccount);

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);

        
        }
    }
}