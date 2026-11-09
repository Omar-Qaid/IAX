using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.ProductReceipt;

public sealed class VendPackingSlipVersionConfiguration : IEntityTypeConfiguration<VendPackingSlipVersion>
{
    public void Configure(EntityTypeBuilder<VendPackingSlipVersion> builder)
    {
        builder.ToTable(nameof(VendPackingSlipVersion));
        builder.Ignore(x => x.DataAreaId);
        builder.Property(x => x.InternalPackingSlipId).HasMaxLength(FieldLengths.PackingSlipId);
        builder.Property(x => x.LedgerVoucher).HasMaxLength(FieldLengths.Voucher);
        builder.Property(x => x.ParmId).HasMaxLength(FieldLengths.ParmId);
        builder.HasIndex(x => new { x.VendPackingSlipJour, x.InternalPackingSlipId }).IsUnique();
        builder.HasOne<VendPackingSlipJour>().WithMany().HasForeignKey(x => x.VendPackingSlipJour)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
