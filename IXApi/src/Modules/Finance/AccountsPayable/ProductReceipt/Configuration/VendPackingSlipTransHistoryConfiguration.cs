using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.ProductReceipt;

public sealed class VendPackingSlipTransHistoryConfiguration : IEntityTypeConfiguration<VendPackingSlipTransHistory>
{
    public void Configure(EntityTypeBuilder<VendPackingSlipTransHistory> builder)
    {
        builder.ToTable(nameof(VendPackingSlipTransHistory));
        builder.HasIndex(x => new { x.VendPackingSlipTrans, x.ValidFrom }).IsUnique();
        builder.HasOne(x => x.PackingSlipLine).WithMany().HasForeignKey(x => x.VendPackingSlipTrans)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
