using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustInvoiceTableConfiguration : IEntityTypeConfiguration<CustInvoiceTable>
    {
        public void Configure(EntityTypeBuilder<CustInvoiceTable> builder)
        {
            builder.ToTable("CustInvoiceTable");

            builder.Property(t => t.CashDiscPercent).HasPrecision(18, 4);

            builder.HasIndex(t => t.InvoiceId).IsUnique();
            builder.HasIndex(t => t.OrderAccount);
            builder.HasIndex(t => t.InvoiceDate);
       

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();


     
        }
    }
}