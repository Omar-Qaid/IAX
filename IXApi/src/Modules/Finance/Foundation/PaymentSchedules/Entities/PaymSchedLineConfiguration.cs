using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
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
