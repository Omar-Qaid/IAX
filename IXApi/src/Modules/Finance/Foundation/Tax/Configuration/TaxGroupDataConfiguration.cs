using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxGroupDataConfiguration : IEntityTypeConfiguration<TaxGroupData>
    {
        public void Configure(EntityTypeBuilder<TaxGroupData> builder)
        {
            builder.ToTable("TaxGroupData");

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

            builder.HasOne(x => x.TaxGroupHeadingTable)
                .WithMany()
                .HasForeignKey(x => x.TaxGroup)
                .HasPrincipalKey(tg => tg.TaxGroup)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
