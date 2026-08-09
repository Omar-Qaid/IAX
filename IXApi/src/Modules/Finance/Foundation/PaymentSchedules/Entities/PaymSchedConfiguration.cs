using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class PaymSchedConfiguration : IEntityTypeConfiguration<PaymSched>
    {
        public void Configure(EntityTypeBuilder<PaymSched> builder)
        {
            builder.ToTable("PaymSched");

            // Primary Key
            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId)
                .ValueGeneratedOnAdd();

            // Properties
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(200)
                .IsRequired();

            // Unique business key (index only, NOT HasAlternateKey —
            // HasAlternateKey causes EF Core to treat FK references to Name
            // as identifying relationships, blocking key-property mutation on dependents).
            builder.HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}
