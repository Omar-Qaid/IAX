using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendLedgerAccountsConfiguration : IEntityTypeConfiguration<VendLedgerAccounts>
{
    public void Configure(EntityTypeBuilder<VendLedgerAccounts> builder)
    {
        builder.ToTable("VendLedgerAccounts");
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.DataAreaId).HasMaxLength(4).IsRequired();
        builder.Property(x => x.PostingProfile).HasMaxLength(FieldLengths.PostingProfile).IsRequired();
        builder.Property(x => x.Num).HasMaxLength(FieldLengths.Num).IsRequired();

        // Num stores a vendor account for Table, a vendor group for Group, or no value for All.
        // These alternatives cannot share one foreign key.
    }
}
