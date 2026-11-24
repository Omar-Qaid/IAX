using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The open (unsettled) part of a vendor transaction: one row, or one per installment when the invoice is on a
/// payment schedule. A row is removed when it is fully settled.
/// </summary>
public class VendTransOpenConfiguration : IEntityTypeConfiguration<VendTransOpen>
{
    public void Configure(EntityTypeBuilder<VendTransOpen> b)
    {
        b.ToTable("VendTransOpen");
        // Not unique: an invoice on a payment schedule has one open row per installment (D365).
        b.HasIndex(x => new { x.DataAreaId, x.RefRecId });
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.RefRecId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
    }
}
