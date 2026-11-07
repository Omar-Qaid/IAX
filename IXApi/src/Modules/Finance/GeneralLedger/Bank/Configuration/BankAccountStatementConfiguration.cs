using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.GeneralLedger;

public sealed class BankAccountStatementConfiguration : IEntityTypeConfiguration<BankAccountStatement>
{
    public void Configure(EntityTypeBuilder<BankAccountStatement> builder)
    {
        builder.Property(x => x.EndingBalance).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.DataAreaId, x.AccountId, x.AccountStatementNum }).IsUnique();
    }
}
