using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxOnItemConfiguration : IEntityTypeConfiguration<TaxOnItem>
    {
        public void Configure(EntityTypeBuilder<TaxOnItem> builder)
        {
            builder.ToTable("TaxOnItem");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxTable)
                .WithMany()
                .HasForeignKey(x => x.TaxCode)
                .HasPrincipalKey(t => t.TaxCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaxExemptCodeTable)
                .WithMany()
                .HasForeignKey(x => x.TaxExemptCode)
                .HasPrincipalKey(te => te.ExemptCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaxItemGroupHeadingTable)
                .WithMany()
                .HasForeignKey(x => x.TaxItemGroup)
                .HasPrincipalKey(tig => tig.TaxItemGroup)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
