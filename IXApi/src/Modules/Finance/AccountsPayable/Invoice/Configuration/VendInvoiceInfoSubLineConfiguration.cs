using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.AccountsPayable;
public sealed class VendInvoiceInfoSubLineConfiguration : IEntityTypeConfiguration<VendInvoiceInfoSubLine>
{
    public void Configure(EntityTypeBuilder<VendInvoiceInfoSubLine> b)
    {
        b.HasOne<VendInvoiceInfoLine>().WithMany().HasForeignKey(x => x.LineRefRecId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DataAreaId, x.ParmId, x.LineRefRecId });
        b.HasIndex(x => new { x.JournalRefTableId, x.JournalRefRecId });
        b.Property(x => x.ReceiveNow).HasPrecision(28, 12);
        b.Property(x => x.InventNow).HasPrecision(28, 12);
    }
}
