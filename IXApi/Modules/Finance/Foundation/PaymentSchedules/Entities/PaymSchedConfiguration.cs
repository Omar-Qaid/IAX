using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
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

    public class PaymSchedLineConfiguration : IEntityTypeConfiguration<PaymSchedLine>
    {
        public void Configure(EntityTypeBuilder<PaymSchedLine> builder)
        {
            builder.ToTable("PaymSchedLine");

            // Primary Key
            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId)
                .ValueGeneratedOnAdd();

            // Properties
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            // Relationship — FK to PaymSched via the unique Name column.
            // HasPrincipalKey implicitly creates the necessary alternate key
            // without the identifying-relationship side-effect.
            builder.HasOne(x => x.PaymSchedTable)
                .WithMany()
                .HasForeignKey(x => x.Name)
                .HasPrincipalKey(x => x.Name)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}
