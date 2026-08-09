using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxPeriodHeadConfiguration : IEntityTypeConfiguration<TaxPeriodHead>
    {
        public void Configure(EntityTypeBuilder<TaxPeriodHead> builder)
        {
            builder.ToTable("TaxPeriodHead");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxAuthorityAddressTable)
                .WithMany()
                .HasForeignKey(x => x.TaxAuthority)
                .HasPrincipalKey(a => a.TaxAuthority)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
