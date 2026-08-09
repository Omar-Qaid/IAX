using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
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