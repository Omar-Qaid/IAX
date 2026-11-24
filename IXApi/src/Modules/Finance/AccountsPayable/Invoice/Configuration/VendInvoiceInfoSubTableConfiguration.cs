using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.AccountsPayable;
public sealed class VendInvoiceInfoSubTableConfiguration : IEntityTypeConfiguration<VendInvoiceInfoSubTable>
{
    public void Configure(EntityTypeBuilder<VendInvoiceInfoSubTable> b)
    {
        b.HasIndex(x => new { x.DataAreaId, x.ParmId, x.OrigPurchId }).IsUnique();
    }
}
