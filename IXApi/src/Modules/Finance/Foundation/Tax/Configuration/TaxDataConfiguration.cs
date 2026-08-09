using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxDataConfiguration : IEntityTypeConfiguration<TaxData>
    {
        public void Configure(EntityTypeBuilder<TaxData> builder)
        {
            builder.ToTable("TaxData");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxTable)
                .WithMany()
                .HasForeignKey(x => x.TaxCode)
                .HasPrincipalKey(t => t.TaxCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
