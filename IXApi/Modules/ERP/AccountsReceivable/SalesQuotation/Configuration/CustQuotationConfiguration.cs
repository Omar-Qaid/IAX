using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
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

    public class CustQuotationTransConfiguration : IEntityTypeConfiguration<CustQuotationTrans>
    {
        public void Configure(EntityTypeBuilder<CustQuotationTrans> builder)
        {
            builder.ToTable("CustQuotationTrans");

            builder.Property(t => t.LineNum).HasPrecision(18, 4);
            builder.Property(t => t.PriceUnit).HasPrecision(18, 4);
            builder.Property(t => t.Qty).HasPrecision(18, 4);
            builder.Property(t => t.SalesPrice).HasPrecision(18, 4);
            builder.Property(t => t.SalesMarkup).HasPrecision(18, 4);
            builder.Property(t => t.DiscPercent).HasPrecision(18, 4);
            builder.Property(t => t.DiscAmount).HasPrecision(18, 4);
            builder.Property(t => t.LineAmount).HasPrecision(18, 4);
            builder.Property(t => t.TaxAmount).HasPrecision(18, 4);

            builder.HasIndex(t => t.QuotationId);

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);
        
   
        }
    }
}

