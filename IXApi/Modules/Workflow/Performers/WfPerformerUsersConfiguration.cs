using IAX.IXApi.Modules.Workflow.Performers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerUsersConfiguration : IEntityTypeConfiguration<WfPerformerUsers>
    {
        public void Configure(EntityTypeBuilder<WfPerformerUsers> builder)
        {
            builder.ToTable("WfUsersPerformers");

            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId)
                .HasColumnName("UsersPerformerId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.PerformerId)
                .HasColumnName("PerformerID")
                .IsRequired();

            builder.Property(x => x.UserID)
                .HasColumnName("UserID")
                .IsRequired();

            builder.Property(x => x.ExtendedProperties)
                .HasColumnName("ExtendedProperties");

            builder.Property(x => x.RelatedField)
                .HasColumnName("RelatedField");

            // Ignore columns not present in WfUsersPerformers table but present in Entity<long>
            builder.Ignore(x => x.IsActive);
            builder.Ignore(x => x.RowVersion);

            // Configure relationships
            builder.HasOne(x => x.Performer)
                .WithMany()
                .HasForeignKey(x => x.PerformerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

