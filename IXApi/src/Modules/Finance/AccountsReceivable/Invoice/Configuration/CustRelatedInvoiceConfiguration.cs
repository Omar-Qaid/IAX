using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed class CustRelatedInvoiceConfiguration : IEntityTypeConfiguration<CustRelatedInvoice>
{
    public void Configure(EntityTypeBuilder<CustRelatedInvoice> builder)
    {
        builder.ToTable("CustRelatedInvoice");
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.HasIndex(x => new { x.DataAreaId, x.ParentCustInvoice, x.InvoiceAssociationType });
        builder.HasIndex(x => new { x.DataAreaId, x.CustInvoice });
        builder.HasOne<CustInvoiceTable>().WithMany().HasForeignKey(x => x.CustInvoice)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CustInvoiceTable>().WithMany().HasForeignKey(x => x.OriginalCustInvoice)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CustInvoiceTable>().WithMany().HasForeignKey(x => x.ParentCustInvoice)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
