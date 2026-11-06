using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The open (unsettled) part of a vendor transaction. One row per open transaction, which is what
/// the unique index enforces; the row is removed when the transaction is fully settled.
/// </summary>
public class VendTransOpenConfiguration : IEntityTypeConfiguration<VendTransOpen>
{
    public void Configure(EntityTypeBuilder<VendTransOpen> b)
    {
        b.ToTable("VendTransOpen");
        b.HasIndex(x => new { x.DataAreaId, x.RefRecId }).IsUnique();
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.RefRecId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
    }
}
