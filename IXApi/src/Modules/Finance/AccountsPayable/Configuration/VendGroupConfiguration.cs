using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.Configuration
{
    public class VendGroupConfiguration : IEntityTypeConfiguration<VendGroup>
    {
        public void Configure(EntityTypeBuilder<VendGroup> builder)
        {
            builder.ToTable("VendGroup");

            // Primary Key & Alternate Key
            builder.HasKey(x => x.RecId);
            builder.HasAlternateKey(x => x.VendGroupCode);

            // ==========================================================
            // Vendor Group -> Payment Terms
            // VendGroup.PaymTermId -> PaymTerm.PaymTermId
            // ==========================================================
            builder.HasOne(x => x.PaymTermTable)
                   .WithMany()
                   .HasForeignKey(x => x.PaymTermId)
                   .HasPrincipalKey(pt => pt.PaymTermId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new
            {
                x.VendGroupCode,
                x.DataAreaId
            }).IsUnique();
        }
    }
}
