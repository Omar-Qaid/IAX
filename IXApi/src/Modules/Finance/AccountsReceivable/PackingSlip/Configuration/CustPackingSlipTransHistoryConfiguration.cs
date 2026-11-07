using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

public sealed class CustPackingSlipTransHistoryConfiguration : IEntityTypeConfiguration<CustPackingSlipTransHistory>
{
    public void Configure(EntityTypeBuilder<CustPackingSlipTransHistory> builder)
    {
        builder.ToTable(nameof(CustPackingSlipTransHistory));
        builder.HasIndex(x => new { x.CustPackingSlipTrans, x.ValidFrom }).IsUnique();
        builder.HasOne(x => x.PackingSlipLine).WithMany().HasForeignKey(x => x.CustPackingSlipTrans)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
