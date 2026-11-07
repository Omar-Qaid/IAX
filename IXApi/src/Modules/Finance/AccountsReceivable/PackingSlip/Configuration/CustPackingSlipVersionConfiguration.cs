using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

public sealed class CustPackingSlipVersionConfiguration : IEntityTypeConfiguration<CustPackingSlipVersion>
{
    public void Configure(EntityTypeBuilder<CustPackingSlipVersion> builder)
    {
        builder.ToTable(nameof(CustPackingSlipVersion));
        builder.Ignore(x => x.DataAreaId);
        builder.Property(x => x.InternalPackingSlipId).HasMaxLength(FieldLengths.PackingSlipId);
        builder.Property(x => x.LedgerVoucher).HasMaxLength(FieldLengths.Voucher);
        builder.Property(x => x.ParmId).HasMaxLength(FieldLengths.ParmId);
        builder.HasIndex(x => new { x.CustPackingSlipJour, x.InternalPackingSlipId }).IsUnique();
        builder.HasOne<CustPackingSlipJour>().WithMany().HasForeignKey(x => x.CustPackingSlipJour)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
