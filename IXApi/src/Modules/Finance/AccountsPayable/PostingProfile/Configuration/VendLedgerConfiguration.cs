using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendLedgerConfiguration : IEntityTypeConfiguration<VendLedger>
{
    public void Configure(EntityTypeBuilder<VendLedger> builder)
    {
        builder.ToTable("VendLedger");
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.DataAreaId).HasMaxLength(4).IsRequired();
        builder.HasIndex(x => new { x.DataAreaId, x.PostingProfile }).IsUnique();
    }
}
