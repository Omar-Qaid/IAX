using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendInvoiceInfoTableConfiguration : IEntityTypeConfiguration<VendInvoiceInfoTable>
{
    public void Configure(EntityTypeBuilder<VendInvoiceInfoTable> b)
    {
        b.ToTable("VendInvoiceInfoTable");
        b.HasKey(x => x.RecId);
        b.HasIndex(x => new { x.DataAreaId, x.ParmId }).IsUnique();

        // Per vendor, not global: APTables.md section 8.1 says not to impose uniqueness of invoice
        // numbers across all vendors and legal entities. The index serves the duplicate lookup the
        // service performs; it is not unique because a discarded pending invoice is soft-deleted
        // and must not reserve its number.
        b.HasIndex(x => new { x.DataAreaId, x.InvoiceAccount, x.Num });
    }
}

public class VendInvoiceInfoLineConfiguration : IEntityTypeConfiguration<VendInvoiceInfoLine>
{
    public void Configure(EntityTypeBuilder<VendInvoiceInfoLine> b)
    {
        b.ToTable("VendInvoiceInfoLine");
        b.HasKey(x => x.RecId);
        b.HasIndex(x => new { x.DataAreaId, x.ParmId });
        b.HasIndex(x => new { x.DataAreaId, x.ParmId, x.OrigPurchId, x.InventTransId }).IsUnique();
        b.Property(x => x.LineNum).HasPrecision(18, 6);
        b.Property(x => x.ReceiveNow).HasPrecision(18, 6);
        b.Property(x => x.PurchPrice).HasPrecision(18, 4);
        b.Property(x => x.LineAmount).HasPrecision(18, 4);
    }
}
