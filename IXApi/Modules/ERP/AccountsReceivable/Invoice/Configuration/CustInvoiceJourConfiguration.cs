using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustInvoiceJourConfiguration : IEntityTypeConfiguration<CustInvoiceJour>
    {
        public void Configure(EntityTypeBuilder<CustInvoiceJour> builder)
        {
            builder.ToTable("CustInvoiceJour");

            builder.HasIndex(j => j.InvoiceId).IsUnique();
            builder.HasIndex(j => j.SalesId);
            builder.HasIndex(j => j.OrderAccount);
            builder.HasIndex(j => j.InvoiceAccount);

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);

        

            
        }
    }
}


