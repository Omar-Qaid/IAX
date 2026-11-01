using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice
{
    public class CustInvoiceJourConfiguration : IEntityTypeConfiguration<CustInvoiceJour>
    {
        public void Configure(EntityTypeBuilder<CustInvoiceJour> builder)
        {
            builder.ToTable("CustInvoiceJour");

            builder.HasIndex(j => new { j.DataAreaId, j.InvoiceId }).IsUnique();
            builder.HasIndex(j => new { j.DataAreaId, j.SalesId });
            builder.HasIndex(j => new { j.DataAreaId, j.OrderAccount });
            builder.HasIndex(j => new { j.DataAreaId, j.InvoiceAccount });

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);

        

            
        }
    }
}



