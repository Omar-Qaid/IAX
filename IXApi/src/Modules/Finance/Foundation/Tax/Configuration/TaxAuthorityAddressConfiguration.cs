using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxAuthorityAddressConfiguration : IEntityTypeConfiguration<TaxAuthorityAddress>
    {
        public void Configure(EntityTypeBuilder<TaxAuthorityAddress> builder)
        {
            builder.ToTable("TaxAuthorityAddress");

            // Primary Key & Alternate Key
            builder.HasKey(x => x.RecId);
            builder.HasAlternateKey(x => x.TaxAuthority);

            // Relationships
            builder.HasOne(x => x.AddressLocation)
                .WithMany()
                .HasForeignKey(x => x.Location)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VendTable)
                .WithMany()
                .HasForeignKey(x => x.AccountNum)
                .HasPrincipalKey(v => v.AccountNum)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RoundOffGainLedgerDimensionTable)
                .WithMany()
                .HasForeignKey(x => x.RoundOffGainLedgerDimension)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RoundOffLossLedgerDimensionTable)
                .WithMany()
                .HasForeignKey(x => x.RoundOffLossLedgerDimension)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
