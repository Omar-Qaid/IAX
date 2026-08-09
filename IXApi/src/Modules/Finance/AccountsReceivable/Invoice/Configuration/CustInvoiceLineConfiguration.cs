using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustInvoiceLineConfiguration : IEntityTypeConfiguration<CustInvoiceLine>
    {
        public void Configure(EntityTypeBuilder<CustInvoiceLine> builder)
        {
            builder.ToTable("CustInvoiceLine");

            builder.Property(t => t.LineNum).HasPrecision(18, 4);
            builder.Property(t => t.Quantity).HasPrecision(18, 4);
            builder.Property(t => t.UnitPrice).HasPrecision(18, 4);
            builder.Property(t => t.AmountCur).HasPrecision(18, 4);
            builder.Property(t => t.TaxAmount).HasPrecision(18, 4);

            builder.HasIndex(t => t.ParentRecId);

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

       

      
        }
    }
}