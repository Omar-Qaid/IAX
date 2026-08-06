 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustInvoiceTransConfiguration : IEntityTypeConfiguration<CustInvoiceTrans>
    {
        public void Configure(EntityTypeBuilder<CustInvoiceTrans> builder)
        {
            builder.ToTable("CustInvoiceTrans");

            builder.Property(t => t.LineNum).HasPrecision(18, 4);
            builder.Property(t => t.Qty).HasPrecision(18, 4);
            builder.Property(t => t.SalesPrice).HasPrecision(18, 4);
            builder.Property(t => t.LineAmount).HasPrecision(18, 4);

            builder.HasIndex(t => t.InvoiceId);
            builder.HasIndex(t => t.SalesId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.InventTransId);

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(10)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.DataAreaId);

         

        }
    }
}



