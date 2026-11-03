using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendBankAccountConfiguration : IEntityTypeConfiguration<VendBankAccount>
{
    public void Configure(EntityTypeBuilder<VendBankAccount> builder)
    {
        builder.HasIndex(x => new { x.DataAreaId, x.VendAccount, x.AccountID }).IsUnique();
        builder.HasOne<VendTable>().WithMany().HasForeignKey(x => x.VendAccount)
            .HasPrincipalKey(x => x.AccountNum).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
    }
}
