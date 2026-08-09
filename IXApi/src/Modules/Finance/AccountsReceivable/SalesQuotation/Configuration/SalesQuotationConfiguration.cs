using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class SalesQuotationConfiguration : IEntityTypeConfiguration<SalesQuotationTable>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationTable> builder)
        {
            builder.ToTable("SalesQuotationTable");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => new { x.DataAreaId, x.QuotationId }).IsUnique();
            builder.HasIndex(x => x.CustAccount);
            builder.HasIndex(x => x.SalesIdRef);

            builder.Property(x => x.QuotationStatus).IsRequired();

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(10)
                .HasDefaultValue("SAR")
                .IsRequired();

            builder.Property(x => x.PaymentTerms).HasMaxLength(50);

       

         

          
        
     
        }
    }
}


